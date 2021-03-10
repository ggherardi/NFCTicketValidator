﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public class SmartTicket
    {
        public double Credit { get; set; }        
        public string TicketTypeName { get => Type.Name; set => Type = TicketEnumeration.GetAll<SmartTicketType>().FirstOrDefault(t => t.Name == value); }
        [JsonIgnore]
        public SmartTicketType Type { get; set; }        
        public DateTime? CurrentValidation { get; set; }
        public DateTime? SessionValidation { get; set; }
        public double SessionExpense { get;  set; }
        public byte[] CardID { get; set; }        

        public override string ToString() => $"CardID: {(CardID != null ? BitConverter.ToString(CardID) : string.Empty)}, Type: {Type?.Name}, Credit: {Credit}, CurrentValidation: {CurrentValidation?.ToString("g")}, SessionExpense: {SessionExpense}";
    }

    public abstract class TicketEnumeration
    {        
        public string Name { get; set; }
        [JsonIgnore]
        public double Cost { get; set; }
        [JsonIgnore]
        public int DurationInMinutes { get; set; }
        [JsonIgnore]
        public TicketEnumeration NextTicketUpgrade { get; set; }

        protected TicketEnumeration(string name, double cost, int durationInMinutes, TicketEnumeration nextTicketUpgrade = null) => (Name, Cost, DurationInMinutes, NextTicketUpgrade) = (name, cost, durationInMinutes, nextTicketUpgrade);
        protected TicketEnumeration() { }

        public static IEnumerable<T> GetAll<T>() where T : TicketEnumeration => typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(f => f.GetValue(null)).Cast<T>();
    }

    public class SmartTicketType : TicketEnumeration
    {
        public static SmartTicketType CIS = new SmartTicketType("CIS", 24, 10080);
        public static SmartTicketType ROMA72 = new SmartTicketType("ROMA72", 18, 4320, CIS);
        public static SmartTicketType ROMA48 = new SmartTicketType("ROMA48", 12.50, 2880, ROMA72);
        public static SmartTicketType ROMA24 = new SmartTicketType("ROMA24", 7, 1440, ROMA48);
        public static SmartTicketType BIT = new SmartTicketType("BIT", 1.5, 100, ROMA24);

        public SmartTicketType(string name, double cost, int durationInMinutes, TicketEnumeration nextTicketUpgrade = null) : base(name, cost, durationInMinutes, nextTicketUpgrade) { }
        public SmartTicketType() { }
    }
}
