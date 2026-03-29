# HelpDesk Service API

A production-ready RESTful Web API for managing helpdesk support tickets, built with **.NET 10** using a clean **3-layered monolith architecture**.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Swagger UI](#swagger-ui)
- [API Versioning](#api-versioning)
- [CORS](#cors)
- [Running Tests](#running-tests)
- [Data Models](#data-models)

---

## Architecture Overview

The solution follows a **3-layered architecture**:

```
┌────────────────────────────────────┐
│         Presentation Layer         │  HelpdeskService.API
│  Controllers, Swagger, Middleware  │
└────────────────┬───────────────────┘
                 │
┌────────────────▼───────────────────┐
│          Business Layer            │  HelpdeskService.Core
│  Entities, DTOs, Interfaces,       │
│  Settings                          │
└────────────────┬───────────────────┘
                 │
┌────────────────▼───────────────────┐
│          Data Access Layer         │  HelpdeskService.Data
│  EF Core DbContext, Repositories,  │
│  Service Implementations           │
└────────────────────────────────────┘
```

Additionally:

- **`HelpdeskService.Tests`** — xUnit test project covering unit and integration tests.

---

## Tech Stack

| Technology                       | Purpose                     |
| -------------------------------- | --------------------------- |
| .NET 10                          | Framework                   |
| ASP.NET Core Web API             | HTTP server                 |
| Entity Framework Core 10         | ORM                         |
| EF Core InMemory                 | Database provider (default) |
| JWT Bearer Authentication        | Authorization               |
| Swashbuckle / Swagger            | API documentation           |
| Asp.Versioning.Mvc               | API versioning              |
| xUnit                            | Unit & Integration testing  |
| Moq                              | Mocking in tests            |
| Microsoft.AspNetCore.Mvc.Testing | Integration test host       |

---

## Project Structure

```
HelpdeskService.sln
│
├── HelpdeskService.API/              # Web API presentation layer
│   ├── Controllers/
│   │   ├── HomeController.cs         # Health check and welcome
│   │   ├── AuthController.cs         # Register & Login endpoints
│   │   └── TicketsController.cs      # Full CRUD for tickets
│   ├── Swagger/
│   │   └── ConfigureSwaggerOptions.cs
│   ├── Program.cs                    # App bootstrap
│   └── appsettings.json
│
├── HelpdeskService.Core/             # Business / domain layer
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Ticket.cs                 # TicketStatus + TicketPriority enums
│   │   └── Comment.cs
│   ├── DTOs/
│   │   ├── AuthDtos.cs               # RegisterDto, LoginDto, AuthResponseDto
│   │   └── TicketDtos.cs             # CreateTicketDto, UpdateTicketDto, TicketDto, CommentDto
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   └── ITicketsService.cs
│   └── Settings/
│       └── JwtSettings.cs
│
├── HelpdeskService.Data/             # Data access layer
│   ├── Context/
│   │   └── HelpdeskDbContext.cs      # EF Core DbContext with model configuration
│   ├── Services/
│   │   ├── AuthService.cs            # Implements IAuthService, JWT generation, password hashing
│   │   └── TicketsService.cs         # Implements ITicketsService, full CRUD
│   └── Extensions/
│       └── DataServiceExtensions.cs  # DI registration helper
│
└── HelpdeskService.Tests/            # xUnit test project
    ├── Services/
    │   ├── AuthServiceTests.cs        # Unit tests for AuthService
    │   └── TicketsServiceTests.cs     # Unit tests for TicketsService
    └── Controllers/
        ├── HomeControllerTests.cs     # Integration tests for HomeController
        ├── AuthControllerTests.cs     # Integration tests for AuthController
        └── TicketsControllerTests.cs  # Integration tests for TicketsController
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run the API

```bash
# Clone the repository
git clone https://github.com/ArtemNorenko1982/helpdesk-service-api.git
cd helpdesk-service-api

# Restore packages
dotnet restore

# Run the API
dotnet run --project HelpdeskService.API

# The API will be available at: http://localhost:5000
# Swagger UI opens at: http://localhost:5000 (root)
```

---

## Configuration

Edit `HelpdeskService.API/appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKey_AtLeast32Characters!",
    "Issuer": "HelpdeskService",
    "Audience": "HelpdeskServiceUsers",
    "ExpirationInMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:4200"]
  }
}
```

> ⚠️ **Important**: Change the `SecretKey` before deploying to production.

---

## API Endpoints

All endpoints are versioned under `/api/v1/`.

### HomeController

| Method | Endpoint              | Auth | Description     |
| ------ | --------------------- | ---- | --------------- |
| GET    | `/api/v1/home`        | ❌   | Welcome message |
| GET    | `/api/v1/home/health` | ❌   | Health check    |

### AuthController

| Method | Endpoint                | Auth | Description             |
| ------ | ----------------------- | ---- | ----------------------- |
| POST   | `/api/v1/auth/register` | ❌   | Register a new user     |
| POST   | `/api/v1/auth/login`    | ❌   | Login and get JWT token |

### TicketsController

| Method | Endpoint                        | Auth     | Description                |
| ------ | ------------------------------- | -------- | -------------------------- |
| GET    | `/api/v1/tickets`               | ✅ Admin | Get all tickets            |
| GET    | `/api/v1/tickets/my`            | ✅ User  | Get current user's tickets |
| GET    | `/api/v1/tickets/{id}`          | ✅ User  | Get ticket by ID           |
| POST   | `/api/v1/tickets`               | ✅ User  | Create a new ticket        |
| PUT    | `/api/v1/tickets/{id}`          | ✅ Owner | Update a ticket            |
| DELETE | `/api/v1/tickets/{id}`          | ✅ Owner | Delete a ticket            |
| POST   | `/api/v1/tickets/{id}/comments` | ✅ User  | Add a comment              |

---

## Authentication

The API uses **JWT Bearer tokens** for authentication.

### Register a user

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

### Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "username": "johndoe",
  "email": "john@example.com",
  "role": "User",
  "expiresAt": "2025-01-01T12:00:00Z"
}
```

### Use the token

Include the token in subsequent requests:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## Swagger UI

When running in Development mode, Swagger UI is available at the root URL:

```
http://localhost:5000
```

The Swagger UI includes:

- Full API documentation for all versions
- JWT Bearer authentication support (click **Authorize** and enter `Bearer {your_token}`)
- Interactive request testing

---

## API Versioning

The API uses URL-segment versioning via `Asp.Versioning.Mvc`:

- Default version: `v1`
- URL pattern: `/api/v{version}/{controller}`
- Current version: `v1`

The `api-supported-versions` header is included in all responses.

---

## CORS

Two CORS policies are configured:

| Policy                   | Description                                                   |
| ------------------------ | ------------------------------------------------------------- |
| `AllowConfiguredOrigins` | (Default) Allows origins from `Cors:AllowedOrigins` in config |
| `AllowAll`               | Open policy — useful for development                          |

Configure allowed origins in `appsettings.json` under `Cors:AllowedOrigins`.

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=normal"

# Run only unit tests
dotnet test --filter "FullyQualifiedName~Services"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Controllers"
```

The test suite includes:

- **Unit Tests** (Services layer):
  - `AuthServiceTests` — register, login, duplicate email, wrong password
  - `TicketsServiceTests` — CRUD operations, ownership validation, comments

- **Integration Tests** (Controllers layer):
  - `HomeControllerTests` — health and welcome endpoints
  - `AuthControllerTests` — registration and login flows
  - `TicketsControllerTests` — full ticket lifecycle with JWT auth

---

## Data Models

### User

| Field        | Type     | Description            |
| ------------ | -------- | ---------------------- |
| Id           | int      | Primary key            |
| Username     | string   | Unique username        |
| Email        | string   | Unique email address   |
| PasswordHash | string   | PBKDF2-hashed password |
| Role         | string   | `User` or `Admin`      |
| CreatedAt    | DateTime | Account creation time  |

### Ticket

| Field       | Type      | Description                                |
| ----------- | --------- | ------------------------------------------ |
| Id          | int       | Primary key                                |
| Title       | string    | Ticket title (max 200)                     |
| Description | string    | Ticket description (max 2000)              |
| Status      | enum      | `Open`, `InProgress`, `Resolved`, `Closed` |
| Priority    | enum      | `Low`, `Medium`, `High`, `Critical`        |
| UserId      | int       | Owner's user ID                            |
| CreatedAt   | DateTime  | Creation timestamp                         |
| UpdatedAt   | DateTime? | Last update timestamp                      |

### Comment

| Field     | Type     | Description             |
| --------- | -------- | ----------------------- |
| Id        | int      | Primary key             |
| Content   | string   | Comment text (max 1000) |
| TicketId  | int      | Associated ticket       |
| UserId    | int      | Author's user ID        |
| CreatedAt | DateTime | Creation timestamp      |

---

## Security

- Passwords are hashed using **PBKDF2** with SHA-256, 100,000 iterations, and a random 16-byte salt
- JWT tokens are signed with **HMAC-SHA256**
- Tokens expire after a configurable duration (default: 60 minutes)
- Owner checks enforce that users can only modify their own tickets
