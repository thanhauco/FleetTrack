using System;
using FleetTrack.Core.Common;
using FleetTrack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/reports/utilization")]
    public class UtilizationController : ControllerBase
    {
        private readonly UtilizationReportService _utilization;

        public UtilizationController(UtilizationReportService utilization)
        {
            _utilization = utilization;
        }

        [HttpGet]
        public IActionResult Get(DateTime? from, DateTime? to)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return BadRequest(new { error = "Query parameters 'from' and 'to' are required." });
            }

            if (to.Value < from.Value)
            {
                return BadRequest(new { error = "'to' cannot be before 'from'." });
            }

            return Ok(_utilization.Build(new DateRange(from.Value, to.Value)));
        }
    }
}
