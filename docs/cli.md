# Command line interface

```sh
dotnet run --project src/FleetTrack.Cli -- <command>
```

| Command | Description |
| --- | --- |
| `vehicles` | List vehicles |
| `alerts` | Maintenance and licence alerts |
| `report [days]` | Cost report per vehicle (default 90 days) |
| `utilization [days]` | Utilization per vehicle (default 30 days) |
| `simulate` | Plan, start and complete a Berlin to Munich trip |

The CLI runs against in-memory data seeded by `DemoDataSeeder`, so every invocation starts fresh.
