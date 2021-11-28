using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controllers
{
    [ApiController]
    [Route("api/c/[controller]")]
    public class PlatformsController : ControllerBase
    {
        
        public PlatformsController()
        {
        }   

        [HttpPost]
        public ActionResult TestInboundConnection() 
        {
            Console.WriteLine("--> Inbound post @ Command Service");
            return Ok("inbound test of Command service platforms controller");
        }


    }
}