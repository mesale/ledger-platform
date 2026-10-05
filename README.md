# Ledger Platform

[![CI](https://github.com/mesale/ledger-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/mesale/ledger-platform/actions/workflows/ci.yml)
![Branch](https://img.shields.io/badge/branch-main-blue)
![Projects](https://img.shields.io/badge/projects-5-8A2BE2)
![.NET](https://img.shields.io/badge/.NET-9-512BD4)

A double-entry financial ledger and payments backend built with ASP.NET Core, designed to mirror how a real fintech team builds and ships this kind of system: strict correctness guarantees, idempotent and resilient payment flows, and a full CI/CD and observability pipeline behind it.

This is a learning/portfolio project built collaboratively to practice professional full-stack and DevOps workflows, not a production payments system — no real money or real external rails are involved.

## What it does

- Maintains accounts and a double-entry ledger where every transaction's debits always equal its credits, enforced as a domain invariant
- Processes transfers idempotently, so a retried or duplicated request never double-processes
- Handles multi-step transfers to a simulated external bank/card rail as a saga, with compensation on failure and reconciliation on ambiguous timeouts
- Reconciles its own ledger on a schedule, catching drift before it becomes a real problem
- Applies simple fraud/risk rules (velocity limits, large-amount holds) before a transfer posts
- Ships with full observability, infrastructure-as-code, and an automated deploy pipeline

## Architecture

The system is a **modular monolith**, not microservices — the ledger, saga orchestration, reconciliation, and fraud rules all live in one deployable application sharing one database, so balance-affecting operations get real ACID guarantees instead of distributed-transaction complexity. The one deliberate exception is the **mocked external rail**, a genuinely separate service used to practice real cross-service failure handling (timeouts, retries, sagas) without needing an actual bank integration.

Internally, the backend follows **Clean Architecture** (`Domain` → `Application` → `Infrastructure` → `Api`, dependencies pointing inward only) with **CQRS via MediatR** separating writes from reads.

Full design detail — database schema, entity relationships, the saga state machine, architecture decision records, and the stage-by-stage task board — lives in the project's [design document](https://claude.ai/code/artifact/4295a9d7-8789-45cf-8566-47013fc76c15).

## Tech stack

| Layer | Technology |
| --- | --- |
| Backend | ASP.NET Core (.NET 9), EF Core, MediatR, FluentValidation |
| Database | PostgreSQL |
| Caching / idempotency | Redis |
| Messaging | RabbitMQ via MassTransit (outbox relay, saga state machine) |
| Auth | ASP.NET Core Identity / JWT + refresh tokens |
| Frontend | React, TypeScript |
| Infra | Docker, Terraform, GitHub Actions |
| Observability | Serilog, OpenTelemetry, Prometheus, Grafana |
| Testing | xUnit, Testcontainers, k6/NBomber |

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) and Docker Compose
- Node.js (for the frontend, once it exists)

### Setup

```bash
git clone https://github.com/<org-or-user>/ledger-platform.git
cd ledger-platform

# copy the env template and fill in local values
cp .env.example .env

# start Postgres, Redis, and RabbitMQ
docker compose up -d

# confirm everything is healthy
docker compose ps
```

RabbitMQ's management UI is available at `http://localhost:15672` once it's running.

### Build and test

```bash
dotnet restore Ledger.sln
dotnet build Ledger.sln
dotnet test Ledger.sln
```

## Project structure

```
src/
├── Ledger.Domain/          # entities, invariants — no external dependencies
├── Ledger.Application/     # use cases, organized by feature (CQRS via MediatR)
├── Ledger.Infrastructure/  # EF Core, messaging, external clients
├── Ledger.Api/              # HTTP layer, composition root
└── Ledger.MockRail/        # standalone service simulating a bank/card network
tests/                      # mirrors src/, one test project per project
frontend/                   # React + TypeScript app
infra/                      # Terraform, deployment manifests
docs/                       # architecture notes, runbooks
```

## Contributing (just the two of us, for now)

- All changes go through a pull request into `main` — no direct pushes
- At least one review required before merging
- CI (build + test) must pass before a PR can merge
- See the design doc's task board for the current stage and open work

## License

[MIT](./LICENSE) © 2026 Mesalem Tsegaye and Yared Mengiste
