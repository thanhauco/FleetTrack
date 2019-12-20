using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Services;
using FleetTrack.Infrastructure.Persistence;
using Xunit;

namespace FleetTrack.Tests.Services
{
    public class VehicleServiceTests
    {
        private readonly InMemoryVehicleRepository _repo = new InMemoryVehicleRepository();
        private readonly VehicleService _service;

        public VehicleServiceTests()
        {
            _service = new VehicleService(_repo);
        }

        [Fact]
        public void Register_AddsVehicle()
        {
            var result = _service.Register("ab-1", "Ford", "Transit", 2018, FuelType.Diesel, 100);

            Assert.True(result.IsSuccess);
            Assert.NotNull(_repo.Get(result.Value.Id));
        }

        [Fact]
        public void Register_RejectsDuplicatePlate()
        {
            _service.Register("AB-1", "Ford", "Transit", 2018, FuelType.Diesel, 100);

            var duplicate = _service.Register("ab-1", "Ford", "Focus", 2017, FuelType.Gasoline, 0);

            Assert.True(duplicate.IsFailure);
            Assert.Single(_repo.GetAll());
        }

        [Fact]
        public void Register_ConcurrentDuplicates_OnlyOneSucceeds()
        {
            var successes = 0;

            Parallel.For(0, 64, i =>
            {
                if (_service.Register(i % 2 == 0 ? "DUP-1" : "dup-1", "Ford", "Transit", 2018, FuelType.Diesel, 0).IsSuccess)
                {
                    Interlocked.Increment(ref successes);
                }
            });

            Assert.Equal(1, successes);
            Assert.Single(_repo.GetAll());
        }

        [Fact]
        public void Retire_UpdatesStatus()
        {
            var vehicle = _service.Register("AB-1", "Ford", "Transit", 2018, FuelType.Diesel, 100).Value;

            var result = _service.Retire(vehicle.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(VehicleStatus.Retired, _repo.Get(vehicle.Id).Status);
        }

        [Fact]
        public void Retire_FailsForVehicleInUse()
        {
            var vehicle = _service.Register("AB-1", "Ford", "Transit", 2018, FuelType.Diesel, 100).Value;
            vehicle.AssignToTrip();

            Assert.True(_service.Retire(vehicle.Id).IsFailure);
        }

        [Fact]
        public void Search_FiltersAndPages()
        {
            for (var i = 0; i < 12; i++)
            {
                _service.Register("TR-" + i.ToString("00"), "Ford", "Transit", 2018, FuelType.Diesel, 0);
            }

            _service.Register("CAR-1", "Toyota", "Prius", 2019, FuelType.Hybrid, 0);

            var page = _service.Search("ford", 2, 5);

            Assert.Equal(12, page.TotalCount);
            Assert.Equal(5, page.Items.Count);
            Assert.Equal("TR-05", page.Items.First().LicensePlate);
        }
    }
}
