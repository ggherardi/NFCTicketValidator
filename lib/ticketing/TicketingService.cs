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
        private string _password;
        private byte[] _cardID;
        private NFCReader _nfcReader;
        private SmartTicket _ticket;
        private IValidatorLocation _location;

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

        public void ValidateTicket()
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                if (_ticket.SessionValidation == null)
                {
                    ResetTicketValidation();
                }
                else
                {
                    TimeSpan timeSinceSessionValidation = timeStamp - (DateTime)_ticket.SessionValidation;
                    if (timeSinceSessionValidation.Minutes < _ticket.Type.DurationInMinutes)
                    {
                        // ticket still valid management
                    }
                    else if (timeSinceSessionValidation.Minutes > _ticket.Type.NextTicketUpgrade.DurationInMinutes)
                    {
                        // Ticket is expired for both the current ticket type and the upgraded ticket type
                        ResetTicketValidation();
                    }
                    else
                    {
                        if((timeStamp - (DateTime)_ticket.CurrentValidation).Minutes < SmartTicketType.BIT.DurationInMinutes)
                        {
                            // ticket still valid management
                        }                        
                        else if (_ticket.SessionExpense + _ticket.Type.Cost >= _ticket.Type.NextTicketUpgrade.Cost)
                        {
                            // Upgrade the ticket since it would be more cost efficient than buying a new base ticket
                            UpgradeTicket();
                        }
                    }                        
                }                               
                WriteTicket();
                _ticket = ReadTicket();
            }
            catch(Exception ex)
            {
                
            }
        }

        private void ResetTicketValidation()
        {
            ChargeTicket(SmartTicketType.BIT);
            DateTime timestamp = DateTime.Now;
            _ticket.CurrentValidation = timestamp;
            _ticket.SessionValidation = timestamp;
        }

        private void UpgradeTicket()
        {
            double expense = _ticket.Type.NextTicketUpgrade.Cost - _ticket.SessionExpense;
            _ticket.Credit -= expense;
            _ticket.SessionExpense += expense;
            _ticket.Type = (SmartTicketType)_ticket.Type.NextTicketUpgrade;
            
        }

        private void ChargeTicket(SmartTicketType chargeType)
        {
            if(_ticket.Credit - chargeType.Cost < 0)
            {
                throw new Exception("Insufficient credit.");
            }
            _ticket.Credit -= chargeType.Cost;
            _ticket.SessionExpense += chargeType.Cost;
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
