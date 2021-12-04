using CommandService.Models;
using CommandService.SyncDataServices;
using Microsoft.EntityFrameworkCore;

namespace CommandService.Data
{
    public static class DbInitializer
    {
        public static void Populate(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = grpcClient.ReturnAllPlatforms();

                SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
            }
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--> Seeding new platforms...");

            foreach (var plat in platforms)
            {
                if (!repo.ExternalPlatformExists(plat.Id))
                {
                    repo.CreatePlatform(plat);
                }
            }

            repo.SaveChanges();
        }

    }
}