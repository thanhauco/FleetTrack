using FleetTrack.Core.Common;
using FleetTrack.Core.Events;
using FleetTrack.Core.Interfaces;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetTrack.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<IEventBus, InProcessEventBus>();
            services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
            services.AddSingleton<IDriverRepository, InMemoryDriverRepository>();
            services.AddSingleton<ITripRepository, InMemoryTripRepository>();
            services.AddSingleton<VehicleService>();
            services.AddSingleton<DriverService>();
            services.AddSingleton<TripService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
