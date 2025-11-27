#!/usr/bin/env bash
set -euo pipefail
dotnet ef database update initial --project "./src/Backend/Backend.API.Infrastructure"
