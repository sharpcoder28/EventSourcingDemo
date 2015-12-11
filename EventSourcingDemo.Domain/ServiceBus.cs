using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingDemo.Domain
{
    class ServiceBus
    {
        //Endpoint=sb://blueshipdev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3H27MgTvOAVKfVrDOhccIM5jtBsnXj7CCi9BjywSimg= 
    }

    //public interface Handles<T>
    //{
    //    void Handle(T message);
    //}

    //public interface ICommandSender
    //{
    //    void Send<T>(T command) where T : Command;

    //}
    //public interface IEventPublisher
    //{
    //    void Publish<T>(T @event) where T : Event;
    //}
}
