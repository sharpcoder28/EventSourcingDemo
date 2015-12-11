using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcingDemo.Domain
{
    public class Command : Message
    {
    }

    public class CreateShipment : Command
    {
        public readonly Guid ShipmentID;
        public readonly decimal Weight;
        public readonly int Quantity;
        public readonly decimal FreightClass;

        public CreateShipment(Guid shipmentID, decimal weight, int quantity, decimal freightClass)
        {
            ShipmentID = shipmentID;
            Weight = weight;
            Quantity = quantity;
            FreightClass = freightClass;
        }
    }
}
