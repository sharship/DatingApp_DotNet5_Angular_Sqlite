using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // initialized host
            var host = CreateHostBuilder(args).Build();
            // get a scope to handle scoped services, and then get services that are dependency injected
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // get DBContext and apply migrations
                var context = services.GetRequiredService<DataContext>();
                await context.Database.MigrateAsync();

                // get an service instance of UserManager, and then seed data
                var userManager = services.GetRequiredService<UserManager<AppUser>>();

                var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
                
                await Seed.SeedUsersAsync(userManager, roleManager);
            }
            catch (System.Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during migration");
            }

            // host run the application
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
