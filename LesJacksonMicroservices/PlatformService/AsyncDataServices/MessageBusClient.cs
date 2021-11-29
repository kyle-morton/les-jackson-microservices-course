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

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(
                     exchange: "trigger", 
                     type: ExchangeType.Fanout 
                );

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to Message Bus: {ex.Message}");
            }
        }

        public void PublishNewPlatform(PlatformPublishDto dto)
        {
            throw new NotImplementedException();
        }
    }
}