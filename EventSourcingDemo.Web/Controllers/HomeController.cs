using EventSourcingDemo.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EventSourcingDemo.Domain;

namespace EventSourcingDemo.Web.Controllers
{
    public class HomeController : Controller
    {
        private FakeBus _bus;
        //private ReadModelFacade _readmodel;

        public HomeController()
        {
            _bus = ServiceLocator.Bus;
            //_readmodel = new ReadModelFacade();
        }

        // GET: Home
        public ActionResult Index()
        {
            return View(new List<ShipmentModel>());
        }

        public ActionResult Create()
        {
            

            return View();
        }

        [HttpPost]
        public ActionResult Create(ShipmentModel shipmentVm)
        {
            var command = new CreateShipment(Guid.NewGuid(), shipmentVm.Weight, shipmentVm.Quantity, shipmentVm.FreightClass);

            _bus.Send(command);

            return View("Index");
        }
    }
}