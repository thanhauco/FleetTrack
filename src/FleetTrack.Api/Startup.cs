using System;
using FleetTrack.Api.Middleware;
using FleetTrack.Core.Common;
using FleetTrack.Core.Events;
using FleetTrack.Core.Handlers;
using FleetTrack.Core.Interfaces;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Notifications;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Infrastructure.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

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
            services.AddSingleton<IEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<InProcessEventBus>>();
                return new InProcessEventBus((e, ex) => logger.LogError(ex, "Handler for {Event} failed", e.GetType().Name));
            });
            services.AddSingleton<INotifier, ConsoleNotifier>(sp => new ConsoleNotifier());

            services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
            services.AddSingleton<IDriverRepository, InMemoryDriverRepository>();
            services.AddSingleton<ITripRepository, InMemoryTripRepository>();
            services.AddSingleton<IMaintenanceRepository, InMemoryMaintenanceRepository>();
            services.AddSingleton<IFuelLogRepository, InMemoryFuelLogRepository>();

            services.AddSingleton(sp => new MaintenanceScheduler(new MaintenanceSchedulerOptions
            {
                IntervalKm = Configuration.GetValue("Fleet:MaintenanceIntervalKm", 10000.0),
                IntervalDays = Configuration.GetValue("Fleet:MaintenanceIntervalDays", 180)
            }));

            services.AddSingleton<VehicleService>();
            services.AddSingleton<DriverService>();
            services.AddSingleton<TripService>();
            services.AddSingleton<AlertService>();
            services.AddSingleton<CostReportService>();
            services.AddSingleton<UtilizationReportService>();
            services.AddSingleton<DemoDataSeeder>();
            services.AddSingleton(sp => new LongTripNotifier(
                sp.GetRequiredService<IEventBus>(),
                sp.GetRequiredService<INotifier>(),
                Configuration.GetValue("Fleet:LongTripThresholdKm", 500.0)));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "FleetTrack API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Resolve eagerly so the handler subscribes to the event bus.
            app.ApplicationServices.GetRequiredService<LongTripNotifier>();

            if (Configuration.GetValue("Seed:Enabled", false))
            {
                app.ApplicationServices.GetRequiredService<DemoDataSeeder>().Seed();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FleetTrack API v1"));

            app.UseMvc();
        }
    }
}
