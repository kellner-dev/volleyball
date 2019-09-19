using System.Threading.Tasks;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using volleyball.common.message;
using RabbitMQ.Client.Events;
using System;

namespace volleyball.common.queue
{
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
        public event EventHandler ReceivedVolley;
        public void Consume(string queue)
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

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    RecievedVolleyEventArgs e = new RecievedVolleyEventArgs(queue);
                    ReceivedVolley(this, e);
                };
                channel.BasicConsume(queue: queue,
                                    autoAck: true,
                                    consumer: consumer);

            }
        }

        public Task Publish(BaseVolleyballMessage message)
        {
            //TODO: Get hostname from config
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: message.GetType().ToString(),
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish(exchange: "",
                                    routingKey: message.GetType().ToString(),
                                    basicProperties: null,
                                    body: body);
            }

            return Task.CompletedTask;
        }
    }
}