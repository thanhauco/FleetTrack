using System;
using System.ComponentModel.DataAnnotations;
using FleetTrack.Core.Domain;

namespace FleetTrack.Api.Models
{
    public class DriverDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }

        public string LicenseNumber { get; set; }

        public string LicenseClass { get; set; }

        public DateTime LicenseExpiryUtc { get; set; }

        public bool IsActive { get; set; }
    }

    public class HireDriverRequest
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        public LicenseClass LicenseClass { get; set; }

        public DateTime LicenseExpiryUtc { get; set; }
    }
}
