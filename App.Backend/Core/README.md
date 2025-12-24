# Backend Core

This project (`Backend.API.Core`) contains the application business logic and services.

## Responsibilities

- **Services**: Implementation of business logic (e.g., `UserService`, `ProjectService`).
- **Interfaces**: Definitions for services to ensure loose coupling.
- **Query Logic**: Helpers for pagination, sorting, and filtering.

It acts as the bridge between the API layer and the Data/Domain layers.

## SSH Benchmark:
Reminder of the benchmarks for creating 1000 repos via a persistent ssh connection

```
[INFO] Connecting to localhost:23232 as backend...
[INFO] Connected. Starting bombardment...
....................................................................................................

------------------------------------------------
[REPORT] Bombardment Complete
Total Requests:   1000
Total Time:       39031 ms
Avg Latency:      2309.22 ms
Min Latency:      177 ms
Max Latency:      7441 ms
Throughput:       25.62 req/sec
------------------------------------------------
```