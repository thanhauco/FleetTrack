# REST API

Base URL: `http://localhost:5000`. Swagger UI is available at `/swagger`.

| **Method** | **Route** | **Description** |
| --- | --- | --- |
| GET | `/api/vehicles?q=&page=&pageSize=` | Search vehicles |
| GET | `/api/vehicles/{id}` | Get a vehicle |
| POST | `/api/vehicles` | Register a vehicle (optional `category`, default `Car`) |
| POST | `/api/vehicles/{id}/retire` | Retire a vehicle |
| GET | `/api/vehicles/{id}/maintenance` | Maintenance history |
| POST | `/api/vehicles/{id}/maintenance` | Record a service (409 while the vehicle is on a trip) |
| GET | `/api/vehicles/{id}/fuel` | Fuel log |
| POST | `/api/vehicles/{id}/fuel` | Record a fill-up (optional `filledUtc` for late receipts) |
| GET | `/api/vehicles/{id}/fuel/summary` | Fill count, litres, L/100 km, km/L |
| GET | `/api/drivers/{id}` | Get a driver |
| GET | `/api/drivers/expiring?days=30` | Licences expiring soon |
| POST | `/api/drivers` | Hire a driver |
| POST | `/api/drivers/{id}/deactivate` | Deactivate a driver |
| POST | `/api/trips` | Plan a trip |
| POST | `/api/trips/{id}/start` | Start a trip |
| POST | `/api/trips/{id}/complete` | Complete a trip with the end odometer |
| POST | `/api/trips/{id}/cancel` | Cancel a trip |
| GET | `/api/reports/alerts` | Maintenance and licence alerts |
| GET | `/api/reports/vehicles/{id}/costs?from=&to=` | Cost report (`from` and `to` required) |
| GET | `/api/reports/utilization?from=&to=` | Utilization report (`from` and `to` required) |
| GET | `/api/health` | Health check |

## Vehicle categories and licences

| **Category** | **Minimum licence** | **Also drivable with** |
| --- | --- | --- |
| `Car` | B | C, CE, D |
| `Van` (up to 3.5 t) | B | C, CE, D |
| `Truck` | C | CE |
| `ArticulatedTruck` | CE | none |
| `Bus` | D | none |

Planning a trip for a driver whose licence does not cover the vehicle returns 400.

## Errors

Errors are returned as `{ "error": "message" }`.

| **Status** | **When** |
| --- | --- |
| 400 | Validation failure, business-rule violation, or `DomainException` / `ArgumentException` |
| 404 | Unknown vehicle, driver or trip |
| 409 | Duplicate plate or licence number, or servicing a vehicle that is on a trip |
| 500 | Unexpected error (logged with method and path) |
