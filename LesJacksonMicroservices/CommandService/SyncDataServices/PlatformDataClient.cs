using System.Net;
using AutoMapper;
using CommandService.Models;
using Grpc.Net.Client;
using PlatformService;

namespace CommandService.SyncDataServices
{
    public class PlatformDataClient : IPlatformDataClient
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IHostEnvironment _env;

        public PlatformDataClient(IConfiguration configuration, IMapper mapper, IHostEnvironment env)
        {
            _config = configuration;
            _mapper = mapper;
            _env = env;
        }

        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            Console.WriteLine("--> Calling Grpc Service - " + _config["GrpcPlatform"]);

            if (_env.IsDevelopment())
            {
                Console.WriteLine("--> Manually setting default proxy...");
                HttpClient.DefaultProxy = new WebProxy();
            }

            var channel = GrpcChannel.ForAddress(_config["GrpcPlatform"]);
            var client = new GrpcPlatform.GrpcPlatformClient(channel);
            var request = new GetAllRequest();

            IEnumerable<Platform> platforms;
            try
            {
                var response = client.GetAllPlatforms(request);
                platforms = _mapper.Map<IEnumerable<Platform>>(response.Platform);
            }
            catch (Exception ex)
            {
                Console.WriteLine("--> Could not call Grpc Service - " + ex.Message);
                platforms = new List<Platform>();
            }

            return platforms;
        }
    }
}