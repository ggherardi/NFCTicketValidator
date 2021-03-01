using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ticketing
{
    public class SmartTicket
    {
        public double Credit { get; set; }
        public SmartTicketType Type { get; set; }        
        public DateTime CurrentValidation { get; set; }
        public DateTime SessionValidation { get; set; }
        public double SessionExpense { get;  set; }
        public byte[] CardID { get; set; }        

        public override string ToString() => $"CardID: {CardID}, Type: {Type?.Name}, Credit: {Credit}";
    }

    public abstract class TicketEnumeration
    {        
        public string Name { get; private set; }
        [JsonIgnore]
        public double Cost { get; private set; }
        [JsonIgnore]
        public int DurationInMinutes { get; private set; }

        protected TicketEnumeration(string name, double cost, int durationInMinutes) => (Name, Cost, DurationInMinutes) = (name, cost, durationInMinutes);
        protected TicketEnumeration() { }
    }

    public class SmartTicketType : TicketEnumeration
    {
        public static SmartTicketType BIT = new SmartTicketType("BIT", 1.5, 100);
        public static SmartTicketType ROMA24 = new SmartTicketType("ROMA24", 7, 1440);
        public static SmartTicketType ROMA48 = new SmartTicketType("ROMA48", 12.50, 2880);
        public static SmartTicketType ROMA72 = new SmartTicketType("ROMA72", 18, 4320);
        public static SmartTicketType CIS = new SmartTicketType("CIS", 24, 10080);

        public SmartTicketType(string name, double cost, int durationInMinutes) : base(name, cost, durationInMinutes) { }
        public SmartTicketType() { }
    }
}
