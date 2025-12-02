# INTERVU - Guidelines & Conventions

Welcome to the Intervu project! This document provides general guidelines and conventions to ensure code quality and consistency throughout the development process.

## Quick Links (Tabs)

- [Commit Guidelines](#commit-guidelines-tab)
- [Entity Framework Codefirst Guideline](#quick-notes-tab)
- [Meeting Notes](#meeting-notes-tab)

---

## Solution Structure

- `Intervu.sln`: Root solution file aggregating all projects.
- `Intervu.API/`: Presentation layer (ASP.NET Core Web API)
  - Controllers, Hubs (SignalR), configuration (`appsettings*.json`), startup (`Program.cs`).
- `Intervu.Application/`: Application layer
  - Use cases, DTOs, interfaces, services, mappings, common models (e.g., `PagedResult`).
- `Intervu.Domain/`: Domain layer
  - Entities, domain abstractions (base entity), constants, repository interfaces.
- `Intervu.Infrastructure/`: Infrastructure layer
  - Persistence (EF Core DbContext + SQL Server repositories), external services (email, payments, code execution), DI extensions.

Cross-layer conventions

- Repositories return domain entities and primitives; DTOs are used in application/use-case layer only.
- Mapping between domain and DTOs is handled via AutoMapper profiles in `Intervu.Application`.
- EF Core migrations are under `Intervu.Infrastructure/Persistence/SqlServer/Migrations`.

```
intervu-be/
│
├── Intervu.API/
│   ├── Controllers/
│   │   ├── EmailDemoController.cs
│   │   ├── WeatherForecastController.cs
│   │   └── v1/
│   ├── Hubs/
│   │   └── InterviewRoomHub.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Utils/
│   │   ├── JsonElementComparer.cs
│   │   ├── LowercaseControllerRouteConvention.cs
│   │   └── Constant/
│   ├── Program.cs
│   ├── WeatherForecast.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── Intervu.Application/
│   ├── Interfaces/
│   │   ├── ExternalServices/
│   │   └── UseCases/
│   ├── UseCases/
│   │   ├── Authentication/
│   │   ├── InterviewBooking/
│   │   ├── ...
│   ├── DTOs/
│   │   ├── Availability/
│   │   ├── Company/
│   │   ├── ...
│   ├── Services/
│   │   ├── InterviewRoomCache.cs
│   │   ├── JwtService.cs
│   │   ├── PasswordHashHandler.cs
│   │   ├── RoomManagerService.cs
│   │   └── CodeGeneration/
│   ├── Common/
│   │   └── PagedResult.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   └── DependencyInjection.cs
│
├── Intervu.Domain/
│   ├── Abstractions/
│   │   └── Entity/
│   ├── Entities/
│   │   ├── Company.cs
│   │   ├── Feedback.cs
│   │   └── ...
│   └── Intervu.Domain.csproj
│
├── Intervu.Infrastructure/
│   ├── Persistence/
│   │   └── SqlServer/
│   ├── ExternalServices/
│   └── DependencyInjection.cs
│
└── Intervu.sln
```

## Commit Guidelines <!-- commit-guidelines-tab -->

### Commit Message Convention

To maintain a clean, readable, and easy-to-track project history, we will adhere to the **Conventional Commits** specification. This practice not only helps team members understand the nature of changes but also enables automated changelog generation and semantic versioning.

### Structure of a Commit Message

Each commit message consists of three main parts: a **Header**, an optional **Body**, and an optional **Footer**.

```
<type>(<scope>): <subject>
<BLANK LINE>
<body>
<BLANK LINE>
<footer>
```

---

### 1. Header (Required)

The header is the most important part and follows a strict format: `<type>(<scope>): <subject>`

#### **`<type>`**

This prefix describes the kind of change you have made. The types used in this project are:

| Type         | Description                                                                               |
| :----------- | :---------------------------------------------------------------------------------------- |
| **feat**     | A new feature for the user.                                                               |
| **fix**      | A bug fix in the codebase.                                                                |
| **docs**     | Changes that only affect documentation.                                                   |
| **style**    | Changes that do not affect the meaning of the code (white-space, formatting, etc.).       |
| **refactor** | A code change that neither fixes a bug nor adds a feature.                                |
| **perf**     | A code change that improves performance.                                                  |
| **test**     | Adding missing tests or correcting existing tests.                                        |
| **build**    | Changes that affect the build system or external dependencies (e.g., npm, nuget, gradle). |
| **cicd**     | Changes to our CI/CD configuration files and scripts.                                     |
| **chore**    | Other changes that don't modify source or test files (e.g., updating `.gitignore`).       |
| **revert**   | Reverts a previous commit.                                                                |

#### **`<scope>` (Optional)**

The `scope` provides additional contextual information about the commit. It could be the name of a module, component, or feature.

- `feat(api): add endpoint to get user profile`
- `fix(auth): correct password validation logic`
- `docs(readme): update setup instructions`

#### **`<subject>`**

- Use the imperative, present tense: "add", "change", "fix" not "added", "changed", or "fixed".
- Don't capitalize the first letter.
- Don't add a dot (.) at the end.
- Keep it short and concise, preferably under 50 characters.

---

### 2. Body (Optional)

- Use the body to provide more detailed explanations for the change.
- Separate the body from the header with a blank line.
- Explain the **"what"** and **"why"** of the change, not the "how".

---

### 3. Footer (Optional)

- Use the footer to reference issues from an issue tracker (e.g., Jira, GitHub Issues).
- Use it to indicate Breaking Changes.
- Separate the footer from the body with a blank line.

**Breaking Change:** Any commit that introduces a breaking change must start the footer with `BREAKING CHANGE:`, followed by a detailed description.

### Examples

**A simple commit (header only):**

```
fix(api): correct user data serialization
```

**A commit for a new feature:**

```
feat: allow users to upload profile pictures
```

**A commit with a detailed explanation (body):**

```
perf(db): improve query performance for fetching products

The previous query was using a LEFT JOIN which was inefficient
for large datasets. This commit changes it to use a subquery
which significantly reduces the query time.
```

**A commit with an issue reference and a Breaking Change:**

```
feat(auth): switch to JWT for authentication

This replaces the old session-based authentication system. All API
endpoints now require a Bearer token in the Authorization header.

Closes #123

BREAKING CHANGE: The authentication method has been changed.
Clients must now obtain a JWT and send it with every request.
```

## Entity Framework Codefirst Guideline <!-- quick-notes-tab -->

### Overview

This section provides guidelines for using **Entity Framework Core (Code-First)** in the Intervu project.  
The commands below are used to generate database migrations and apply schema changes consistently across environments.

---

### 🧱 1. Creating a Migration

To generate a new migration, use the following command:

dotnet ef migrations add <migration-name> -o Persistence/SqlServer/Migrations

#### Explanation

| Part                                | Description                                                                                                                                    |
| :---------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------- |
| dotnet ef migrations add            | Generates a new migration based on model changes.                                                                                              |
| <migration-name>                    | A short, descriptive name for the migration (e.g., AddUserTable, UpdateEventSchema).                                                           |
| -o Persistence/SqlServer/Migrations | Specifies the folder where migration files will be stored. In this project, all migrations are located under Persistence/SqlServer/Migrations. |

#### Example

dotnet ef migrations add InitDatabase -o Persistence/SqlServer/Migrations

This will create files under:

📂 Persistence  
　 📂 SqlServer  
　　 📂 Migrations  
　　　 ├── 20251025123045_InitDatabase.cs  
　　　 └── IntervuDbContextModelSnapshot.cs

---

### 🗄️ 2. Updating the Database

Once a migration has been created, apply it to the database using:

dotnet ef database update

#### Explanation

| Command                   | Purpose                                                                                           |
| :------------------------ | :------------------------------------------------------------------------------------------------ |
| dotnet ef database update | Applies all pending migrations to the database, ensuring the schema matches the current EF model. |

#### Example

dotnet ef database update

This command will:

- Create the database if it does not already exist.
- Apply all migrations found under Persistence/SqlServer/Migrations.
- Update the database schema to reflect your current entity models.

---

By following this convention, we can work more effectively and keep the project in a healthy state. Thank you for your cooperation!
