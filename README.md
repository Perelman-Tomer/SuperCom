# Task Manager

A full-stack task management application built with **.NET 9**, **React 19**, **SQL Server**, and **RabbitMQ**.

---

## Architecture Overview

```
Task-Manager/
├── backend/
│   └── src/
│       ├── SuperCom.Core/            # Entities, DTOs, Interfaces, Enums, Validators
│       ├── SuperCom.Infrastructure/  # DbContext, EF Migrations, Service implementations
│       ├── SuperCom.API/             # ASP.NET Core Web API (Controllers, Middleware)
│       └── SuperCom.ReminderService/ # Windows Service – due-date checker + RabbitMQ consumer
│   └── tests/
│       └── SuperCom.Tests/           # Unit tests, Validator tests, Integration tests
├── frontend/                         # React + Vite + Redux Toolkit + MUI
└── docker-compose.yml                # SQL Server 2022 + RabbitMQ 4
```

### Backend

| Layer | Responsibility |
|---|---|
| **SuperCom.Core** | Domain entities (`TaskItem`, `Tag`, `TaskTag`), DTOs, interfaces (`ITaskService`, `ITagService`), FluentValidation validators, `Priority` enum |
| **SuperCom.Infrastructure** | `SuperComDbContext` (EF Core 9 + SQL Server), service implementations (`TaskService`, `TagService`), seed data, migrations |
| **SuperCom.API** | RESTful controllers (`/api/tasks`, `/api/tags`), global exception-handler middleware, CORS, JSON enum serialization, auto-migration on startup |
| **SuperCom.ReminderService** | `DueDateCheckerService` — background producer that finds overdue tasks and publishes to RabbitMQ. `ReminderConsumerService` — background consumer that logs reminders. Runs as a Windows Service. |

> **Note:** The internal .NET namespaces use the prefix `SuperCom.*` — this is the project's code namespace and does not affect the application name or branding.

### Frontend

| Module | Responsibility |
|---|---|
| **Redux Toolkit** | `taskSlice` and `tagSlice` with async thunks for CRUD |
| **MUI v7** | Material UI components, theme, date pickers, data tables |
| **react-hook-form + yup** | Form validation |
| **axios** | HTTP client (`http://localhost:5055/api`) |

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

## Getting Started

### 1. Start Infrastructure (SQL Server & RabbitMQ)

```bash
docker compose up -d
```

This starts:
- **SQL Server 2022** on port `1433` (SA password: `SuperCom@2026!`)
- **RabbitMQ 4** on ports `5672` (AMQP) and `15672` (Management UI — guest/guest)

### 2. Run the Backend API

```bash
cd backend/src/SuperCom.API
dotnet run
```

The API starts on **http://localhost:5055**. On first run it auto-migrates and seeds 8 default tags.

### 3. Run the Frontend

```bash
cd frontend
npm install
npm run dev
```

Opens on **http://localhost:5173**.

### 4. (Optional) Run the Reminder Windows Service

```bash
cd backend/src/SuperCom.ReminderService
dotnet run
```

Checks for overdue tasks every 60 seconds and publishes reminders to the `task-reminders` RabbitMQ queue.

---

## Running Tests

### Backend (xUnit)

```bash
cd backend
dotnet test --verbosity normal
```

Tests include:
- **Unit tests** — `TaskServiceTests`, `TagServiceTests` (EF Core InMemory)
- **Validator tests** — `CreateTaskItemValidator`, `CreateTagValidator` (FluentValidation)
- **Integration tests** — Full HTTP pipeline via `WebApplicationFactory` (InMemory DB)

### Frontend (Vitest)

```bash
cd frontend
npm test
```

---

## API Endpoints

### Tasks (`/api/tasks`)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/tasks` | Get all tasks (ordered by newest first) |
| `GET` | `/api/tasks/{id}` | Get a single task by ID |
| `POST` | `/api/tasks` | Create a new task |
| `PUT` | `/api/tasks/{id}` | Update an existing task |
| `DELETE` | `/api/tasks/{id}` | Delete a task |

### Tags (`/api/tags`)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/tags` | Get all tags (ordered alphabetically) |
| `GET` | `/api/tags/{id}` | Get a single tag by ID |
| `POST` | `/api/tags` | Create a new tag (unique name enforced) |
| `PUT` | `/api/tags/{id}` | Update an existing tag |
| `DELETE` | `/api/tags/{id}` | Delete a tag |

---

## SQL Query — Tasks with 2 or More Tags

The following query retrieves all tasks that have **at least 2 tags**, along with the tag count and a comma-separated list of tag names:

```sql
SELECT
    t.Id,
    t.Title,
    COUNT(tt.TagId)              AS TagCount,
    STRING_AGG(tg.Name, ', ')    AS TagNames
FROM Tasks t
    INNER JOIN TaskTags tt ON t.Id = tt.TaskItemId
    INNER JOIN Tags tg     ON tt.TagId = tg.Id
GROUP BY t.Id, t.Title
HAVING COUNT(tt.TagId) >= 2
ORDER BY TagCount DESC;
```

---

## Tech Stack

| Area | Technology |
|---|---|
| Backend Framework | ASP.NET Core 9 (Web API) |
| ORM | Entity Framework Core 9 (SQL Server) |
| Database | SQL Server 2022 (Docker) |
| Validation | FluentValidation 12 |
| Messaging | RabbitMQ 4 (via RabbitMQ.Client 7) |
| Frontend Framework | React 19 + Vite 7 |
| State Management | Redux Toolkit 2 |
| UI Library | MUI v7 (Material UI) |
| Forms | react-hook-form + yup |
| Testing (BE) | xUnit, Moq, EF Core InMemory |
| Testing (FE) | Vitest, React Testing Library |
