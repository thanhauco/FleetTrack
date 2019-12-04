#!/usr/bin/env bash
set -euo pipefail

configuration="${1:-Release}"

dotnet restore FleetTrack.sln
dotnet build FleetTrack.sln -c "$configuration" --no-restore
dotnet test tests/FleetTrack.Tests -c "$configuration" --no-build
