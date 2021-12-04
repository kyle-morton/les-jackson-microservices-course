using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _config;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly object _exchange;

        public MessageBusClient(IConfiguration configuration)
        {
            _config = configuration;

            var factory = new ConnectionFactory() 
            {
                HostName = _config["RabbitMQ"],
                Port = int.Parse(_config["RabbitMQPort"])
            };

            Console.WriteLine("MQ: " + _config["RabbitMQ"] + "/" + _config["RabbitMQPort"]);

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(
                     exchange: "trigger", 
                     type: ExchangeType.Fanout 
                );

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;                

                Console.WriteLine("--> Connected to message bus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to Message Bus: {ex.Message}");
            }
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }

        public void PublishNewPlatform(PlatformPublishDto dto)
        {
            var message = JsonSerializer.Serialize(dto);
            if (_connection.IsOpen) {
                Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                SendMessage(message);
            }
            else {
                Console.WriteLine("--> RabbitMQ Connection is closed, not sending");
            }
        }

        private void SendMessage(string message) 
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                "trigger",
                routingKey: "",
                basicProperties: null,
                body: body
            );
            Console.WriteLine($"--> We have sent {message}");
        }

        private void Dispose() 
        {
            Console.WriteLine($"--> Message bus disposed");
            if (_channel.IsOpen) {
               _channel.Close();
               _connection.Close(); 
            }
        }
    }
}