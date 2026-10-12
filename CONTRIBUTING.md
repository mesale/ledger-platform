# Contributing to Ledger Platform

Thank you for contributing to **Ledger Platform**! This repository is a double-entry financial ledger and payments platform built with ASP.NET Core (.NET 9), designed to practice real-world fintech engineering standards: strict domain invariants, clean architecture, idempotency, robust testing, and clean git history.

To keep code quality high and collaboration smooth, all contributors must follow the guidelines outlined below.

---

## Table of Contents

- [Code of Conduct & Collaboration](#code-of-conduct--collaboration)
- [Prerequisites & Development Setup](#prerequisites--development-setup)
- [Issue Workflow](#issue-workflow)
- [The Golden Rule: One PR = One Issue](#the-golden-rule-one-pr--one-issue)
- [PR Sizing & Scope](#pr-sizing--scope)
- [Branch Naming Conventions](#branch-naming-conventions)
- [Code Style & Standards (.editorconfig)](#code-style--standards-editorconfig)
- [Commit Message Conventions](#commit-message-conventions)
- [Submitting a Pull Request (PR)](#submitting-a-pull-request-pr)
- [Definition of Done (DoD)](#definition-of-done-dod)
- [Fintech & Architectural Guardrails](#fintech--architectural-guardrails)
- [Questions & Communication](#questions--communication)

---

## Code of Conduct & Collaboration

We are committed to providing a welcoming, constructive, and respectful environment for everyone.
- **Empathy & Respect**: Provide constructive and actionable feedback during code reviews.
- **Assume Good Faith**: Code reviews focus on code quality, architecture, performance, and correctness — never personal preferences.
- **Proactive Communication**: Discuss significant architectural decisions or database schema changes in an issue before writing code.

---

## Prerequisites & Development Setup

### 1. Prerequisites

Ensure you have the following installed on your machine:
- [.NET 9 SDK](https://dotnet.microsoft.com/download) (`9.0.3xx` or newer)
- [Docker](https://www.docker.com/) & Docker Compose
- [Git](https://git-scm.com/)
- [Node.js](https://nodejs.org/) & npm (v20+, required for the upcoming React/TypeScript frontend)
- Recommended IDE: Visual Studio 2022 (v17.12+), JetBrains Rider, or VS Code with C# Dev Kit

### 2. Setup Steps

Follow these steps to set up and run the platform locally:

```bash
# 1. Clone the repository
git clone https://github.com/mesale/ledger-platform.git
cd ledger-platform

# 2. Copy the environment variables template
cp .env.example .env

# 3. Start local dependencies (PostgreSQL, Redis, RabbitMQ)
docker compose up -d

# 4. Confirm containers are healthy
docker compose ps

# 5. Restore NuGet dependencies
dotnet restore Ledger.sln

# 6. Build the solution
dotnet build Ledger.sln

# 7. Run all tests
dotnet test Ledger.sln
```

### 3. Local Infrastructure Services

Once Docker Compose is running:
- **PostgreSQL**: `localhost:5432` (database credentials defined in `.env`)
- **Redis**: `localhost:6379` (caching and distributed idempotency locking)
- **RabbitMQ AMQP**: `localhost:5672` (messaging and saga state machine)
- **RabbitMQ Management UI**: `http://localhost:15672` (default: `guest` / `guest`)

### 4. Running the Applications

- **Ledger API** (Main backend):
  ```bash
  dotnet run --project src/Ledger.Api
  ```
- **Ledger MockRail** (External bank/card network simulator):
  ```bash
  dotnet run --project src/Ledger.MockRail
  ```

---

## Issue Workflow

Every code contribution must originate from a tracked GitHub issue.

### 1. Search First
Before opening a new issue, search existing open and closed issues to ensure the topic or bug has not already been reported or resolved.

### 2. Creating an Issue
Choose the appropriate issue type and provide complete context:

- **Bug Reports**:
  - Clear summary and business impact.
  - Precise, reproducible steps.
  - Expected behavior vs. actual behavior.
  - Relevant logs, stack traces, request/response JSON payloads.
  - Environment details (.NET version, OS, container status).
- **Feature Requests & Tasks**:
  - Motivation and problem statement.
  - Proposed implementation or architecture.
  - Clear, testable acceptance criteria (checkboxes).

### 3. Claiming / Assigning an Issue
- Always assign the issue to yourself or post a comment indicating you are taking it before starting work.
- Do not start work on an issue assigned to someone else without coordinating first.

---

## The Golden Rule: One PR = One Issue

> **Strict Rule**: Every Pull Request must focus on **exactly one issue / single concern**.

- **No Multi-Issue Bundling**: Never bundle multiple distinct bug fixes, features, or tasks into a single PR.
- **No Drive-by Refactorings or Formatting**: Do not reformat unrelated files or refactor adjacent code while working on an issue. If you notice unrelated technical debt or styling inconsistencies, open a separate issue.
- **Why this rule is strictly enforced**:
  - **Faster Reviews**: Reviewers can review and approve small, focused changes rapidly.
  - **Isolated Rollbacks**: If a bug or regression is introduced, the PR can be reverted safely without rolling back unrelated features.
  - **Clean Git History**: Keeps commit history and `git bisect` clean and traceable.

---

## PR Sizing & Scope

To maintain high code review standards and rapid feedback loops, adhere to these PR size guidelines:

### 1. Size Guidelines
- **Ideal Size**: **Under 200–400 lines of diff** (excluding auto-generated files, package lockfiles, or EF Core migrations).
- **Maximum Recommended**: 500 lines of diff. PRs exceeding this threshold will usually be requested to be split.

### 2. Slicing Large Features
If a feature requires substantial work, decompose it into smaller, stacked PRs linked to sub-tasks:
1. **PR 1 (Domain)**: Invariants, entities, value objects, domain events, and unit tests.
2. **PR 2 (Application)**: MediatR commands/queries, handlers, and FluentValidation validators.
3. **PR 3 (Infrastructure)**: EF Core configurations, migrations, external client adapters, and repository implementations.
4. **PR 4 (Api)**: Endpoints, request/response models, middleware, and integration tests.

---

## Branch Naming Conventions

All branches must follow a structured, predictable naming scheme.

### 1. Format

```text
<type>/<issue-number>-<short-description>
```

*(If working on a task without an issue number, use `<type>/<short-description>`)*

### 2. Allowed Branch Types

| Prefix | Description | Example |
| :--- | :--- | :--- |
| `feat/` | New feature or capability | `feat/#12-account-invariants` |
| `fix/` | Bug fix | `fix/#45-idempotency-key-race` |
| `refactor/` | Code refactoring (no behavior change) | `refactor/#32-mediatr-pipeline` |
| `test/` | Adding or updating tests | `test/#19-transfer-saga-tests` |
| `docs/` | Documentation changes only | `docs/#8-update-contributing-guide` |
| `chore/` | Tooling, dependencies, or CI updates | `chore/#14-upgrade-testcontainers` |

### 3. Branching Rules
- Use lowercase alphanumeric characters and hyphens (`kebab-case`).
- Do not use underscores (`_`), uppercase letters, or spaces.
- Always branch off the latest `main` branch:
  ```bash
  git checkout main
  git pull origin main
  git checkout -b feat/#12-account-invariants
  ```

---

## Code Style & Standards (.editorconfig)

This project strictly enforces styling and quality rules defined in [`.editorconfig`](./.editorconfig). CI enforces these rules on every pull request.

### 1. General Formatting Defaults
- **Encoding**: UTF-8 (`charset = utf-8`)
- **Line Endings**: LF (`end_of_line = lf`)
- **Indentation**:
  - `4 spaces`: C# (`*.cs`), Dockerfile (`Dockerfile`)
  - `2 spaces`: JSON (`*.json`), YAML (`*.yml`, `*.yaml`), C# project files (`*.csproj`), Markdown (`*.md`), Frontend (`*.ts`, `*.tsx`, `*.js`, `*.jsx`, `*.css`, `*.scss`, `*.html`), Shell scripts (`*.sh`)
- **Trailing Whitespace**: Automatically trimmed (`trim_trailing_whitespace = true`), except in Markdown (`trim_trailing_whitespace = false`).
- **Final Newline**: Always insert a trailing newline (`insert_final_newline = true`).

### 2. C# Code Conventions (.NET 9)

- **File-Scoped Namespaces**: Enforced as compiler error.
  ```csharp
  // Correct
  namespace Ledger.Domain.Accounts;

  // Prohibited
  namespace Ledger.Domain.Accounts
  {
  }
  ```
- **Using Directives**:
  - Placed outside the namespace declaration (enforced as error).
  - System directives sorted first (`dotnet_sort_system_directives_first = true`).
- **Allman Braces**:
  - Braces must always appear on their own newline for classes, methods, `if/else`, `try/catch/finally`.
  - Braces are required for all control blocks, including single-line statements (`csharp_prefer_braces = true:error`).
- **Accessibility Modifiers**:
  - Always explicitly declare accessibility modifiers (`dotnet_style_require_accessibility_modifiers = always:error`).
  - Preferred modifier order: `public`, `private`, `protected`, `internal`, `static`, etc.
- **Modern C# Idioms**:
  - Prefer pattern matching and switch expressions over nested `if`/`switch` blocks.
  - Prefer null-coalescing expressions (`??`) and null propagation (`?.`).
  - Expression-bodied members preferred for single-line methods and properties.
- **`var` Usage**:
  - Use `var` when the type is apparent (e.g. `var account = new Account();`).
  - Use explicit types when the type is not apparent (e.g. `Account account = GetAccount();`).

### 3. C# Naming Rules (Enforced as Errors)

| Target | Convention | Rule & Example |
| :--- | :--- | :--- |
| **Interfaces** | `I` + PascalCase | `IAccountRepository`, `ITransferService` |
| **Types** (classes, structs, records, enums) | PascalCase | `Account`, `TransactionEntry`, `TransferStatus` |
| **Members** (methods, properties, events) | PascalCase | `PostTransactionAsync`, `CreatedAt`, `Balance` |
| **Private Fields** | `_` + camelCase | `_ledgerDbContext`, `_mediator`, `_logger` |
| **Constants** | PascalCase | `DefaultPageSize`, `MaxRetryAttempts` |
| **Parameters & Locals** | camelCase | `accountId`, `transferAmount`, `idempotencyKey` |

### 4. Code Formatting & Verification Commands

Verify and auto-fix formatting before committing:

```bash
# Verify formatting matches .editorconfig (fails if formatting issues exist)
dotnet format Ledger.sln --verify-no-changes

# Automatically fix formatting issues across the entire solution
dotnet format Ledger.sln
```

---

## Commit Message Conventions

We follow the **Conventional Commits** specification (`<type>(<scope>): <description>`).

### 1. Commit Structure

```text
<type>(<scope>): <short description in imperative mood>

[optional detailed body explaining context and rationale]

[optional footer(s), e.g., Closes #123]
```

### 2. Common Commit Types
- `feat`: A new feature or capability.
- `fix`: A bug fix.
- `refactor`: Code restructuring without modifying behavior.
- `test`: Adding or modifying tests.
- `docs`: Documentation updates.
- `chore`: Tooling, build system, or dependency updates.
- `style`: Formatting changes that do not affect code logic.

### 3. Examples
- `feat(domain): enforce debits equal credits invariant on transactions`
- `fix(api): reject transfer requests with duplicate idempotency key`
- `test(infrastructure): add testcontainers integration tests for postgres`
- `chore(ci): add dotnet format verification step`

---

## Submitting a Pull Request (PR)

### 1. Pre-Push Verification Checklist
Before pushing your branch, execute the following locally:
1. Rebase on latest `main`:
   ```bash
   git fetch origin
   git rebase origin/main
   ```
2. Build solution cleanly in Release mode:
   ```bash
   dotnet build Ledger.sln --configuration Release
   ```
3. Run all tests and ensure they pass:
   ```bash
   dotnet test Ledger.sln --configuration Release
   ```
4. Verify code format against `.editorconfig`:
   ```bash
   dotnet format Ledger.sln --verify-no-changes
   ```

### 2. Opening the PR
- **Base branch**: `main`.
- **Title**: Descriptive conventional commit style (e.g., `feat(application): add open account command handler`).
- **Link the Issue**: Use GitHub linking keywords in the description: `Closes #123`, `Fixes #123`, or `Resolves #123`.

### 3. PR Description Template

Use this format for your PR description:

```markdown
## Summary
Brief summary of what changes are introduced and why.

## Linked Issue
Closes #<issue_number>

## Changes Made
- Point 1
- Point 2

## How Was This Tested?
- [ ] Unit tests added / updated
- [ ] Integration tests added / updated
- [ ] Tested locally with Docker Compose services

## Checklist
- [ ] PR addresses only one issue / concern
- [ ] Diff is appropriately sized (< 400 lines)
- [ ] Follows .editorconfig guidelines
- [ ] `dotnet test` passes locally
- [ ] `dotnet format --verify-no-changes` passes locally
```

### 4. Code Review & Merging
- Every PR requires **at least one review approval** before merging.
- All CI checks (build, test, format) must be green.
- Address all comments and feedback promptly.
- PRs are merged into `main` using **Squash and Merge** (or Rebase) to maintain a clean, linear git history.

---

## Definition of Done (DoD)

A contribution is considered complete and eligible for merge only when:
- [ ] Acceptance criteria from the linked issue are completely met.
- [ ] Domain invariants are preserved (e.g., debits equal credits, positive balances).
- [ ] Comprehensive unit tests cover new logic and boundary conditions.
- [ ] Integration tests verify database interactions and messaging where applicable.
- [ ] Clean Architecture layer boundaries are respected (dependencies point inward).
- [ ] `.editorconfig` rules and naming conventions are strictly met.
- [ ] All automated CI checks pass.
- [ ] Documentation (README or code comments) is updated if APIs or configuration changed.

---

## Fintech & Architectural Guardrails

As a financial ledger and payments system, the following engineering principles must be adhered to at all times:

1. **Double-Entry Ledger Invariant**:
   - Every transaction entry must have equal total debits and credits.
   - Balance changes must never occur through ad-hoc single-column updates.
2. **Monetary Precision**:
   - **Never** use floating-point types (`float`, `double`) for currency, balances, or amounts. Always use `decimal` or strongly-typed money value objects.
3. **Idempotency**:
   - All write operations that create or alter balances/transfers must require an idempotency key.
   - Retried requests with the same key must return the original result without re-executing balance changes.
4. **Clean Architecture Boundaries**:
   - `Ledger.Domain`: Core entities, invariants, value objects. Zero external package or database dependencies.
   - `Ledger.Application`: Use cases, CQRS commands/queries via MediatR, validators. Depends only on `Domain`.
   - `Ledger.Infrastructure`: Database (EF Core), message bus (RabbitMQ), external service clients. Depends on `Application` and `Domain`.
   - `Ledger.Api`: Controllers/endpoints, Swagger, middleware. Entry point / composition root.
5. **Database Migrations**:
   - Schema modifications must always be managed through EF Core migrations, never direct database alterations.

---

## Questions & Communication

- For design discussions, open an issue labeled `discussion` or `question`.
- For architectural reference and phase roadmaps, refer to the project [design document](./docs/Ledger-Platform-Design-Document.docx).
