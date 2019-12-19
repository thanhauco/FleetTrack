using System;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Events;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Core.Services
{
    public class TripService
    {
        private readonly ITripRepository _trips;
        private readonly IVehicleRepository _vehicles;
        private readonly IDriverRepository _drivers;
        private readonly IClock _clock;
        private readonly IEventBus _events;

        public TripService(ITripRepository trips, IVehicleRepository vehicles, IDriverRepository drivers, IClock clock, IEventBus events)
        {
            _trips = Guard.NotNull(trips, nameof(trips));
            _vehicles = Guard.NotNull(vehicles, nameof(vehicles));
            _drivers = Guard.NotNull(drivers, nameof(drivers));
            _clock = Guard.NotNull(clock, nameof(clock));
            _events = Guard.NotNull(events, nameof(events));
        }

        public Trip Get(Guid id)
        {
            return _trips.Get(id);
        }

        public Result<Trip> Plan(Guid vehicleId, Guid driverId, GeoPoint origin, GeoPoint destination, DateTime plannedStartUtc)
        {
            var vehicle = _vehicles.Get(vehicleId);
            if (vehicle == null)
            {
                return Result.Fail<Trip>("Vehicle not found.");
            }

            var driver = _drivers.Get(driverId);
            if (driver == null)
            {
                return Result.Fail<Trip>("Driver not found.");
            }

            if (!driver.IsLicenseValid(_clock.UtcNow))
            {
                return Result.Fail<Trip>("Driver is inactive or has an expired licence.");
            }

            // Valid today is not enough for a trip booked weeks ahead.
            if (!driver.IsLicenseValid(plannedStartUtc))
            {
                return Result.Fail<Trip>($"Licence of {driver.FullName} expires on {driver.LicenseExpiryUtc:yyyy-MM-dd}, before the planned start.");
            }

            if (vehicle.Status == VehicleStatus.Retired || vehicle.Status == VehicleStatus.InMaintenance)
            {
                return Result.Fail<Trip>($"Vehicle {vehicle.LicensePlate} cannot be used ({vehicle.Status}).");
            }

            var trip = new Trip(vehicleId, driverId, origin, destination, plannedStartUtc);
            _trips.Add(trip);
            return Result.Ok(trip);
        }

        public Result<Trip> Start(Guid tripId)
        {
            var trip = _trips.Get(tripId);
            if (trip == null)
            {
                return Result.Fail<Trip>("Trip not found.");
            }

            if (trip.Status != TripStatus.Planned)
            {
                return Result.Fail<Trip>($"Trip is {trip.Status}, only planned trips can be started.");
            }

            // The licence may have expired, or the driver been deactivated, since planning.
            var driver = _drivers.Get(trip.DriverId);
            if (driver == null || !driver.IsLicenseValid(_clock.UtcNow))
            {
                return Result.Fail<Trip>("Driver is inactive or has an expired licence.");
            }

            var vehicle = _vehicles.Get(trip.VehicleId);
            if (vehicle.Status != VehicleStatus.Available)
            {
                return Result.Fail<Trip>($"Vehicle {vehicle.LicensePlate} is not available.");
            }

            vehicle.AssignToTrip();
            trip.Start(_clock.UtcNow, vehicle.OdometerKm);

            _vehicles.Update(vehicle);
            _trips.Update(trip);
            return Result.Ok(trip);
        }

        public Result<Trip> Complete(Guid tripId, double endOdometerKm)
        {
            var trip = _trips.Get(tripId);
            if (trip == null)
            {
                return Result.Fail<Trip>("Trip not found.");
            }

            var vehicle = _vehicles.Get(trip.VehicleId);
            var now = _clock.UtcNow;

            try
            {
                trip.Complete(now, endOdometerKm);
                vehicle.UpdateOdometer(endOdometerKm);
                vehicle.ReleaseFromTrip();
            }
            catch (DomainException ex)
            {
                return Result.Fail<Trip>(ex.Message);
            }

            _vehicles.Update(vehicle);
            _trips.Update(trip);

            _events.Publish(new TripCompletedEvent(trip.Id, trip.VehicleId, trip.DriverId, trip.DistanceKm ?? 0, now));
            return Result.Ok(trip);
        }

        public Result Cancel(Guid tripId)
        {
            var trip = _trips.Get(tripId);
            if (trip == null)
            {
                return Result.Fail("Trip not found.");
            }

            var wasInProgress = trip.Status == TripStatus.InProgress;

            try
            {
                trip.Cancel();
            }
            catch (DomainException ex)
            {
                return Result.Fail(ex.Message);
            }

            if (wasInProgress)
            {
                var vehicle = _vehicles.Get(trip.VehicleId);
                vehicle.ReleaseFromTrip();
                _vehicles.Update(vehicle);
            }

            _trips.Update(trip);
            return Result.Ok();
        }
    }
}
