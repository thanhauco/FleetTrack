using System;
using FleetTrack.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly IClock _clock;

        public HealthController(IClock clock)
        {
            _clock = clock;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "ok", timeUtc = _clock.UtcNow });
        }
    }
}
