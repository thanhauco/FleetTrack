using System;
using FleetTrack.Api.Mapping;
using FleetTrack.Api.Models;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/trips")]
    public class TripsController : ControllerBase
    {
        private readonly TripService _trips;

        public TripsController(TripService trips)
        {
            _trips = trips;
        }

        [HttpGet("{id:guid}")]
        public ActionResult<TripDto> Get(Guid id)
        {
            var trip = _trips.Get(id);
            if (trip == null)
            {
                return NotFound();
            }

            return trip.ToDto();
        }

        [HttpPost]
        public ActionResult<TripDto> Plan(PlanTripRequest request)
        {
            var result = _trips.Plan(
                request.VehicleId,
                request.DriverId,
                new GeoPoint(request.Origin.Latitude, request.Origin.Longitude),
                new GeoPoint(request.Destination.Latitude, request.Destination.Longitude),
                request.PlannedStartUtc);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value.ToDto());
        }

        [HttpPost("{id:guid}/start")]
        public ActionResult<TripDto> Start(Guid id)
        {
            var result = _trips.Start(id);
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return result.Value.ToDto();
        }

        [HttpPost("{id:guid}/complete")]
        public ActionResult<TripDto> Complete(Guid id, CompleteTripRequest request)
        {
            var result = _trips.Complete(id, request.EndOdometerKm);
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return result.Value.ToDto();
        }

        [HttpPost("{id:guid}/cancel")]
        public IActionResult Cancel(Guid id)
        {
            var result = _trips.Cancel(id);
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return NoContent();
        }
    }
}
