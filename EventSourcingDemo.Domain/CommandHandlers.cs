using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcingDemo.Domain
{
    public class ShipmentCommandHandlers
    {
        private readonly IRepository<Shipment> _repository;

        public ShipmentCommandHandlers(IRepository<Shipment> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateShipment message)
        {
            var shipment = new Shipment(message.ShipmentID, message.Weight, message.Quantity, message.FreightClass);

            _repository.Save(shipment, -1);
        }
    }
}
