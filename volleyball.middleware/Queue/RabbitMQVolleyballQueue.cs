using System.Threading.Tasks;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using volleyball.middleware.message;

namespace volleyball.middleware.queue
{
    public class RabbitMQVolleyBallQueue : IVolleyballQueue
    {
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