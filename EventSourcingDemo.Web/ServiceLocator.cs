using EventSourcingDemo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventSourcingDemo.Web
{
    public static class ServiceLocator
    {
        public static FakeBus Bus { get; set; }

    }
}