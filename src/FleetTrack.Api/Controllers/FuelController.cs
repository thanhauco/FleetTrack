using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Api.Mapping;
using FleetTrack.Api.Models;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;
using FleetTrack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/vehicles/{vehicleId:guid}/fuel")]
    public class FuelController : ControllerBase
    {
        private readonly FuelService _fuel;
        private readonly IVehicleRepository _vehicles;
        private readonly IFuelLogRepository _fuelLogs;

        public FuelController(FuelService fuel, IVehicleRepository vehicles, IFuelLogRepository fuelLogs)
        {
            _fuel = fuel;
            _vehicles = vehicles;
            _fuelLogs = fuelLogs;
        }

        [HttpGet]
        public ActionResult<IEnumerable<FuelLogDto>> List(Guid vehicleId)
        {
            if (_vehicles.Get(vehicleId) == null)
            {
                return NotFound();
            }

            return _fuelLogs.FindByVehicle(vehicleId).Select(l => l.ToDto()).ToList();
        }

        [HttpPost]
        public ActionResult<FuelLogDto> Record(Guid vehicleId, RecordFuelRequest request)
        {
            if (_vehicles.Get(vehicleId) == null)
            {
                return NotFound();
            }

            var result = _fuel.RecordFill(
                vehicleId,
                request.Liters,
                new Money(request.Cost, request.Currency),
                request.OdometerKm,
                request.FilledUtc);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Created($"api/vehicles/{vehicleId}/fuel/{result.Value.Id}", result.Value.ToDto());
        }

        [HttpGet("summary")]
        public ActionResult<FuelSummaryDto> Summary(Guid vehicleId)
        {
            var result = _fuel.Summarise(vehicleId);
            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            var s = result.Value;
            return new FuelSummaryDto
            {
                VehicleId = s.VehicleId,
                FillCount = s.FillCount,
                TotalLiters = s.TotalLiters,
                LitersPer100Km = s.LitersPer100Km,
                KmPerLiter = s.KmPerLiter
            };
        }
    }
}
