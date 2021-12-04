using CommandService.Models;

namespace CommandService.SyncDataServices
{
    public interface IPlatformDataClient
    {
         IEnumerable<Platform> ReturnAllPlatforms();
    }
}