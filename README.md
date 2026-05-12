# SkyRoute — Flight Search & Booking

Flight aggregation and booking module built with .NET 8 and Angular 17.

---

## Overview

SkyRoute is a flight aggregation and booking module that simulates integration with multiple airline providers.

The system allows users to:

- Search flights across multiple providers
- Compare prices using provider-specific pricing rules
- View total and per-passenger pricing
- Book flights with validation based on route type (domestic vs international)

The focus of this implementation is on architecture, testability, and extensibility rather than real airline integrations.

---

## Architecture Overview

The backend follows Clean Architecture with strict separation of concerns:

- **Domain**: Core entities, enums, and interfaces. No external dependencies.
- **Application**: Use cases, validation, and orchestration logic.
- **Infrastructure**: Providers, pricing strategies, repositories, and external concerns.
- **API**: HTTP layer (controllers, DTOs, middleware).

### Dependency Rule

Outer layers depend on inner layers.  
The Domain layer has zero external dependencies.

This structure allows:

- Replacing infrastructure without affecting business logic
- Testing business rules in isolation
- Adding new providers or pricing rules without modifying existing code

---

## Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 · ASP.NET Core Web API |
| Frontend | Angular 17 · Standalone Components |
| Testing | xUnit · Moq |
| Deploy | Docker · docker-compose |

---

# Running the Application

## Docker

```bash
docker-compose up --build

