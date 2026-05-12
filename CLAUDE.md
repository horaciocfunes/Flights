# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Commands

### Backend

First-time setup (creates `SkyRoute.sln` and links all projects):
```bash
cd backend
.\setup.ps1
```

Run the API:
```bash
dotnet run --project backend/src/SkyRoute.API
```

Run all tests:
```bash
cd backend
dotnet test
```

Run a single test class:
```bash
dotnet test --filter "FullyQualifiedName~GlobalAirPricingStrategyTests"
```

Build only:
```bash
dotnet build backend/src/SkyRoute.API
```

### Frontend

```bash
cd frontend
npm install
npm start          # http://localhost:4200
npm run build      # production build to dist/skyroute-frontend/browser
```

### Docker

```bash
docker-compose up --build
```

API: `http://localhost:5000` · Swagger: `http://localhost:5000/swagger` · Frontend: `http://localhost:4200`

---

## Architecture

### Backend — Clean Architecture

Four projects with a strict inward dependency rule:

```
SkyRoute.API  →  SkyRoute.Application  →  SkyRoute.Domain
                 SkyRoute.Infrastructure →  SkyRoute.Domain
                 SkyRoute.Infrastructure →  SkyRoute.Application
```

- **Domain** — entities (`Flight`, `Booking`, `Passenger`, `Airport`), enums, and interfaces (`IFlightProvider`, `IPricingStrategy`, `IBookingRepository`, `IAirportRepository`). Zero external dependencies.
- **Application** — use cases (`FlightSearchService`, `BookingService`), `BookingRequestValidator`, application models (`FlightResult`, `BookingRequest`), and exception types. Depends only on Domain.
- **Infrastructure** — implements Domain interfaces: two providers (`GlobalAirProvider`, `BudgetWingsProvider`), two pricing strategies, two in-memory repositories. All DI wiring lives in `ServiceCollectionExtensions.AddSkyRouteServices()`.
- **API** — controllers, DTOs, and `ErrorHandlingMiddleware`. Controllers are thin: map DTO → application model, call one service method, return result. No business logic, no try/catch.

### Key design constraints

**Adding a provider** requires only two things: a new class implementing `IFlightProvider` (with its `IPricingStrategy`), and one `AddSingleton<IFlightProvider, NewProvider>()` line in `ServiceCollectionExtensions`. `FlightSearchService` receives `IEnumerable<IFlightProvider>` and aggregates all of them automatically.

**Pricing strategies** are registered as their concrete types (`GlobalAirPricingStrategy`, not `IPricingStrategy`) because each provider injects its own strategy by concrete type. The strategy is applied by `FlightSearchService` at aggregation time, not by the provider — this keeps pricing independently testable.

**`IsInternational`** is a computed property on `Flight` (`Origin.CountryCode != Destination.CountryCode`). `BookingsController` never sets it on `BookingRequest`. `BookingService` overwrites it from the cached `FlightResult` before validation runs — the client cannot influence which document type is required.

**`BookingRequestValidator`** runs in two phases: `RuleForEach` for per-passenger basics, then a top-level `RuleFor(x => x).Custom(...)` for document format. The split is necessary because `ChildRules` closures only receive the child (`PassengerInfo`), not the parent (`BookingRequest`), making `IsInternational` inaccessible from inside `RuleForEach`.

**Error handling** is centralised in `ErrorHandlingMiddleware`: `FlightNotFoundException` → 404, `BookingValidationException` → 400 with structured field errors, anything else → 500.

**Flight results** are cached in `IMemoryCache` keyed by `FlightId` (30-minute TTL) after each search. `BookingService` reads from this cache — if the TTL has expired, a `FlightNotFoundException` is thrown.

### Frontend — Angular 17 Standalone

No NgModules. All components are standalone. Routes use lazy `loadComponent`.

- `core/services/search-state.service.ts` — in-memory singleton that passes the selected `Flight` from results to the booking form. If it is null when `/book` loads, the component redirects to `/`.
- `core/services/airport.service.ts` — fetches airports once and replays via `shareReplay(1)`.
- The booking form renders one form group per passenger using `FormArray`. The `documentLabel` field comes from the API response and is bound directly as the input label — no conditional logic in the template.

### Test project

`SkyRoute.Tests` references Domain, Application, and Infrastructure directly. `IMemoryCache` is not mocked — `new MemoryCache(new MemoryCacheOptions())` is used instead because `TryGetValue` is an extension method that Moq cannot intercept. `IBookingRepository` is mocked with Moq.
