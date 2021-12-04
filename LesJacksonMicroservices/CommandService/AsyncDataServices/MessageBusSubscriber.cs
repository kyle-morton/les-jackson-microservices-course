using System.Text;
using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IModel _channel;
        private object _exchange;
        private string _queueName;

        public MessageBusSubscriber(IConfiguration config, IEventProcessor eventProcessor)
        {
            _config = config;
            _eventProcessor = eventProcessor;

            InitializeRabbitMQ();
        }
        
        private void InitializeRabbitMQ() 
        {
            Console.WriteLine("MQ: " + _config["RabbitMQ"] + "/" + _config["RabbitMQPort"]);
            var factory = new ConnectionFactory() { 
                HostName = _config["RabbitMQ"],
                Port = int.Parse(_config["RabbitMQPort"]) 
            };

            _connection = factory.CreateConnection();        
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(
                queue: _queueName,
                exchange: "trigger",
                routingKey: ""
            );
            Console.WriteLine("--> Listening on message bus...");

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Connection shutdown");
        }

        public override void Dispose()
        {
            if (_channel.IsOpen) {
                _channel.Close();
                _connection.Close();
            }
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ModuleHandle, ea) => {
                Console.WriteLine("--> Event received");

                var body = ea.Body;
                var notificationMsg = Encoding.UTF8.GetString(body.ToArray());

                _eventProcessor.ProcessEvent(notificationMsg);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}