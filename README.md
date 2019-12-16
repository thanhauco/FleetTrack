# FleetTrack

[![Build Status](https://travis-ci.org/fleettrack/fleettrack.svg?branch=master)](https://travis-ci.org/fleettrack/fleettrack)

FleetTrack is a compact fleet management toolkit written in C# for .NET Core 2.2.
It tracks vehicles, drivers, trips, maintenance and fuel usage, and produces cost
and utilization reports.

## Solution layout

| Project | Description |
| --- | --- |
| `src/FleetTrack.Core` | Domain model, services and abstractions (`netstandard2.0`) |
| `src/FleetTrack.Infrastructure` | In-memory persistence, CSV export, notifications, demo data |
| `src/FleetTrack.Api` | ASP.NET Core 2.2 REST API with Swagger |
| `src/FleetTrack.Cli` | Command line front-end |
| `tests/FleetTrack.Tests` | xUnit test suite |

## Getting started

```sh
dotnet build FleetTrack.sln
dotnet test tests/FleetTrack.Tests
dotnet run --project src/FleetTrack.Api
```

The API listens on `http://localhost:5000` and exposes Swagger UI at `/swagger`.

```sh
dotnet run --project src/FleetTrack.Cli -- vehicles
dotnet run --project src/FleetTrack.Cli -- alerts
```

## Features

- Vehicle registry with status tracking (available, in use, in maintenance, retired)
- Driver licence validation and expiry alerts
- Trip planning, start, completion and cancellation with odometer tracking
- Maintenance scheduling by distance and elapsed time
- Fuel efficiency (L/100 km) and per-vehicle cost reports
- Utilization report across a date range
- In-process domain events (e.g. long trip notifications)

See [docs/architecture.md](docs/architecture.md) and [docs/api.md](docs/api.md) for details.

## License

MIT
