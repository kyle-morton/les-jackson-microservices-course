using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repo;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _client;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(IPlatformRepo repo, IMapper mapper, ICommandDataClient client, IMessageBusClient messageBusClient)
        {
            _repo = repo;
            _mapper = mapper;
            _client = client;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms() 
        {
            Console.WriteLine("--> Getting Platforms");

            var platforms = _repo.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }

        [HttpGet]
        [Route("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id) 
        {
            Console.WriteLine("--> Getting Platform " + id);

            var platform = _repo.GetPlatformById(id);
            if (platform == null) 
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PlatformReadDto>(platform));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto dto) 
        {
            var platform = _mapper.Map<Platform>(dto);
            
            _repo.CreatePlatform(platform);
            _repo.SaveChanges();

            var readDto = _mapper.Map<PlatformReadDto>(platform);

            // send sync
            try {
                await _client.SendPlatformToCommand(readDto);
            }
            catch (Exception ex) {
                Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
            }

            // send async
            try
            {
                 var publishDto = _mapper.Map<PlatformPublishDto>(readDto);
                 publishDto.Event = "Platform_Published";
                 _messageBusClient.PublishNewPlatform(publishDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
            }

            return CreatedAtRoute(
                nameof(GetPlatformById), 
                new { id = readDto.Id }, 
                readDto
            );
        }
    }
}