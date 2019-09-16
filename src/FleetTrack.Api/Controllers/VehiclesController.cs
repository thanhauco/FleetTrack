using System;
using FleetTrack.Api.Mapping;
using FleetTrack.Api.Models;
using FleetTrack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly VehicleService _vehicles;

        public VehiclesController(VehicleService vehicles)
        {
            _vehicles = vehicles;
        }

        [HttpGet]
        public ActionResult<PagedResponse<VehicleDto>> Search(string q = null, int page = 1, int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "page must be >= 1 and pageSize between 1 and 100." });
            }

            return _vehicles.Search(q, page, pageSize).ToResponse(v => v.ToDto());
        }

        [HttpGet("{id:guid}")]
        public ActionResult<VehicleDto> Get(Guid id)
        {
            var vehicle = _vehicles.Get(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle.ToDto();
        }

        [HttpPost]
        public ActionResult<VehicleDto> Create(CreateVehicleRequest request)
        {
            var result = _vehicles.Register(request.LicensePlate, request.Make, request.Model, request.Year, request.FuelType, request.OdometerKm);
            if (result.IsFailure)
            {
                return Conflict(new { error = result.Error });
            }

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value.ToDto());
        }

        [HttpPost("{id:guid}/retire")]
        public IActionResult Retire(Guid id)
        {
            var result = _vehicles.Retire(id);
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return NoContent();
        }
    }
}
