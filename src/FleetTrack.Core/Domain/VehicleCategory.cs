namespace FleetTrack.Core.Domain
{
    /// <summary>
    /// Licensing category of a vehicle. Values follow a simplified EU model.
    /// </summary>
    public enum VehicleCategory
    {
        /// <summary>Passenger car.</summary>
        Car = 0,

        /// <summary>Van up to 3.5 t gross weight, drivable on a B licence.</summary>
        Van = 1,

        /// <summary>Rigid truck over 3.5 t.</summary>
        Truck = 2,

        /// <summary>Tractor unit with semi-trailer, or truck with heavy trailer.</summary>
        ArticulatedTruck = 3,

        /// <summary>Vehicle with more than eight passenger seats.</summary>
        Bus = 4
    }
}
