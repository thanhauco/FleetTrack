using System;

namespace FleetTrack.Core.Domain
{
    /// <summary>
    /// Which licence classes may drive which vehicle categories.
    /// </summary>
    /// <remarks>
    /// Simplified EU rules: C, CE and D are only issued to holders of a B licence,
    /// so every class may drive cars and light vans; CE includes C; D covers buses
    /// only. Sub-categories (C1, D1, BE ...) and national exceptions are not modelled.
    /// </remarks>
    public static class LicensePolicy
    {
        public static bool CanDrive(LicenseClass licence, VehicleCategory category)
        {
            switch (category)
            {
                case VehicleCategory.Car:
                case VehicleCategory.Van:
                    return true;
                case VehicleCategory.Truck:
                    return licence == LicenseClass.C || licence == LicenseClass.CE;
                case VehicleCategory.ArticulatedTruck:
                    return licence == LicenseClass.CE;
                case VehicleCategory.Bus:
                    return licence == LicenseClass.D;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown vehicle category.");
            }
        }

        public static LicenseClass MinimumFor(VehicleCategory category)
        {
            switch (category)
            {
                case VehicleCategory.Car:
                case VehicleCategory.Van:
                    return LicenseClass.B;
                case VehicleCategory.Truck:
                    return LicenseClass.C;
                case VehicleCategory.ArticulatedTruck:
                    return LicenseClass.CE;
                case VehicleCategory.Bus:
                    return LicenseClass.D;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown vehicle category.");
            }
        }
    }
}
