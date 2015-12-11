using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcingDemo
{
    public class Event : Message
    {
        public int Version;
    }

    public class ShipmentCreated : Event
    {
        public readonly Guid Id;
        public readonly decimal Weight;
        public readonly int Quantity;
        public readonly decimal FreightClass;
        public ShipmentCreated(Guid id, decimal weight, int quantity, decimal freightClass)
        {
            Id = id;
            Weight = weight;
            Quantity = quantity;
            FreightClass = freightClass;
        }
    }
}
