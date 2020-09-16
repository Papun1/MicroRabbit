using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string,List<Type>> _handler;
        private readonly List<Type> _eventType;
        public RabbitMQBus(IMediator mediator)
        {
            _mediator = mediator;
            _handler = new Dictionary<string, List<Type>>();
            _eventType = new List<Type>();
        }
        public Task SendCommand<T>(T Command) where T : Command
        {
            return _mediator.Send(Command);
        }
        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "Localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var eventName = @event.GetType().ToString();
                channel.QueueDeclare(eventName, false, false, false, null);
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", eventName, null, body);  
            }
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);
            if(!_eventType.Contains(typeof(T)))
            {
                _eventType.Add(typeof(T));
            }
            if(!_handler.ContainsKey(eventName))
            {
                _handler.Add(eventName, new List<Type>());
            }
            if(_handler[eventName].Any(s=>s.GetType() == handlerType))
            {
                throw new ArgumentException($"Handler Type{handlerType.Name} already is registered for{eventName}",nameof(handlerType));
            }
            _handler[eventName].Add(handlerType);
            StartBasicConsume<T>();
        }
        private void StartBasicConsume<T>() where T:Event
        {
            var factory = new ConnectionFactory() { HostName = "Localhost",DispatchConsumersAsync=true };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var eventName = typeof(T).Name;
            channel.QueueDeclare(eventName, false, false, false, null);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body.Span);
            try
            {
                await processEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task processEvent(string eventName, string message)
        {
           if(_handler.ContainsKey(eventName))
            {
                var subscrptions = _handler[eventName];
                foreach(var subscrption in subscrptions)
                {
                    var handler = Activator.CreateInstance(subscrption);
                    if (handler == null) continue;
                    var eventType = _eventType.SingleOrDefault(t => t.Name == eventName);
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event});
                }
            }
        }
    }
}
