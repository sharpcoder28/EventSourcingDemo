using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventSourcingDemo.Web.Models
{
    public class ShipmentModel
    {
        public decimal FreightClass { get; set; }

        public decimal Weight { get; set; }

        public int Quantity { get; set; }

        public string Description { get; set; }
    }
}