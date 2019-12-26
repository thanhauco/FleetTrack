using System.Linq;
using FleetTrack.Api.Models;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;

namespace FleetTrack.Api.Mapping
{
    public static class Mappers
    {
        public static VehicleDto ToDto(this Vehicle v)
        {
            return new VehicleDto
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                FuelType = v.FuelType.ToString(),
                Category = v.Category.ToString(),
                Status = v.Status.ToString(),
                OdometerKm = v.OdometerKm,
                LastServiceUtc = v.LastServiceUtc
            };
        }

        public static DriverDto ToDto(this Driver d)
        {
            return new DriverDto
            {
                Id = d.Id,
                FullName = d.FullName,
                LicenseNumber = d.LicenseNumber,
                LicenseClass = d.LicenseClass.ToString(),
                LicenseExpiryUtc = d.LicenseExpiryUtc,
                IsActive = d.IsActive
            };
        }

        public static TripDto ToDto(this Trip t)
        {
            return new TripDto
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                DriverId = t.DriverId,
                Status = t.Status.ToString(),
                PlannedStartUtc = t.PlannedStartUtc,
                StartedUtc = t.StartedUtc,
                CompletedUtc = t.CompletedUtc,
                DistanceKm = t.DistanceKm
            };
        }

        public static MaintenanceDto ToDto(this MaintenanceRecord r)
        {
            return new MaintenanceDto
            {
                Id = r.Id,
                VehicleId = r.VehicleId,
                Type = r.Type.ToString(),
                PerformedUtc = r.PerformedUtc,
                OdometerKm = r.OdometerKm,
                Cost = r.Cost.Amount,
                Currency = r.Cost.Currency,
                Notes = r.Notes
            };
        }

        public static FuelLogDto ToDto(this FuelLog l)
        {
            return new FuelLogDto
            {
                Id = l.Id,
                VehicleId = l.VehicleId,
                DateUtc = l.DateUtc,
                Liters = l.Liters,
                Cost = l.Cost.Amount,
                Currency = l.Cost.Currency,
                OdometerKm = l.OdometerKm
            };
        }

        public static PagedResponse<TDto> ToResponse<T, TDto>(this PagedResult<T> page, System.Func<T, TDto> map)
        {
            return new PagedResponse<TDto>
            {
                Items = page.Items.Select(map).ToList(),
                Page = page.Page,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
                TotalPages = page.TotalPages
            };
        }
    }
}
