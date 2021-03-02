using CSharp.NFC;
using CSharp.NFC.NDEF;
using CSharp.NFC.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Encryption;

namespace Ticketing
{
    public class TicketingService
    {
        string _password;
        byte[] _cardID;
        NFCReader _nfcReader;
        SmartTicket _ticket;
        public SmartTicket ConnectedTicket { get => _ticket; private set => _ticket = value; }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, string password) 
        {
            _cardID = cardID;
            _nfcReader = ticketValidator;
            _password = password;
        }

        public TicketingService(NFCReader ticketValidator, byte[] cardID) : this(ticketValidator, cardID, string.Empty) { }

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

        /// <summary>
        /// 
        /// </summary>
        public void ResetTicket()
        {
            _ticket = new SmartTicket() { Credit = 0, Type = SmartTicketType.BIT, CurrentValidation = null, SessionValidation = null, SessionExpense = 0, CardID = _cardID };
            WriteTicket();
        }

        public void ValidateTicket()
        {
            //if (_ticket.CurrentValidation == DateTime.Now.AddSeconds())
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
