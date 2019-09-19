using System.Threading.Tasks;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using volleyball.common.message;
using RabbitMQ.Client.Events;
using System;

namespace volleyball.common.queue
{
    //TODO: This isn't working out, let's just abstract out using MassTransit
    public class RecievedVolleyEventArgs : EventArgs
    {
        private readonly BaseVolleyballMessage _message;

        public RecievedVolleyEventArgs(string queue)
        {
            _message = VolleyballMessageFactory.CreateByName(queue);
        }

        public BaseVolleyballMessage Message
        {
            get { return _message; }
        }
    }

    public class RabbitMQVolleyBallQueue : IVolleyballQueue
    {
        public event EventHandler<RecievedVolleyEventArgs> ReceivedVolley;
        public event EventHandler<ShutdownEventArgs> Shutdown;
        public event EventHandler<ConsumerEventArgs> Registered;
        public event EventHandler<ConsumerEventArgs> Unregistered;
        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;
        public Task Consume(string queue)
        {
            //TODO: Get hostname from config
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
                //channel.QueueBind(queue, queue, string.Format("{0}.*", queue), null);
                //channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    RecievedVolleyEventArgs e = new RecievedVolleyEventArgs(queue);
                    ReceivedVolley(this, e);

                    channel.BasicAck(ea.DeliveryTag, false);
                };
                consumer.Shutdown += (obj, e) =>
                {
                    Shutdown(obj, e);
                };
                consumer.Registered += (obj, e) =>
                {
                    Registered(obj, e);
                };
                consumer.Unregistered += (obj, e) =>
                {
                    Unregistered(obj, e);
                };
                consumer.ConsumerCancelled += (obj, e) =>
                {
                    ConsumerCancelled(obj, e);
                };
                channel.BasicConsume(queue: queue,
                                    autoAck: false,
                                    consumer: consumer);
                return Task.CompletedTask;
            }
        }

        public Task Publish(BaseVolleyballMessage message)
        {
            //TODO: Get hostname from config
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: message.GetType().Name,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
                //channel.ExchangeDeclare(message.GetType().Name, ExchangeType.Topic);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish(exchange: "",//message.GetType().Name,
                                    routingKey: message.GetType().Name,
                                    basicProperties: null,
                                    body: body);
            }

            return Task.CompletedTask;
        }
    }
}