using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Api.Mapping;
using FleetTrack.Api.Models;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/vehicles/{vehicleId:guid}/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IMaintenanceRepository _maintenance;
        private readonly IClock _clock;

        public MaintenanceController(IVehicleRepository vehicles, IMaintenanceRepository maintenance, IClock clock)
        {
            _vehicles = vehicles;
            _maintenance = maintenance;
            _clock = clock;
        }

        [HttpGet]
        public ActionResult<IEnumerable<MaintenanceDto>> List(Guid vehicleId)
        {
            if (_vehicles.Get(vehicleId) == null)
            {
                return NotFound();
            }

            return _maintenance.FindByVehicle(vehicleId).Select(r => r.ToDto()).ToList();
        }

        [HttpPost]
        public ActionResult<MaintenanceDto> Record(Guid vehicleId, RecordMaintenanceRequest request)
        {
            var vehicle = _vehicles.Get(vehicleId);
            if (vehicle == null)
            {
                return NotFound();
            }

            var now = _clock.UtcNow;
            var record = new MaintenanceRecord(
                vehicleId,
                request.Type,
                now,
                vehicle.OdometerKm,
                new Money(request.Cost, request.Currency),
                request.Notes);

            vehicle.CompleteMaintenance(now);
            _vehicles.Update(vehicle);
            _maintenance.Add(record);

            return Created($"api/vehicles/{vehicleId}/maintenance/{record.Id}", record.ToDto());
        }
    }
}
