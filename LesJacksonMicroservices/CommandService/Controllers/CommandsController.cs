using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
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
    }
}