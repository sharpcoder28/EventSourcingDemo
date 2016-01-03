//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

namespace Microsoft.Samples.ListenerOne
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using System.Threading;
    using System.Configuration;
    using System.Threading.Tasks;
    using System.IO;
    using Newtonsoft.Json;
    using EventSourcingDemo.Domain;

    public class Program
    {       

        private static TopicClient topicClient;
        private static string TopicName = "Shipments";
        private static string SubscriptionName = "ListenerOneSubscription";

        static void Main(string[] args)
        {
            if (!VerifyConfiguration())
            {
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Creating Topic and Subscriptions");
            CreateTopic();            
            Console.WriteLine("Press any key to start receiving messages that you just sent ...");
            Console.ReadKey();
            ReceiveMessages();
            Console.WriteLine("\nEnd of scenario, press any key to exit.");
            Console.ReadKey();

        }

        private static bool VerifyConfiguration()
        {
            bool configOK = true;
            var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            if (connectionString.Contains("[your namespace]") || connectionString.Contains("[your access key]"))
            {
                configOK = false;
                Console.WriteLine("Please update the 'Microsoft.ServiceBus.ConnectionString' appSetting in app.config to specify your Service Bus namespace and secret key.");
            }
            return configOK;

        }

        private static void CreateTopic()
        {
            NamespaceManager namespaceManager = NamespaceManager.Create();
                        
            try
            {
                var topic = namespaceManager.GetTopic(TopicName);
                           

                Console.WriteLine("Creating Subscriptions 'AuditSubscription' and 'AgentSubscription'...");

                if (!namespaceManager.SubscriptionExists(topic.Path, SubscriptionName))
                {
                    SubscriptionDescription myAuditSubscription = namespaceManager.CreateSubscription(topic.Path, SubscriptionName);
                }
                    
                //SubscriptionDescription myAgentSubscription = namespaceManager.CreateSubscription(myTopic.Path, "AgentSubscription");
            }
            catch (MessagingException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
                

        private static void ReceiveMessages()
        {
            // For PeekLock mode (default) where applications require "at least once" delivery of messages 
            SubscriptionClient agentSubscriptionClient = SubscriptionClient.Create(TopicName, "ListenerOneSubscription");
            BrokeredMessage message = null;
            while (true)
            {
                try
                {
                    //receive messages from Agent Subscription
                    message = agentSubscriptionClient.Receive(TimeSpan.FromSeconds(5));
                    if (message != null)
                    {
                        Console.WriteLine("\nReceiving message from AgentSubscription...");

                        var s = message.GetBody<Stream>();

                        var payload = "";
                        using (var reader = new StreamReader(s))
                        {
                            payload = reader.ReadToEnd();
                        }

                        var jsonSettings = new JsonSerializerSettings
                        {
                            // Allows deserializing to the actual runtime type
                            TypeNameHandling = TypeNameHandling.All,
                            // In a version resilient way
                            TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                        };

                        var deserialized = (Message)JsonConvert.DeserializeObject(payload, jsonSettings);


                        Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, deserialized.ToString()));
                        // Further custom message processing could go here...
                        message.Complete();
                    }
                    else
                    {
                        //no more messages in the subscription
                        Console.WriteLine("No messages");
                        Thread.Sleep(1000);
                    }
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
            }            

            agentSubscriptionClient.Close();            
        }

        private static BrokeredMessage CreateSampleMessage(string messageId, string messageBody)
        {
            BrokeredMessage message = new BrokeredMessage(messageBody);
            message.MessageId = messageId;
            return message;
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
