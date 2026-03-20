# SAPL .NET Demo Application

Demo application showing SAPL policy enforcement integrated with ASP.NET Core. The demo connects to a SAPL PDP and a Keycloak identity provider running in Docker.

## Features

This demo covers the full range of SAPL enforcement patterns:

| Category | Endpoints | Description |
|----------|-----------|-------------|
| Basic enforcement | `GET /api/hello`, `GET /api/patient/{id}`, `GET /api/patients` | Pre-enforce and post-enforce attributes |
| Constraint handlers | `GET /api/constraints/*` | Redaction, filtering, logging, auditing, timestamps, error handling, resource replacement |
| Service-layer enforcement | `GET /api/services/*` | Programmatic enforcement in service classes (not just controllers) |
| JWT export | `GET /api/exportData/{pilotId}/{sequenceId}` | Bearer-token authorization with secrets |
| SSE streaming | `GET /api/streaming/heartbeat/*` | Till-denied, drop-while-denied, and recoverable streaming enforcement |

### Constraint Handler Types Demonstrated

| Handler | Type | Constraint | Behavior |
|---------|------|------------|----------|
| `LogAccessHandler` | Runnable | `logAccess` | Logs policy decisions |
| `AuditTrailHandler` | Consumer | `auditTrail` | Records responses to in-memory audit log |
| `RedactFieldsHandler` | Mapping | `redactFields` | Replaces sensitive fields with `[REDACTED]` |
| `ClassificationFilterHandler` | Filter predicate | `filterByClassification` | Filters documents by classification level |
| `InjectTimestampHandler` | Method invocation | `injectTimestamp` | Injects policy timestamp into response |
| `CapTransferHandler` | Method invocation | `capTransferAmount` | Caps transfer amounts to policy-defined maximum |
| `NotifyOnErrorHandler` | Error handler | `notifyOnError` | Logs warnings on errors in protected operations |
| `EnrichErrorHandler` | Error mapping | `enrichError` | Wraps exceptions with support URL |

## Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK

## Quick Start

### 1. Start infrastructure

```
docker compose up -d
```

This starts a SAPL PDP on `http://localhost:8443` and Keycloak on `http://localhost:8080`. Keycloak takes about 30 seconds to import the realm on first start. Wait until `curl -s http://localhost:8080/realms/demo` returns JSON before running the app.

### 2. Install dependencies and run

```
dotnet run
```

The API is available at `http://localhost:3000`.

### 3. Test unauthenticated endpoints

```
curl http://localhost:3000/api/hello
curl http://localhost:3000/api/patients
curl http://localhost:3000/api/patient/P-001
```

### 4. Test constraint handlers

```
curl http://localhost:3000/api/constraints/redacted
curl http://localhost:3000/api/constraints/documents
curl http://localhost:3000/api/constraints/logged
curl http://localhost:3000/api/constraints/audited
curl http://localhost:3000/api/constraints/audit-log
curl http://localhost:3000/api/constraints/timestamped
```

### 5. Test streaming enforcement

```
curl -N http://localhost:3000/api/streaming/heartbeat/till-denied
curl -N http://localhost:3000/api/streaming/heartbeat/drop-while-denied
curl -N http://localhost:3000/api/streaming/heartbeat/recoverable
```

## Getting a Token

To test JWT-authenticated endpoints (export):

```
TOKEN=$(curl -s -X POST http://localhost:8080/realms/demo/protocol/openid-connect/token -H "Content-Type: application/x-www-form-urlencoded" -d "grant_type=password&client_id=demo-app&client_secret=dev-secret&username=clinician1&password=password" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")
```

Then use it:

```
curl -H "Authorization: Bearer $TOKEN" http://localhost:3000/api/exportData/1/42
```

### Test Users

All passwords are `password`.

| Username | Role | pilotId |
|----------|------|---------|
| clinician1 | CLINICIAN | 1 |
| clinician2 | CLINICIAN | 2 |
| participant1 | PARTICIPANT | 1 |
| participant2 | PARTICIPANT | 2 |

## Architecture

```
sapl-dotnet-demos/
  Controllers/          API controllers with @PreEnforce / @PostEnforce attributes
  Handlers/             Constraint handler implementations (one per handler type)
  Services/             Service-layer enforcement with EnforcementEngine
  Data/                 In-memory patient data
  policies/             SAPL policy files loaded by the PDP
  keycloak/             Keycloak realm export (users, client, mappers)
  docker-compose.yml    PDP + Keycloak infrastructure
```

## Related

- Library: [heutelbeck/sapl-dotnet](https://github.com/heutelbeck/sapl-dotnet)
- Documentation: [sapl.io](https://sapl.io)
- NuGet packages: [Sapl.Core](https://www.nuget.org/packages/Sapl.Core), [Sapl.AspNetCore](https://www.nuget.org/packages/Sapl.AspNetCore)

## License

[Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0)
