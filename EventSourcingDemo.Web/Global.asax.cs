using EventSourcingDemo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EventSourcingDemo.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var bus = new FakeBus();

            var storage = new EventStore(bus);
            var rep = new Repository<Shipment>(storage);
            var commands = new ShipmentCommandHandlers(rep);
            bus.RegisterHandler<CreateShipment>(commands.Handle);




            ServiceLocator.Bus = bus;
        }
    }
}
