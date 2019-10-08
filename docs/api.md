# REST API

Base URL: `http://localhost:5000`. Swagger UI is available at `/swagger`.

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/vehicles?q=&page=&pageSize=` | Search vehicles |
| GET | `/api/vehicles/{id}` | Get a vehicle |
| POST | `/api/vehicles` | Register a vehicle |
| POST | `/api/vehicles/{id}/retire` | Retire a vehicle |
| GET | `/api/vehicles/{id}/maintenance` | Maintenance history |
| POST | `/api/vehicles/{id}/maintenance` | Record a service |
| GET | `/api/drivers/{id}` | Get a driver |
| GET | `/api/drivers/expiring?days=30` | Licences expiring soon |
| POST | `/api/drivers` | Hire a driver |
| POST | `/api/drivers/{id}/deactivate` | Deactivate a driver |
| POST | `/api/trips` | Plan a trip |
| POST | `/api/trips/{id}/start` | Start a trip |
| POST | `/api/trips/{id}/complete` | Complete a trip with the end odometer |
| POST | `/api/trips/{id}/cancel` | Cancel a trip |
| GET | `/api/reports/alerts` | Maintenance and licence alerts |
| GET | `/api/reports/vehicles/{id}/costs?from=&to=` | Cost report |
| GET | `/api/reports/utilization?from=&to=` | Utilization report |
| GET | `/api/health` | Health check |

Errors are returned as `{ "error": "message" }`.
