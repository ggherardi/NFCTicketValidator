using CSharp.NFC;
using CSharp.NFC.NDEF;
using CSharp.NFC.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFCTicketing.Encryption;

namespace NFCTicketing
{
    public class TicketingService
    {
        private readonly string _password;
        private readonly byte[] _cardID;
        private readonly NFCReader _nfcReader;
        private readonly IValidatorLocation _location;
        private readonly IValidationStorage _storage;
        private EncryptableSmartTicket _ticket;
        private DateTime _timestamp;
        private string _encryptedTicketHash;

        public EncryptableSmartTicket ConnectedTicket { get => _ticket; private set => _ticket = value; }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location, IValidationStorage storage, string password) 
        {
            _cardID = cardID;
            _nfcReader = ticketValidator;
            _location = location;
            _password = password;
            _storage = storage ?? new LocalDBStorage();
        }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location) : this(ticketValidator, cardID, location, null, string.Empty) { }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location, IValidationStorage storage) : this(ticketValidator, cardID, location, storage, string.Empty) { }

        public void ConnectTicket()
        {
            _ticket = ReadTicket();
        }

        /// <summary>
        /// Adds the specified amount to the credit of the connected ticket
        /// </summary>
        /// <param name="creditAmount"></param>
        public void AddCredit(decimal creditAmount)
        {
            _ticket.Credit += creditAmount;
            WriteTicket();
            RegisterTransaction((decimal)creditAmount);
        }

        public void InitNewTicket()
        {
            _ticket = new EncryptableSmartTicket() { Credit = 0, TicketTypeName = SmartTicketType.BIT.Name, CurrentValidation = null, SessionValidation = null, SessionExpense = 0, UsageTimestamp = DateTime.Now, CardID = _cardID };
            WriteTicket();
        }

        /// <summary>
        /// Main business logic, check flowchart.png
        /// </summary>
        public void ValidateTicket()
        {            
            try
            {
                _timestamp = DateTime.Now;
                _ticket.UsageTimestamp = _timestamp;
                if (_ticket.SessionValidation == null)
                {
                    ResetTicketValidation();
                }
                else
                {
                    TimeSpan timeSinceFirstValidation = _timestamp - (DateTime)_ticket.SessionValidation;
                    if (timeSinceFirstValidation.TotalMinutes < _ticket.Type.DurationInMinutes)
                    {
                        ManageValidTicket();
                    }
                    else if (_ticket.Type.NextTicketUpgrade == null || timeSinceFirstValidation.TotalMinutes > _ticket.Type.NextTicketUpgrade.DurationInMinutes)
                    {
                        // Ticket is expired for both the current ticket type and the upgraded ticket type
                        ResetTicketValidation();
                    }
                    else
                    {
                        if((_timestamp - (DateTime)_ticket.CurrentValidation).TotalMinutes < SmartTicketType.BIT.DurationInMinutes)
                        {
                            ManageValidTicket();
                        }                        
                        else
                        {
                            if (_ticket.Type.NextTicketUpgrade != null && _ticket.SessionExpense + SmartTicketType.BIT.Cost >= _ticket.Type.NextTicketUpgrade.Cost)
                            {
                                // Upgrade the ticket since it would be more cost efficient than buying a new base ticket
                                UpgradeTicket();
                            }
                            else
                            {
                                ValidateBaseTicket();
                            }
                        }
                    }                        
                }                               
                WriteTicket();
                _ticket = ReadTicket();
                RegisterTicketUpdate();
            }
            catch(Exception)
            {
                
            }
        }

        private void ResetTicketValidation()
        {
            _ticket.SessionExpense = 0;
            ChargeTicket(SmartTicketType.BIT.Cost);
            _ticket.CurrentValidation = _timestamp;
            _ticket.SessionValidation = _timestamp;            
            RegisterValidation();
        }

        private void UpgradeTicket()
        {
            decimal upgradeCost = _ticket.Type.NextTicketUpgrade.Cost - (decimal)_ticket.SessionExpense;
            ChargeTicket(upgradeCost);
            _ticket.Type = (SmartTicketType)_ticket.Type.NextTicketUpgrade;            
        }

        private void ManageValidTicket()
        {
            TimeSpan timeSinceLastValidation = _timestamp - (DateTime)_ticket.CurrentValidation;
            if (timeSinceLastValidation.TotalMinutes > SmartTicketType.BIT.DurationInMinutes)
            {
                _ticket.CurrentValidation = _timestamp;
                RegisterValidation();
            }
        }

        private void ValidateBaseTicket()
        {
            ChargeTicket(SmartTicketType.BIT.Cost);
            _ticket.CurrentValidation = _timestamp;
            RegisterValidation();
        }

        private void ChargeTicket(decimal amount)
        {
            if (_ticket.Credit - amount < 0)
            {
                throw new Exception("Insufficient credit.");
            }
            _ticket.Credit -= amount;
            _ticket.SessionExpense += amount;
        }

        private void RegisterValidation()
        {
            _storage.RegisterValidation(new TicketValidation() { CardId = _ticket.CardID, Location = _location.GetLocation(), Time = _timestamp, EncryptedTicketHash = _encryptedTicketHash });
        }

        private void RegisterTicketUpdate()
        {
            _storage.RegisterTicketUpdate(_ticket);
        }

        private void RegisterTransaction(decimal amount)
        {
            _storage.RegisterTransaction(new CreditTransaction() { CardId = _ticket.CardID, Location = _location.GetLocation(), Date = DateTime.Now, Amount = amount });
        }

        public EncryptableSmartTicket ReadTicket()
        {
            NDEFPayload payload = _nfcReader.GetNDEFPayload();
            EncryptableSmartTicket ticket = TicketEncryption.DecryptTicket<EncryptableSmartTicket>(payload.Bytes, TicketEncryption.GetPaddedIV(_cardID));
            _encryptedTicketHash = Encoding.Unicode.GetString(payload.Bytes);
            return ticket;
        }

        private List<NFCOperation> WriteTicket()
        {
            // Sample ticket
            //SmartTicket ticket = new SmartTicket() { Credit = 10.5, Type = SmartTicketType.BIT, CurrentValidation = DateTime.Now, SessionValidation = DateTime.Now.AddHours(-10), SessionExpense = 3.0, CardID = new byte[] { 0x04, 0x15, 0x91, 0x8A, 0xCB, 0x42, 0x20 } };
            byte[] encryptedTicketBytes = TicketEncryption.EncryptTicket(_ticket, TicketEncryption.GetPaddedIV(_cardID));
            List<NFCOperation> operations = _nfcReader.WriteTextNDEFMessage(encryptedTicketBytes, _password);
            return operations;
        }
    }
}
