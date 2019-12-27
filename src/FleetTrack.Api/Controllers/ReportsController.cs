using System;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetTrack.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly CostReportService _costs;
        private readonly AlertService _alerts;

        public ReportsController(CostReportService costs, AlertService alerts)
        {
            _costs = costs;
            _alerts = alerts;
        }

        [HttpGet("vehicles/{vehicleId:guid}/costs")]
        public ActionResult<object> Costs(Guid vehicleId, DateTime? from, DateTime? to, string currency = "USD")
        {
            // Non-nullable DateTime query parameters silently bind to 0001-01-01 when
            // omitted, which produced an empty "successful" report.
            if (!from.HasValue || !to.HasValue)
            {
                return BadRequest(new { error = "Query parameters 'from' and 'to' are required." });
            }

            var result = _costs.Build(vehicleId, from.Value, to.Value, currency);
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            var report = result.Value;
            return new
            {
                report.VehicleId,
                report.FromUtc,
                report.ToUtc,
                MaintenanceCost = report.MaintenanceCost.Amount,
                FuelCost = report.FuelCost.Amount,
                TotalCost = report.TotalCost.Amount,
                Currency = report.TotalCost.Currency,
                report.DistanceKm,
                report.CostPerKm
            };
        }

        [HttpGet("alerts")]
        public ActionResult<IEnumerable<object>> Alerts()
        {
            return _alerts.GetAlerts()
                .Select(a => (object)new { Severity = a.Severity.ToString(), a.Message })
                .ToList();
        }
    }
}
