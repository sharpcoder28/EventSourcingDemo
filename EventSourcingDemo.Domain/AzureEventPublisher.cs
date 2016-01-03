using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingDemo.Domain
{
    public class AzureEventPublisher : IEventPublisher
    {
        public const string TopicName = "Shipments";

        public void Publish<T>(T @event) where T : Event
        {
            var topicClient = TopicClient.Create(TopicName);

            using (var ms = new MemoryStream())
            {
                var message = BuildMessage(@event, ms);

                Console.WriteLine("\nSending messages to topic...");

                while (true)
                {
                    try
                    {
                        topicClient.Send(message);
                    }
                    catch (MessagingException e)
                    {
                        if (!e.IsTransient)
                        {
                            Console.WriteLine(e.Message);
                            throw;
                        }
                        else
                        {
                            HandleTransientErrors(e);
                        }
                    }
                    Console.WriteLine(string.Format("Message sent: Id = {0}, Body = {1}", message.MessageId, message.GetBody<Stream>().ToString()));
                    break;
                }

                topicClient.Close();
            }

        }

        private static BrokeredMessage BuildMessage(Event @event, Stream stream)
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
                
            ser.Serialize(jsonWriter, @event);                

            var message = new BrokeredMessage(stream, true);

            jsonWriter.Flush();

            stream.Position = 0;            

            return message;
            
        }

        private static void SendMessages()
        {
            

            //List<BrokeredMessage> messageList = new List<BrokeredMessage>();
            //messageList.Add(CreateSampleMessage("1", "First message information"));
            //messageList.Add(CreateSampleMessage("2", "Second message information"));
            //messageList.Add(CreateSampleMessage("3", "Third message information"));

            //Console.WriteLine("\nSending messages to topic...");


            //foreach (BrokeredMessage message in messageList)
            //{
            //    while (true)
            //    {
            //        try
            //        {
            //            topicClient.Send(message);
            //        }
            //        catch (MessagingException e)
            //        {
            //            if (!e.IsTransient)
            //            {
            //                Console.WriteLine(e.Message);
            //                throw;
            //            }
            //            else
            //            {
            //                HandleTransientErrors(e);
            //            }
            //        }
            //        Console.WriteLine(string.Format("Message sent: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
            //        break;
            //    }
            //}

            //topicClient.Close();
        }


        private static void HandleTransientErrors(MessagingException e)
        {
            //If transient error/exception, let's back-off for 2 seconds and retry
            Console.WriteLine(e.Message);
            Console.WriteLine("Will retry sending the message in 2 seconds");
            Thread.Sleep(2000);
        }
    }
}
