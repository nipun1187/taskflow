# TaskFlow — Task Management System

A production-ready ASP.NET Core Web API for personal and team task management. Built as a college project to demonstrate practical .NET development, clean architecture, EF Core data access, JWT authentication, validation, logging, and unit testing.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/License-MIT-green)
![Tests](https://img.shields.io/badge/tests-passing-brightgreen)

## Live Demo

Clone and run locally — Swagger UI opens at `https://localhost:5001/swagger`:

```bash
git clone https://github.com/nipun1187/taskflow.git
cd taskflow
dotnet run --project src/TaskFlow.Api
```

- **Postman collection**: [`docs/TaskFlow.postman_collection.json`](docs/TaskFlow.postman_collection.json)
- A free public Azure / Render deployment is planned — see [Deployment](#deployment).

## Tech Stack

| Layer            | Technology                                      |
|------------------|-------------------------------------------------|
| Framework        | ASP.NET Core 8.0 Web API                        |
| Language         | C# 12                                           |
| ORM              | Entity Framework Core 8                         |
| Database         | SQL Server (LocalDB / Azure SQL) — SQLite for dev |
| Auth             | JWT Bearer Tokens + ASP.NET Core Identity       |
| Validation       | FluentValidation                                |
| Logging          | Serilog (file + console sinks)                  |
| API Docs         | Swagger / OpenAPI (Swashbuckle)                 |
| Testing          | xUnit, Moq, FluentAssertions                    |
| CI               | GitHub Actions                                  |
| Hosting          | Azure App Service                               |

## Architecture

```
TaskFlow.Api
 ├── Controllers/      HTTP endpoints (thin)
 ├── Services/         Business logic (testable)
 ├── Data/             EF Core DbContext + migrations
 ├── Models/           Domain entities
 ├── Dtos/             Request/response contracts
 └── Middleware/       Cross-cutting (error handling)
TaskFlow.Tests        xUnit + Moq unit tests
```

A standard **N-tier architecture**: Controllers → Services → Repository (EF Core DbContext).
DTOs isolate the API surface from the domain model.

## Features

- User registration & login with JWT
- CRUD on tasks (create, read, update, delete)
- Filtering by status, priority, due date
- Pagination
- Per-user authorization (a user only sees their tasks)
- Validation with friendly error messages
- Centralized error handling middleware
- Structured logging (Serilog)
- 30+ unit tests (`dotnet test`)

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Git

### Run locally

```bash
git clone https://github.com/nipun1187/taskflow.git
cd taskflow
dotnet restore
dotnet ef database update --project src/TaskFlow.Api
dotnet run --project src/TaskFlow.Api
```

Open https://localhost:5001/swagger in your browser.

### Run tests

```bash
dotnet test
```

## API Reference (selected)

| Method | Endpoint                  | Description                |
|--------|---------------------------|----------------------------|
| POST   | `/api/auth/register`      | Register a new user        |
| POST   | `/api/auth/login`         | Get JWT access token       |
| GET    | `/api/tasks`              | List current user's tasks  |
| POST   | `/api/tasks`              | Create a task              |
| GET    | `/api/tasks/{id}`         | Get task by id             |
| PUT    | `/api/tasks/{id}`         | Update a task              |
| DELETE | `/api/tasks/{id}`         | Delete a task              |

Full reference: open Swagger UI after starting the app.

## Deployment

Tested on Azure App Service (Linux, .NET 8 runtime).

```bash
az webapp up --name taskflow-demo --runtime "DOTNETCORE:8.0" \
  --sku F1 --location eastus
```

The included [`.github/workflows/ci.yml`](.github/workflows/ci.yml) builds, tests, and
publishes the artifact on every push to `main`.

## Folder Layout

```
.
├── src/
│   └── TaskFlow.Api/        ASP.NET Core Web API project
├── tests/
│   └── TaskFlow.Tests/      xUnit test project
├── docs/                    Diagrams, ER diagram, screenshots
├── .github/workflows/       CI pipeline
└── README.md
```

## License

MIT © 2026 Raghav Aggarwal — submitted as part of the BCA Final Year Project.
