using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class DbInitializer
    {
        public static void Populate(IApplicationBuilder app, IWebHostEnvironment env) 
        {
            using (var serviceScope = app.ApplicationServices.CreateScope()) 
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), env);
            }
        }

        private static void SeedData(AppDbContext context, IWebHostEnvironment env) 
        {
            if (env.IsProduction()) {
                Console.WriteLine("--> Attempting to add migrations...");
                
                try {
                    context.Database.Migrate();
                }catch(Exception ex) {
                    Console.WriteLine("--> Could not run migrations: " + ex.Message);
                }
            }

            if (context.Platforms.Any()) 
            {
                Console.WriteLine("--> We already have data");
                return;
            }

            Console.WriteLine("--> Seeding data...");
            
            context.Platforms.AddRange(
                new Platform { Name = "Dot Net", Publisher = "Microsoft", Cost = "Free" },
                new Platform { Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free" },
                new Platform { Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free" }
            );

            context.SaveChanges();
        }
    }
}