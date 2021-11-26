using System.Text;
using System.Text.Json;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public HttpCommandDataClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        public async Task SendPlatformToCommand(PlatformReadDto plat)
        {
            var json = JsonSerializer.Serialize(plat);
            var httpContent = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var url = _config["CommandService"];
            var response = await _client.PostAsync($"{url}", httpContent);

            Console.WriteLine($"Sending platform to {url}: {json}");

            if (response.IsSuccessStatusCode) 
            {
                Console.WriteLine("--> Sync POST to CommandService was OK");
            }
            else 
            {
                Console.WriteLine("--> Sync POST to CommandService was NOT OK!");   
            }
        }
    }
}