# Command line interface

```sh
dotnet run --project src/FleetTrack.Cli -- <command>
```

| **Command** | **Description** |
| --- | --- |
| `vehicles` | List vehicles with category, status and odometer |
| `alerts` | Maintenance and licence alerts |
| `report [days]` | Cost report per vehicle (default 90 days) |
| `utilization [days]` | Utilization per vehicle (default 30 days) |
| `simulate` | Plan, start and complete a Berlin to Munich trip with a qualified driver |
| `export vehicles\|drivers [file]` | CSV export to stdout, or to `file` |

Commands return exit code 0 on success and 1 on invalid arguments or failure.

The CLI runs against in-memory data seeded by `DemoDataSeeder`, so every invocation starts
fresh. For example, a trip made by `simulate` does not appear in a later `utilization` run.

CSV cells that begin with `=`, `+`, `-` or `@` are prefixed with an apostrophe. This stops
spreadsheets from evaluating them as formulas.
