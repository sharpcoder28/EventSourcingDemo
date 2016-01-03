using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EventSourcingDemo.Domain
{
    public interface IEventStore
    {
        void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion);
        List<Event> GetEventsForAggregate(Guid aggregateId);
    }

    public class AzureTableEventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;

        public AzureTableEventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        private struct EventDescriptor
        {

            public readonly Event EventData;
            public readonly Guid Id;
            public readonly int Version;

            public EventDescriptor(Guid id, Event eventData, int version)
            {
                EventData = eventData;
                Version = version;
                Id = id;
            }
        }

        public List<Event> GetEventsForAggregate(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            var enterpriseID = new Guid("b1c805bc-41bf-4362-87c1-e8921e7e16df");

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=blueshipdev;AccountKey=kWlQ0YZuxojqmY0oge8XETPGLyiltJfSFXghBnW0f7mUFp6fg2721+jEbsQb0hiYTvUzZHGRwg0kGjLInI3T2g==";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference("shipments");
            table.CreateIfNotExists();

            
            var i = expectedVersion;

            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;

                string eventType = @event.GetType().Name;

                string eventData = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
                {                    
                    TypeNameHandling = TypeNameHandling.All,                    
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                });

                var shipmentEntity = new ShipmentEntity(aggregateId, i, eventData);

                // Create the TableOperation object that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(shipmentEntity);

                // Execute the insert operation.
                table.Execute(insertOperation);

                // push event to the event descriptors list for current aggregate
                //eventDescriptors.Add(new EventDescriptor(aggregateId, @event, i));

                // publish current event to the bus for further processing by subscribers
                _publisher.Publish(@event);
            }



        }
    }

    public class ShipmentEntity : TableEntity
    {
        public ShipmentEntity(Guid shipmentID, int version, string eventData)
        {
            var formattedVersion = version.ToString("D10");
            this.PartitionKey = shipmentID.ToString();
            this.RowKey = formattedVersion;            
            this.EventData = eventData;            
        }

        public ShipmentEntity() { }        
               

        public string EventData { get; set; }        
    }

    public class EventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;

        private struct EventDescriptor
        {

            public readonly Event EventData;
            public readonly Guid Id;
            public readonly int Version;

            public EventDescriptor(Guid id, Event eventData, int version)
            {
                EventData = eventData;
                Version = version;
                Id = id;
            }
        }

        public EventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        private readonly Dictionary<Guid, List<EventDescriptor>> _current = new Dictionary<Guid, List<EventDescriptor>>();

        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            List<EventDescriptor> eventDescriptors;

            // try to get event descriptors list for given aggregate id
            // otherwise -> create empty dictionary
            if (!_current.TryGetValue(aggregateId, out eventDescriptors))
            {
                eventDescriptors = new List<EventDescriptor>();
                _current.Add(aggregateId, eventDescriptors);
            }
            // check whether latest event version matches current aggregate version
            // otherwise -> throw exception
            else if (eventDescriptors[eventDescriptors.Count - 1].Version != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }
            var i = expectedVersion;

            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;

                // push event to the event descriptors list for current aggregate
                eventDescriptors.Add(new EventDescriptor(aggregateId, @event, i));

                // publish current event to the bus for further processing by subscribers
                _publisher.Publish(@event);
            }
        }

        // collect all processed events for given aggregate and return them as a list
        // used to build up an aggregate from its history (Domain.LoadsFromHistory)
        public List<Event> GetEventsForAggregate(Guid aggregateId)
        {
            List<EventDescriptor> eventDescriptors;

            if (!_current.TryGetValue(aggregateId, out eventDescriptors))
            {
                throw new AggregateNotFoundException();
            }

            return eventDescriptors.Select(desc => desc.EventData).ToList();
        }
    }

    public class AggregateNotFoundException : Exception
    {
    }

    public class ConcurrencyException : Exception
    {
    }
}
