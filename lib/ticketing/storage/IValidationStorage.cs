using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public interface IValidationStorage
    {
        void RegisterValidation(TicketValidation validation);
        void RegisterTicketUpdate(EncryptableSmartTicket ticket);
        void RegisterTransaction(CreditTransaction transaction);
    }
}
