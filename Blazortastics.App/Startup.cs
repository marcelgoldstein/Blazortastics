using Blazortastics.App.Services;
using Blazortastics.App.Services.Puzzle;
using Blazortastics.DB;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazortastics.App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<PlacementCheckService>();
            services.AddSingleton<MessageBoxService>();

            var config = services.BuildServiceProvider().GetService<IConfigurationRoot>();

            services.AddDbContext<BlazorDbContext>(o => o.UseSqlServer(config.GetConnectionString("BlazorDB")));

            services.AddTransient<LeaderboardService>();

            services.AddBlazorContextMenu();
        }

        public void Configure(IBlazorApplicationBuilder app, BlazorDbContext db)
        {
            app.AddComponent<App>("app");

            // ensure db is created and up to date
            db.Database.Migrate();
        }
    }
}
