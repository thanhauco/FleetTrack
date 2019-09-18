using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Api.Mapping;
using FleetTrack.Api.Models;
using FleetTrack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/drivers")]
    public class DriversController : ControllerBase
    {
        private readonly DriverService _drivers;

        public DriversController(DriverService drivers)
        {
            _drivers = drivers;
        }

        [HttpGet("{id:guid}")]
        public ActionResult<DriverDto> Get(Guid id)
        {
            var driver = _drivers.Get(id);
            if (driver == null)
            {
                return NotFound();
            }

            return driver.ToDto();
        }

        [HttpGet("expiring")]
        public ActionResult<IEnumerable<DriverDto>> Expiring(int days = 30)
        {
            return _drivers.GetWithExpiringLicenses(days).Select(d => d.ToDto()).ToList();
        }

        [HttpPost]
        public ActionResult<DriverDto> Hire(HireDriverRequest request)
        {
            var result = _drivers.Hire(request.FullName, request.LicenseNumber, request.LicenseClass, request.LicenseExpiryUtc);
            if (result.IsFailure)
            {
                return Conflict(new { error = result.Error });
            }

            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value.ToDto());
        }

        [HttpPost("{id:guid}/deactivate")]
        public IActionResult Deactivate(Guid id)
        {
            var result = _drivers.Deactivate(id);
            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            return NoContent();
        }
    }
}
