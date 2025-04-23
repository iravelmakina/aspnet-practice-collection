# ASP.NET Core Course Project

## Overview
This repository contains all the **lab projects implemented during an ASP.NET Core course**, structured as a single, production-grade solution. It covers everything from CRUD and database modeling to JWT authentication, middleware, and deployment.

- Fully modular ASP.NET Core Web API project
- Multi-layer architecture with Controllers, Services, Infrastructure, and EF Core integration
- Includes PostgreSQL, Redis, Polly, JWT, Google OAuth, Middleware, Filters, and Decorators
- Unit tests with xUnit and Moq

---

## Labs & Features Implemented

### Lab 1: CRUD Operations with Pagination
- Implemented CRUD for domain entities: `Table`, `Reservation`
- Pagination feature for `Get` endpoint (`Table`)
- Unit tested all CRUD logic

### Lab 2: Migrate from Minimal API to Controllers
- Replaced minimal APIs with Controllers using `ControllerBase`
- Injected services via constructor with DI
- Dynamic options configuration (e.g., table settings)
- Unit tested all Controllers

### Lab 3: PostgreSQL + EF Core
- Replaced in-memory storage with PostgreSQL using EF Core
- Defined one-to-one and one-to-many relationships (e.g., Location - Table)
- Used migrations to create/update database schema
- `UseInMemoryDatabase` in unit tests for isolation

### Lab 4: Middleware & Filters
- Custom logging middleware (HTTP method, path, timestamp, status)
- Action filter for `X-API-KEY` authorization from config dictionary
- Exception-handling middleware with structured JSON errors
- Result filter to append `X-Timestamp` to all responses

### Lab 5: Authentication & Authorization
- Implemented CRUD for domain entity `User`
- Login and Registration with hashed + salted passwords
- JWT-based access token system with refresh token mechanism
- Logout and token revocation support
- IP/User-Agent validation for refresh token exchange
- Google OAuth integration + login provider tracking
- Role-based access control
- Password reset system:
  - `forget-password` generates reset code
  - `reset-password` validates it and updates password

### Lab 6: Resilience, Hosted Services & Email
- Typed `HttpClient` with Polly retry/circuit breaker policies
- Custom `HttpClientLogHandler`
- Background migration runner using `IHostedService`
- Password reset code sent to Webhook.site (mock email delivery)
- Background service cleans up reset codes older than 10 minutes
- Optional: SendGrid email integration using raw `HttpClient` (no SDKs)

### Lab 7: Caching, Rate Limiting, ETags
- IMemoryCache Decorator for `GetTables` logic
- Redis-based rate limiting (per route/IP) with middleware and attribute filters
- ETag response caching:
  - Adds `ETag` to headers
  - Returns 304 Not Modified if `If-None-Match` is valid

### Final Lab: Logging, Faker, Deployment
- Replaced all `Console.WriteLine` with `ILogger<T>` using appropriate levels
- Unit tests for `IUserService` (covering all edge cases)
- `/api/dev/generate-random` endpoint returns sample data using fake generator
- Deployed API to Render.com (Swagger link provided in PR)

---

## Tech Stack
- ASP.NET Core Web API
- Entity Framework Core (PostgreSQL)
- xUnit + Moq
- StackExchange.Redis
- IMemoryCache
- Polly (resilient HTTP)
- Google OAuth
- JWT Authentication
- SendGrid (raw REST API)
- Git Flow branching model

---

## Structure
```
DNET.Backend.Api/                            # Main Web API project
├── Controllers/                             # API endpoints
├── Infrastructure/                          # Middleware, filters, loggers
├── Interfaces/                              # Service contracts
├── Models/                                  # Core models (DTOs, exceptions)
├── Options/                                 # Bound settings from config
├── Requests/                                # Incoming DTOs
├── Responses/                               # Outgoing DTOs
├── Services/                                # Business logic & background services
├── appsettings*.json                        # Environment-specific config
├── Program.cs                               # App entry point
└──DNET.Backend.Api.csproj                   # Project file 

DNET.Backend.DataAccess/                     # EF Core infrastructure layer
├── TableReservationsDbContext.cs            # Main DbContext
├── Configurations/                          # Fluent API entity configurations
├── Domain/                                  # EF Core entities
├── Migrations/                              # Auto-generated migrations
└── DNET.Backend.DataAccess.csproj           # Project file

DNET.Backend.Api.Tests/                      # Unit test project
├── ReservationServiceTests.cs
├── TableServiceTests.cs
├── UserServiceTests.cs
├── Utils.cs
└── DNET.Backend.Api.Tests.csproj
```

---

## How to Run
o get the project running locally, follow these steps. PostgreSQL is required, and you can either install it manually or run it via Docker:
```bash
# 1. Clone the repo
https://github.com/yourusername/dotnet-course-cv-backend.git

# 2. Run PostgreSQL with Docker (if not installed locally)
docker run --name aspnet_postgres -p 5432:5432 -e POSTGRES_PASSWORD=admin -d postgres

# 3. Make sure your appsettings.json connection string uses:
# Host=localhost;Port=5432;Database=table-reservations;Username=postgres;Password=admin

# 4. Apply DB migrations (EF CLI required)
dotnet tool install --global dotnet-ef --version 8.0.13
cd DNET.Backend.Api
 dotnet ef database update

# 5. Run the app
dotnet run

# 6. Run tests
cd ../DNET.Backend.Api.Tests
 dotnet test
```

---

## Deployment
- API deployed to Render.com
- Swagger UI available [publicly](https://dnet-backend-table-reservations.onrender.com/swagger/index.html)

---

## License
This project is open-source under the **MIT License**.
