# Changelog

All notable changes to this project are documented here.

## [1.1.0] - 2019-12-31
### Added
- Vehicle categories and licence-class enforcement when planning trips
- Fuel logging service and `/api/vehicles/{id}/fuel` endpoints
- CLI `export` command for vehicles and drivers
- Architecture diagrams, domain rules, concurrency notes and known limitations in the docs

### Fixed
- Servicing a vehicle that was on a trip released it for a second, overlapping trip
- A driver could be on two trips at once
- A trip could be planned for a date after the driver's licence expires, and started after it expired
- Completing a trip could leave it Completed while its vehicle stayed InUse
- A failing event subscriber made a successful trip completion return an error and skipped later subscribers
- Concurrent requests could double-book a vehicle or register duplicate plates or licences
- Utilization counted trips crossing the end of the range in full, reaching more than 100 %
- CSV export was open to spreadsheet formula injection
- API error middleware masked errors after the response had started and did not log 500s
- Report endpoints returned an empty 200 when `from` or `to` was missing

## [1.0.0] - 2019-12-17
### Added
- Domain model: vehicles, drivers, trips, maintenance records, fuel logs
- Services for vehicles, drivers, trips, alerts, cost and utilization reports
- Maintenance scheduler (distance and time based)
- In-memory persistence and CSV export
- ASP.NET Core 2.2 REST API with Swagger
- Command line interface
- xUnit test suite, Travis CI configuration

## [0.3.0] - 2019-09-30
### Added
- REST API and demo data seeder
- CSV export

## [0.2.0] - 2019-06-12
### Added
- Trip lifecycle (plan, start, complete, cancel)
- Maintenance scheduler

## [0.1.0] - 2019-03-04
### Added
- Initial domain model
