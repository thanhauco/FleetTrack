# Architecture

FleetTrack is a single-process application with a dependency-free core. The API and the
CLI are two thin front-ends over the same services; persistence is in memory.

## Component overview

```mermaid
%%{init: {"theme": "base", "themeVariables": {"fontFamily": "Segoe UI, Helvetica, Arial, sans-serif", "lineColor": "#5B6475", "edgeLabelBackground": "#FFFFFF"}}}%%
flowchart LR
    subgraph clients["Clients"]
        direction TB
        http["REST client<br/>(browser, curl, Swagger UI)"]:::client
        operator["Operator<br/>(terminal)"]:::client
    end

    subgraph entry["Entry points"]
        direction TB
        mw["ErrorHandlingMiddleware<br/>400 / 500 mapping"]:::risk
        ctrl["API controllers<br/>vehicles · drivers · trips<br/>maintenance · fuel · reports"]:::entry
        router["CLI CommandRouter<br/>vehicles · alerts · report<br/>utilization · simulate · export"]:::entry
    end

    subgraph core["FleetTrack.Core (netstandard2.0, no dependencies)"]
        direction TB
        ops["Operational services<br/>TripService · FuelService<br/>VehicleService · DriverService"]:::core
        reports["Read-side services<br/>CostReport · Utilization<br/>Alerts · MaintenanceScheduler"]:::core
        domain["Domain model<br/>Vehicle · Driver · Trip · FuelLog<br/>Money · GeoPoint · LicensePolicy"]:::core
        bus["InProcessEventBus"]:::core
        longtrip["LongTripNotifier"]:::core
        ports["Repository + INotifier<br/>interfaces"]:::core
    end

    subgraph infra["FleetTrack.Infrastructure"]
        direction TB
        repos["In-memory repositories<br/>(ConcurrentDictionary)"]:::infra
        csv["CsvExporter"]:::infra
        notifier["ConsoleNotifier"]:::infra
        seed["DemoDataSeeder"]:::infra
    end

    http -->|"HTTP / JSON"| mw --> ctrl
    operator -->|"argv"| router
    ctrl -->|"sync calls, Result&lt;T&gt;"| ops
    ctrl --> reports
    router --> ops
    router --> reports
    router --> csv
    ops --> domain
    reports --> domain
    ops --> ports
    reports --> ports
    ops -.->|"TripCompletedEvent<br/>after commit"| bus
    bus -.->|"dispatch"| longtrip
    longtrip -.->|"Notify"| ports
    repos -->|"implements"| ports
    notifier -->|"implements"| ports
    seed --> repos

    classDef client fill:#69D0BF,stroke:#2F8F7F,color:#0E2E28,rx:10,ry:10
    classDef entry fill:#EC8131,stroke:#B35A17,color:#2B1300,rx:10,ry:10
    classDef core fill:#4D7FEE,stroke:#2C55B8,color:#0A1838,rx:10,ry:10
    classDef infra fill:#F3EFE6,stroke:#B9AE96,color:#2E2A22,rx:10,ry:10
    classDef risk fill:#E94E5E,stroke:#A92736,color:#2B0006,rx:10,ry:10
    style clients fill:#FFFFFF,stroke:#69D0BF
    style entry fill:#FFFFFF,stroke:#EC8131
    style core fill:#FFFFFF,stroke:#4D7FEE
    style infra fill:#FFFFFF,stroke:#B9AE96
```

| **Colour** | **Layer** | **Meaning** |
|---|---|---|
| Teal | Clients | Callers outside the process |
| Orange | Entry points | Translate requests into service calls |
| Royal blue | Core | Business rules; no I/O, no third-party packages |
| Cream | Infrastructure | Replaceable implementations of Core interfaces |
| Coral | Error boundary | Where exceptions become HTTP status codes |

Solid arrows are synchronous calls; dashed arrows are the in-process event flow.

## Trip lifecycle

```mermaid
%%{init: {"theme": "base", "themeVariables": {"fontFamily": "Segoe UI, Helvetica, Arial, sans-serif", "lineColor": "#5B6475", "edgeLabelBackground": "#FFFFFF"}}}%%
stateDiagram-v2
    direction LR
    [*] --> Planned : Plan<br/>licence valid now and on planned date<br/>licence class covers vehicle
    Planned --> InProgress : Start<br/>driver idle, vehicle Available
    InProgress --> Completed : Complete(end odometer)<br/>end ≥ start and ≥ vehicle reading
    Planned --> Cancelled : Cancel
    InProgress --> Cancelled : Cancel<br/>vehicle released
    Completed --> [*]
    Cancelled --> [*]

    classDef planned fill:#F3EFE6,stroke:#B9AE96,color:#2E2A22
    classDef active fill:#EC8131,stroke:#B35A17,color:#2B1300
    classDef done fill:#69D0BF,stroke:#2F8F7F,color:#0E2E28
    classDef stopped fill:#E94E5E,stroke:#A92736,color:#2B0006
    class Planned planned
    class InProgress active
    class Completed done
    class Cancelled stopped
```

Completed and Cancelled are terminal. A vehicle is `InUse` exactly while one of its trips is
`InProgress`; servicing or retiring it in that state is rejected.

## Starting a trip

Every check runs inside `TripService`'s gate before anything is mutated, so a failed start
leaves the trip, the vehicle and the driver exactly as they were.

```mermaid
%%{init: {"theme": "base", "themeVariables": {"fontFamily": "Segoe UI, Helvetica, Arial, sans-serif", "lineColor": "#5B6475", "edgeLabelBackground": "#FFFFFF"}}}%%
flowchart TD
    start(["Start(tripId)"]):::entry --> gate["Acquire TripService gate"]:::core
    gate --> q1{"Trip exists and<br/>is Planned?"}:::check
    q1 -->|NO| f1["Fail: not found / wrong status"]:::fail
    q1 -->|YES| q2{"Driver active and<br/>licence valid today?"}:::check
    q2 -->|NO| f2["Fail: inactive or expired licence"]:::fail
    q2 -->|YES| q3{"Driver already on a<br/>trip in progress?"}:::check
    q3 -->|YES| f3["Fail: driver busy"]:::fail
    q3 -->|NO| q4{"Vehicle Available?"}:::check
    q4 -->|NO| f4["Fail: vehicle not available"]:::fail
    q4 -->|YES| mutate["vehicle.AssignToTrip()<br/>trip.Start(now, odometer)"]:::core
    mutate --> ok(["Ok: trip InProgress"]):::ok
    f1 & f2 & f3 & f4 --> unchanged["No state changed"]:::neutral

    classDef entry fill:#EC8131,stroke:#B35A17,color:#2B1300
    classDef core fill:#4D7FEE,stroke:#2C55B8,color:#0A1838
    classDef check fill:#F3EFE6,stroke:#B9AE96,color:#2E2A22
    classDef fail fill:#E94E5E,stroke:#A92736,color:#2B0006
    classDef ok fill:#69D0BF,stroke:#2F8F7F,color:#0E2E28
    classDef neutral fill:#FFFFFF,stroke:#B9AE96,color:#2E2A22,stroke-dasharray: 4 3
```

## Completing a trip and publishing the event

```mermaid
%%{init: {"theme": "base", "themeVariables": {"fontFamily": "Segoe UI, Helvetica, Arial, sans-serif", "actorBkg": "#4D7FEE", "actorBorder": "#2C55B8", "actorTextColor": "#0A1838", "actorLineColor": "#5B6475", "signalColor": "#2E2A22", "signalTextColor": "#2E2A22", "noteBkgColor": "#F3EFE6", "noteBorderColor": "#B9AE96", "noteTextColor": "#2E2A22", "activationBkgColor": "#EC8131", "activationBorderColor": "#B35A17", "sequenceNumberColor": "#FFFFFF"}}}%%
sequenceDiagram
    autonumber
    participant C as TripsController
    participant S as TripService
    participant R as Repositories
    participant B as InProcessEventBus
    participant H as LongTripNotifier
    participant N as INotifier

    C->>S: Complete(tripId, endOdometerKm)
    activate S
    Note over S: lock(gate)
    S->>R: load trip and vehicle
    alt status ≠ InProgress, or end < start, or end < vehicle odometer
        S-->>C: Result.Fail (nothing mutated)
    else all checks pass
        S->>R: trip.Complete, vehicle.UpdateOdometer, ReleaseFromTrip
        Note over S: release gate before publishing,<br/>so a handler can call back without deadlock
        S->>B: Publish(TripCompletedEvent)
        B->>H: handler(event)
        opt distance ≥ threshold
            H->>N: Notify("Long trip completed")
        end
        Note over B,N: A handler exception is caught and logged,<br/>remaining handlers still run
        S-->>C: Result.Ok(trip)
    end
    deactivate S
```

## Domain rules

| **Rule** | **Enforced in** | **Why** |
|---|---|---|
| A vehicle is on at most one trip at a time | `Vehicle.AssignToTrip` (atomic) | Physical constraint |
| A driver is on at most one trip at a time | `TripService.Start` | A planned overlap may be resolved; a driven one cannot |
| Licence must be valid now, on the planned date and at start | `TripService.Plan`, `TripService.Start` | Licences expire between booking and departure |
| Licence class must cover the vehicle category | `LicensePolicy`, `TripService.Plan` | Simplified EU B / C / CE / D model |
| Odometers never move backwards | `Vehicle.UpdateOdometer`, `TripService.Complete` | Catches mistyped readings |
| A vehicle on a trip cannot be serviced or retired | `Vehicle.CompleteMaintenance`, `Vehicle.Retire` | Servicing would release it for a second trip |
| A fill-up must fit between its neighbours in time | `FuelService.RecordFill` | Receipts are entered late and out of order |

## Concurrency

The API registers services and repositories as singletons, and the in-memory repositories
hand out live entity instances. Every operation that checks state and then changes it
holds a lock:

| **Scope** | **Lock** | **Protects** |
|---|---|---|
| One vehicle | `Vehicle._sync` | Status transitions and odometer updates |
| Trip lifecycle | `TripService._gate` | Cross-aggregate checks (driver idle, vehicle free) |
| Registration | `VehicleService` / `DriverService` gates | Plate and licence-number uniqueness |
| Fuel log | `FuelService._gate` | Neighbour validation and odometer advance |

These locks are correct only within a single process. A database-backed deployment would
replace them with transactions plus unique constraints or optimistic concurrency.

## Error handling

Expected failures are returned as `Result` / `Result<T>`. `DomainException` marks a broken
invariant; `ErrorHandlingMiddleware` maps it, and `ArgumentException`, to HTTP 400, logs
anything else as a 500, and rethrows if the response has already started.

## Known limitations

| **Area** | **Limitation** | **Possible next step** |
|---|---|---|
| Persistence | In memory; data is lost on restart | EF Core repositories behind the same interfaces |
| Maintenance | Time-based interval starts only after the first recorded service | Track a commissioning date per vehicle |
| Cost per km | Includes the first fill-up's cost, whose fuel was burnt before the period | Pro-rate by the full-tank method, as fuel efficiency already does |
| Trip distance | Odometer-based only | No plausibility check against the great-circle distance, since ferries and trains move vehicles without odometer progress |
