using System;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Tests.Helpers;
using Xunit;

namespace FleetTrack.Tests.Domain
{
    public class TripTests
    {
        private static Trip NewTrip()
        {
            return new Trip(Guid.NewGuid(), Guid.NewGuid(), TestData.Berlin, TestData.Munich, TestData.Now);
        }

        [Fact]
        public void NewTrip_IsPlanned()
        {
            var trip = NewTrip();

            Assert.Equal(TripStatus.Planned, trip.Status);
            Assert.Null(trip.DistanceKm);
            Assert.Null(trip.Duration);
        }

        [Fact]
        public void Constructor_RequiresIds()
        {
            Assert.Throws<ArgumentException>(() => new Trip(Guid.Empty, Guid.NewGuid(), TestData.Berlin, TestData.Munich, TestData.Now));
            Assert.Throws<ArgumentException>(() => new Trip(Guid.NewGuid(), Guid.Empty, TestData.Berlin, TestData.Munich, TestData.Now));
        }

        [Fact]
        public void Lifecycle_ComputesDistanceAndDuration()
        {
            var trip = NewTrip();
            trip.Start(TestData.Now, 1000);
            trip.Complete(TestData.Now.AddHours(6), 1585.5);

            Assert.Equal(TripStatus.Completed, trip.Status);
            Assert.Equal(585.5, trip.DistanceKm);
            Assert.Equal(TimeSpan.FromHours(6), trip.Duration);
        }

        [Fact]
        public void Complete_RequiresStartedTrip()
        {
            Assert.Throws<DomainException>(() => NewTrip().Complete(TestData.Now, 10));
        }

        [Fact]
        public void Complete_RejectsLowerOdometer()
        {
            var trip = NewTrip();
            trip.Start(TestData.Now, 1000);

            Assert.Throws<DomainException>(() => trip.Complete(TestData.Now.AddHours(1), 999));
        }

        [Fact]
        public void Cancel_NotAllowedAfterCompletion()
        {
            var trip = NewTrip();
            trip.Start(TestData.Now, 0);
            trip.Complete(TestData.Now.AddHours(1), 50);

            Assert.Throws<DomainException>(() => trip.Cancel());
        }

        [Fact]
        public void Cancel_PlannedTrip()
        {
            var trip = NewTrip();
            trip.Cancel();

            Assert.Equal(TripStatus.Cancelled, trip.Status);
        }
    }
}
