using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
                var dto = JsonSerializer.Deserialize<PlatformPublishDto>(platformPublishedMessage);

                try
                {
                    var plat = _mapper.Map<Platform>(dto);
                    if (repo.ExternalPlatformExists(plat.Id))
                    {
                        Console.WriteLine("--> Platform already exists: " + plat.Id);
                    }
                    else
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                        Console.WriteLine("--> Platform added: " + plat.Id);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("--> Could not add platform to DB");
                }
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--> Determining event type for : " + notificationMessage);

            var message = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (message.Event)
            {
                case "Platform_Published":
                    Console.WriteLine("--> Platform published event detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> Undetermined event detected");
                    return EventType.Undetermined;
            }
        }
    }
    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}