namespace volleyball.consumer.elastic
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using volleyball.common.queue;

    public class ConsumeRabbitMQHostedService : BackgroundService
    {
        private readonly ILogger _logger;

        public ConsumeRabbitMQHostedService(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedService>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var queue = new RabbitMQVolleyBallQueue();
            queue.ReceivedVolley += HandleMessage;
            queue.Shutdown += OnConsumerShutdown;
            queue.Registered += OnConsumerRegistered;
            queue.Unregistered += OnConsumerUnregistered;
            queue.ConsumerCancelled += OnConsumerConsumerCancelled;

            queue.Consume("AnyVolleyballMessage");
            return Task.CompletedTask;
        }

        private void HandleMessage(object sender, RecievedVolleyEventArgs e)
        {
            // we just print this message 
            _logger.LogInformation($"consumer received");
            //_logger.LogInformation($"consumer received {e.Message}");
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"connection shut down {e.ReplyText}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer cancelled {e.ConsumerTag}");
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer unregistered {e.ConsumerTag}");
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer registered {e.ConsumerTag}");
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"consumer shutdown {e.ReplyText}");
        }

        public override void Dispose()
        {
            //_channel.Close();
            //_connection.Close();
            base.Dispose();
        }
    }
}
