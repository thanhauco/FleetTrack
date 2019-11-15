# Architecture

FleetTrack follows a simple layered design:

```
Api / Cli  ->  Core (domain + services)  <-  Infrastructure
```

* **Core** has no external dependencies. It contains entities (`Vehicle`, `Driver`, `Trip`,
  `MaintenanceRecord`, `FuelLog`), value objects (`Money`, `GeoPoint`, `DateRange`), repository
  interfaces and the application services.
* **Infrastructure** implements the repository interfaces in memory, plus CSV export,
  console notifications and demo data.
* **Api** and **Cli** are thin front-ends that compose the object graph.

## Domain rules

* A vehicle can only be assigned to one trip at a time.
* Odometers never move backwards.
* A driver needs an active, unexpired licence to be planned onto a trip.
* Retired vehicles cannot be used or serviced.

## Events

`TripService` publishes a `TripCompletedEvent` through `IEventBus`. `LongTripNotifier` listens
for it and sends a notification when the trip exceeds a configurable distance.

## Error handling

Expected failures are returned as `Result` / `Result<T>`. `DomainException` is reserved for
broken invariants; the API translates it into HTTP 400 in `ErrorHandlingMiddleware`.
