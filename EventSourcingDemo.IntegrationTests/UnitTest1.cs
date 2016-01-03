using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using EventSourcingDemo.Domain;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace EventSourcingDemo.IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                // Allows deserializing to the actual runtime type
                TypeNameHandling = TypeNameHandling.All,
                // In a version resilient way
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };

            var @event = new ShipmentCreated(Guid.NewGuid(), 100, 2, 50);


            string eventData = JsonConvert.SerializeObject(@event, jsonSettings);


            var deserialized = (Message)JsonConvert.DeserializeObject(eventData, jsonSettings);


            Assert.AreEqual(deserialized.GetType(), typeof(ShipmentCreated));
        }

        [TestMethod]
        public void BrokeredMessage_Serialize()
        {            
            var stream = new MemoryStream();
            try
            {
                var writer = new StreamWriter(stream);

                var jsonSettings = new JsonSerializerSettings
                {
                    // Allows deserializing to the actual runtime type
                    TypeNameHandling = TypeNameHandling.All,
                    // In a version resilient way
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                };

                JsonTextWriter jsonWriter = new JsonTextWriter(writer);
                JsonSerializer ser = JsonSerializer.Create(jsonSettings);

                var @event = new ShipmentCreated(Guid.NewGuid(), 100, 2, 50);

                ser.Serialize(jsonWriter, @event);


                var message = new BrokeredMessage(stream, true);

                jsonWriter.Flush();
                stream.Close();



                var s = message.GetBody<Stream>();
                s.Position = 0;
                var payload = "";
                using (var reader = new StreamReader(s))
                {
                    payload = reader.ReadToEnd();
                }

                var deserialized = (Message)JsonConvert.DeserializeObject(payload, jsonSettings);


                Assert.AreEqual(deserialized.GetType(), typeof(ShipmentCreated));
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }    
    }
}
