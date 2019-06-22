using System;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class CostReportServiceTests
    {
        private readonly InMemoryMaintenanceRepository _maintenance = new InMemoryMaintenanceRepository();
        private readonly InMemoryFuelLogRepository _fuel = new InMemoryFuelLogRepository();
        private readonly InMemoryVehicleRepository _vehicles = new InMemoryVehicleRepository();
        private readonly CostReportService _service;
        private readonly Vehicle _vehicle = TestData.Vehicle();

        public CostReportServiceTests()
        {
            _service = new CostReportService(_maintenance, _fuel, _vehicles);
            _vehicles.Add(_vehicle);
        }

        [Fact]
        public void Build_SumsCostsWithinPeriod()
        {
            var from = TestData.Now.AddDays(-30);
            _maintenance.Add(new MaintenanceRecord(_vehicle.Id, MaintenanceType.OilChange, TestData.Now.AddDays(-10), 1100, TestData.Usd(100)));
            _maintenance.Add(new MaintenanceRecord(_vehicle.Id, MaintenanceType.Repair, TestData.Now.AddDays(-90), 900, TestData.Usd(999)));
            _fuel.Add(new FuelLog(_vehicle.Id, TestData.Now.AddDays(-20), 40, TestData.Usd(60), 1000));
            _fuel.Add(new FuelLog(_vehicle.Id, TestData.Now.AddDays(-5), 40, TestData.Usd(70), 1500));

            var result = _service.Build(_vehicle.Id, from, TestData.Now);

            Assert.True(result.IsSuccess);
            Assert.Equal(TestData.Usd(100), result.Value.MaintenanceCost);
            Assert.Equal(TestData.Usd(130), result.Value.FuelCost);
            Assert.Equal(TestData.Usd(230), result.Value.TotalCost);
            Assert.Equal(500, result.Value.DistanceKm);
            Assert.Equal(0.46m, result.Value.CostPerKm);
        }

        [Fact]
        public void Build_CostPerKmIsNullWithoutDistance()
        {
            var result = _service.Build(_vehicle.Id, TestData.Now.AddDays(-1), TestData.Now);

            Assert.Null(result.Value.CostPerKm);
            Assert.Equal(TestData.Usd(0), result.Value.TotalCost);
        }

        [Fact]
        public void Build_FailsForUnknownVehicle()
        {
            Assert.True(_service.Build(Guid.NewGuid(), TestData.Now.AddDays(-1), TestData.Now).IsFailure);
        }

        [Fact]
        public void Build_FailsForInvertedPeriod()
        {
            Assert.True(_service.Build(_vehicle.Id, TestData.Now, TestData.Now.AddDays(-1)).IsFailure);
        }

        [Fact]
        public void Build_FailsOnCurrencyMismatch()
        {
            _fuel.Add(new FuelLog(_vehicle.Id, TestData.Now.AddDays(-1), 40, new Money(60m, "EUR"), 1000));

            var result = _service.Build(_vehicle.Id, TestData.Now.AddDays(-5), TestData.Now);

            Assert.True(result.IsFailure);
        }
    }
}
