using EventSourcingDemo.Domain;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
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


            CreateTopic();

            var bus = new FakeBus();

            // Set up service bus
            var eventPublisher = new AzureEventPublisher();

            //var storage = new EventStore(bus);
            var storage = new AzureTableEventStore(eventPublisher);
            var rep = new Repository<Shipment>(storage);
            var commands = new ShipmentCommandHandlers(rep);
            bus.RegisterHandler<CreateShipment>(commands.Handle);




            ServiceLocator.Bus = bus;
        }

        private static void CreateTopic()
        {
            var TopicName = AzureEventPublisher.TopicName;

            NamespaceManager namespaceManager = NamespaceManager.Create();

            Console.WriteLine("\nCreating Topic " + AzureEventPublisher.TopicName + "...");
            try
            {
                // Delete if exists
                if (!namespaceManager.TopicExists(TopicName))
                {
                    //namespaceManager.DeleteTopic(TopicName);
                    namespaceManager.CreateTopic(TopicName);
                }

                

                //Console.WriteLine("Creating Subscriptions 'AuditSubscription' and 'AgentSubscription'...");
                //var myAuditSubscription = namespaceManager.CreateSubscription(myTopic.Path, "AuditSubscription");
                //var myAgentSubscription = namespaceManager.CreateSubscription(myTopic.Path, "AgentSubscription");
            }
            catch (MessagingException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
