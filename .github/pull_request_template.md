## Summary
<!-- What changes and why? Keep it to a few sentences. -->

## Linked Issue
Closes #<issue_number>

## Type of Change
- [ ] `feat`: new feature or capability
- [ ] `fix`: bug fix
- [ ] `refactor`: no behavior change
- [ ] `test`: tests only
- [ ] `docs`: documentation only
- [ ] `chore`: tooling, dependencies, CI

## Layer(s) Touched
- [ ] Domain
- [ ] Application
- [ ] Infrastructure
- [ ] Api
- [ ] MockRail
- [ ] Frontend
- [ ] Infra / CI

## Changes Made
-
-

## How Was This Tested?
- [ ] Unit tests added / updated
- [ ] Integration tests added / updated (Testcontainers)
- [ ] Tested locally with Docker Compose services
- [ ] Not applicable (explain):

<!-- Include commands run, or request/response examples for API changes. -->

## Fintech Guardrails
<!-- Tick what applies; write N/A for what doesn't. -->
- [ ] Debits equal credits is preserved; no ad-hoc balance updates
- [ ] No `float`/`double` used for money (`decimal` or a money value object only)
- [ ] Write endpoints that move money require and honor an idempotency key
- [ ] Layer boundaries respected (dependencies point inward only)
- [ ] Schema changes are in an EF Core migration (never manual DB edits)
- [ ] No secrets, credentials, or real account data committed (`.env.example` updated if new config was added)

## Breaking Changes / Rollout Notes
<!-- API contract changes, new env vars, migrations that need ordering,
     or anything the reviewer or deployer should know. Write "None" if none. -->

## Screenshots / Evidence (if applicable)
<!-- API responses, Swagger, UI screenshots, Grafana panels, etc. -->

## Checklist
- [ ] PR addresses only one issue / concern
- [ ] Diff is appropriately sized (< 400 lines, excluding migrations and lockfiles)
- [ ] Branch is rebased on latest `main`
- [ ] Follows `.editorconfig` and naming conventions
- [ ] `dotnet build Ledger.sln --configuration Release` passes locally
- [ ] `dotnet test Ledger.sln --configuration Release` passes locally
- [ ] `dotnet format Ledger.sln --verify-no-changes` passes locally
- [ ] Docs (README / code comments) updated if APIs or config changed
- [ ] Acceptance criteria from the linked issue are fully met