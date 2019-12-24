using System;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Services
{
    public class VehicleService
    {
        // Serialises the uniqueness check with the insert; otherwise two concurrent
        // registrations of the same plate both pass FindByPlate.
        private readonly object _gate = new object();
        private readonly IVehicleRepository _vehicles;

        public VehicleService(IVehicleRepository vehicles)
        {
            _vehicles = Guard.NotNull(vehicles, nameof(vehicles));
        }

        public Result<Vehicle> Register(
            string licensePlate,
            string make,
            string model,
            int year,
            FuelType fuelType,
            double odometerKm,
            VehicleCategory category = VehicleCategory.Car)
        {
            lock (_gate)
            {
                if (_vehicles.FindByPlate(licensePlate) != null)
                {
                    return Result.Fail<Vehicle>($"A vehicle with plate {licensePlate} already exists.");
                }

                var vehicle = new Vehicle(licensePlate, make, model, year, fuelType, odometerKm, category);
                _vehicles.Add(vehicle);
                return Result.Ok(vehicle);
            }
        }

        public Vehicle Get(Guid id)
        {
            return _vehicles.Get(id);
        }

        public Result Retire(Guid id)
        {
            var vehicle = _vehicles.Get(id);
            if (vehicle == null)
            {
                return Result.Fail("Vehicle not found.");
            }

            try
            {
                vehicle.Retire();
            }
            catch (DomainException ex)
            {
                return Result.Fail(ex.Message);
            }

            _vehicles.Update(vehicle);
            return Result.Ok();
        }

        public PagedResult<Vehicle> Search(string term, int page, int pageSize)
        {
            var query = _vehicles.GetAll().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(v => v.LicensePlate.ContainsIgnoreCase(term)
                                         || v.Make.ContainsIgnoreCase(term)
                                         || v.Model.ContainsIgnoreCase(term));
            }

            return PagedResult<Vehicle>.Create(query.OrderBy(v => v.LicensePlate), page, pageSize);
        }
    }
}
