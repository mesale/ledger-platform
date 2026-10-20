# Stage 1 Plan: Repo, CI, Domain Model, Ledger Core, Basic Transfer

**Goal:** a trusted double-entry ledger, exposed over a secured HTTP API and backed by real PostgreSQL, where a user can create accounts and move money between them. Every balance-altering write requires an idempotency key, as CONTRIBUTING.md demands.

**Covers:** Phase 0 (close-out), Phase 1 (core ledger domain), Phase 2 (API and persistence), a minimal slice of auth pulled forward from Stage 4, and a lightweight form of idempotency pulled forward from Stage 2.

**Idempotency in Stage 1 (lightweight):** `PostTransfer` and the dev funding path require an `Idempotency-Key`. It is stored on `transactions.idempotency_key` with a unique index. A repeated key returns the original result and creates no second transaction, including when two requests race.

**Out of scope (later stages):** the idempotency middleware, Redis lookup, request-hash check and stored response replay, plus the outbox (Stage 2); sagas and the mock rail (Stages 2-3); refresh tokens and the admin role (Stage 4).

## Exit criteria (the Stage 1 demo)

- [ ] Register and log in, receive a JWT
- [ ] Create two accounts, fund one, transfer between them, and see correct balances
- [ ] Replaying the same transfer with the same `Idempotency-Key` creates one transfer and returns the original result; a transfer without a key is rejected with 400
- [ ] A second user cannot read or move money from the first user's accounts
- [ ] A transaction whose debits do not equal its credits is rejected at the domain level
- [ ] Property-based tests run thousands of random transactions without breaking an invariant
- [ ] Integration tests run against a real Postgres (Testcontainers) in CI, and CI is green
- [ ] `docker compose up` starts the API together with Postgres, Redis and RabbitMQ

## Working rules for this stage (from CONTRIBUTING.md)

- 1 issue = 1 PR. Target under 400 changed lines, hard maximum 500; EF Core migrations, lockfiles and generated files are not counted
- Tests ship in the same PR as the code they cover. Only distinct test concerns (property-based, concurrency, shared fixtures) are separate issues
- Branches are named `<type>/<issue-number>-<short-description>` with type `feat`, `fix`, `refactor`, `test`, `docs` or `chore`; every issue carries its type as a label
- Assign yourself to an issue before starting it
- Schema changes go through EF Core migrations only, including any database trigger
- Every task below becomes a GitHub issue under the "Stage 1" milestone, labelled `stage-1`, its workstream and its type
- Stack PRs by layer: Domain, Application, Infrastructure, then Api

## Workstream 0: Close out Phase 0

- [ ] Verify the Phase 0 checkpoint: fresh-clone build and test, green CI on main, branch protection (PR, 1 approval, CI check, up-to-date branch). No PR expected
- [ ] Add a pull request template (summary, linked issue, changes, testing, checklist)
- [ ] Add issue templates (task with motivation and checkbox acceptance criteria; bug)
- [ ] Fix the garbled sentence in the README Architecture paragraph
- [ ] Commit this plan to `docs/` and link it from the README
- [ ] Create a GitHub Project board (Backlog, In progress, In review, Done). No PR expected

## Workstream 1: Ledger domain, `Ledger.Domain`

No dependencies on EF Core, ASP.NET or any framework. Each issue includes its own unit tests.

- [ ] `Money` value object: `decimal` plus ISO currency, never floating point; rejects negatives; arithmetic only between matching currencies
- [ ] `Account` entity with `AccountType` (Asset, Liability, System) and status (active, frozen, closed)
- [ ] Immutable `LedgerEntry` with `Direction` (Debit, Credit), positive amount and currency
- [ ] `Transaction` aggregate: at least two entries, single currency, debits equal credits, `LedgerInvariantException` otherwise; carries an optional idempotency key
- [ ] Transaction status transitions (pending to posted or failed, posted to reversed); illegal transitions rejected
- [ ] Reversal as new balancing entries; posted entries never edited or deleted
- [ ] Property-based tests (FsCheck): random valid sets always balance; random tampered sets are always rejected
- [ ] Remove the placeholder `UnitTest1.cs` files

## Workstream 2: Application layer, `Ledger.Application`

Each handler issue includes its own unit tests with in-memory fakes.

- [ ] MediatR, FluentValidation and the folder-per-feature layout; Application references only Domain
- [ ] Pipeline behaviors: validation and structured request logging
- [ ] Repository and unit-of-work interfaces, including find-by-idempotency-key
- [ ] `CreateAccount` command, handler and validator
- [ ] `GetBalance` query: derived from ledger entries, never stored
- [ ] `PostTransfer` command with an idempotency key: builds a balanced transaction and posts it atomically; a repeated key returns the original result
- [ ] `ListTransactions` query with pagination
- [ ] Dev-only funding command: deposit from the external funding system account, requires an idempotency key, disabled outside Development

## Workstream 3: Persistence, `Ledger.Infrastructure`

- [ ] EF Core with Npgsql: `LedgerDbContext` and entity configurations (`numeric(18,2)`, enums as strings, indexes, foreign keys)
- [ ] Initial migration for accounts, transactions and ledger entries, including a unique index on `transactions.idempotency_key`
- [ ] Shared Testcontainers fixture (must land before the issues that use it)
- [ ] Repository implementations, with integration tests
- [ ] Unit of work running a posting in one DB transaction at `Serializable` isolation, with a rollback integration test
- [ ] Retry on serialization failure (Postgres error 40001) with a small bounded backoff
- [ ] Resolve duplicate idempotency-key races: on a unique violation, return the existing transaction
- [ ] Enforce append-only ledger entries: no update or delete paths, optional database trigger created in a migration
- [ ] Seed system accounts (external funding, suspense, platform revenue) via a migration
- [ ] Concurrency test: many parallel transfers on one account produce no lost updates, no double spends and no negative balance

## Workstream 4: API and minimal auth, `Ledger.Api`

Each endpoint issue includes its own API tests.

- [ ] Dockerfile for `Ledger.Api`; enable the `api` service in `docker-compose.yml` with healthcheck-based `depends_on`
- [ ] `/health` endpoint with a database check
- [ ] OpenAPI document and Swagger UI (Development only)
- [ ] Global error handling mapping domain and validation exceptions to `ProblemDetails`
- [ ] API test fixture (`WebApplicationFactory` plus Testcontainers)
- [ ] Register and login with ASP.NET Core Identity, issuing a short-lived JWT
- [ ] Ownership policy: a user can only read or move money from accounts they own, with a cross-user test
- [ ] Create-account endpoint
- [ ] Balance and list-transactions endpoints
- [ ] Post-transfer endpoint requiring `Idempotency-Key` (missing key returns 400; repeated key returns the original result)
- [ ] Dev-only funding endpoint requiring `Idempotency-Key`, routable in Development only
- [ ] Run integration tests in CI on every pull request
- [ ] Document the JWT signing key in `.env.example` and the README

## Suggested order

1. **Workstream 0** first. It is small and unblocks clean issue tracking.
2. **Workstream 1** next. Everything else depends on the domain types.
3. **Workstreams 2 and 3 in parallel** once the domain types exist; the repository interfaces are the contract between them. Land the Testcontainers fixture before the persistence issues that use it.
4. **Workstream 4** once there is something to expose. Start with the Dockerfile, health endpoint, error handling and API fixture, which do not depend on the rest.
5. Finish with the cross-user test, the concurrency test, the idempotency replay check and the exit-criteria demo.

## Definition of done (per issue)

- Acceptance criteria met and domain invariants preserved
- Tests added in the same PR; CI is green
- At least one reviewer approved the PR
- `.editorconfig` rules met and `dotnet format` clean
- Docs or README updated if behavior or setup changed

## Risks specific to this stage

| Risk | Mitigation |
| --- | --- |
| `Serializable` isolation causes serialization failures under load | Bounded retry now; measure in Stage 5 before relaxing |
| Domain model churn after persistence is built | Finish and review the domain types before starting the migration |
| Auth scope grows | Limit to register, login, JWT and the ownership check; defer refresh tokens and roles |
| Idempotency scope grows into Stage 2 work | Stage 1 is only the header, the unique index and the duplicate-race handling; no middleware, Redis or request hashing |
| Integration tests slow down CI | Share one container across the test run through a fixture |