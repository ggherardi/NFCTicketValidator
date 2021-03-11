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
        private SmartTicket _ticket;
        private DateTime _timestamp;

        public SmartTicket ConnectedTicket { get => _ticket; private set => _ticket = value; }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location, string password) 
        {
            _cardID = cardID;
            _nfcReader = ticketValidator;
            _location = location;
            _password = password;
        }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location) : this(ticketValidator, cardID, location, string.Empty) { }

        public void ConnectTicket()
        {
            NDEFPayload payload = _nfcReader.GetNDEFPayload();
            if (payload != null)
            {
                _ticket = TicketEncryption.DecryptTicket(payload.Bytes, TicketEncryption.GetPaddedIV(_cardID));
            }
        }

        /// <summary>
        /// Adds the specified amount to the credit of the connected ticket
        /// </summary>
        /// <param name="creditAmount"></param>
        public void AddCredit(double creditAmount)
        {
            _ticket.Credit += creditAmount;
            WriteTicket();
        }

        public void InitNewTicket()
        {
            _ticket = new SmartTicket() { Credit = 0, TicketTypeName = SmartTicketType.BIT.Name, CurrentValidation = null, SessionValidation = null, SessionExpense = 0, CardID = _cardID };
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
                                ChargeTicket(SmartTicketType.BIT.Cost);
                                _ticket.CurrentValidation = _timestamp;
                            }
                        }
                    }                        
                }                               
                WriteTicket();
                _ticket = ReadTicket();
            }
            catch(Exception)
            {
                
            }
        }

        private void ResetTicketValidation()
        {
            if(_timestamp == null)
            {
                _timestamp = DateTime.Now;
            }
            ChargeTicket(SmartTicketType.BIT.Cost);
            _ticket.CurrentValidation = _timestamp;
            _ticket.SessionValidation = _timestamp;
        }

        private void UpgradeTicket()
        {
            double upgradeCost = _ticket.Type.NextTicketUpgrade.Cost - _ticket.SessionExpense;
            ChargeTicket(upgradeCost);
            _ticket.Type = (SmartTicketType)_ticket.Type.NextTicketUpgrade;            
        }

        private void ManageValidTicket()
        {
            TimeSpan timeSinceLastValidation = _timestamp - (DateTime)_ticket.CurrentValidation;
            if (timeSinceLastValidation.TotalMinutes > SmartTicketType.BIT.DurationInMinutes)
            {
                _ticket.CurrentValidation = _timestamp;
            }
        }

        private void ChargeTicket(double amount)
        {
            if (_ticket.Credit - amount < 0)
            {
                throw new Exception("Insufficient credit.");
            }
            _ticket.Credit -= amount;
            _ticket.SessionExpense += amount;
        }

        public SmartTicket ReadTicket()
        {
            NDEFPayload payload = _nfcReader.GetNDEFPayload();
            SmartTicket ticket = TicketEncryption.DecryptTicket(payload.Bytes, TicketEncryption.GetPaddedIV(_cardID));
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
