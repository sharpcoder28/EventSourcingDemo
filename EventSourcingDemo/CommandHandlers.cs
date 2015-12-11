using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcingDemo
{
    public class ShipmentCommandHandlers
    {
        public void Handle(ShipmentCreated message)
        {
            //var shipment = new Shipment(message, message.Name);

            //_repository.Save(item, -1);
        }
    }
}
