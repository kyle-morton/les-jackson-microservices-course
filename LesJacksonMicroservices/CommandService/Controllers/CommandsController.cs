using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers
{
    [ApiController]
    [Route("api/c/platforms/{platformId}/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICommandRepo _repo;

        public CommandsController(ICommandRepo repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }    

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId) 
        {
            Console.WriteLine("--> Hit GetCommandsForPlatform: " + platformId);

            if (!_repo.PlatformExists(platformId)) {
                return NotFound();
            }

            var commands = _repo.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId) 
        {
            Console.WriteLine("--> Hit GetCommandForPlatform: " + platformId + ", " + commandId);

            if (!_repo.PlatformExists(platformId)) {
                return NotFound();
            }

            var command = _repo.GetCommand(platformId, commandId);
            if (command == null) {
                return NotFound();
            }

            return Ok(_mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommand(int platformId, CommandCreateDto dto) 
        {
            Console.WriteLine("--> Hit CreateCommand: " + platformId);

            if (!_repo.PlatformExists(platformId)) {
                return NotFound();
            }

            var command = _mapper.Map<Command>(dto);
            _repo.CreateCommand(platformId, command);
            _repo.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(command);
            
            return CreatedAtRoute(
                nameof(GetCommandForPlatform),
                new { platformId = platformId, commandId = commandReadDto.Id },
                commandReadDto
            );
        }
    }
}