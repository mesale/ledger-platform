---
name: Bug report
about: Something is broken or behaving incorrectly
title: "fix: "
labels: bug
---

## Summary
<!-- One or two sentences: what is wrong? -->

## Business Impact
<!-- Who or what is affected? e.g. incorrect balances, duplicate transfers, failed saga compensation. -->

## Affected Area
- [ ] Domain
- [ ] Application
- [ ] Infrastructure
- [ ] Api
- [ ] MockRail
- [ ] Frontend
- [ ] Infra / CI

## Ledger Integrity Risk
- [ ] Can break debits = credits, or produce an incorrect balance
- [ ] Can cause a duplicate or lost transfer (idempotency / saga)
- [ ] No financial-correctness impact

## Steps to Reproduce
1.
2.
3.

## Expected Behavior

## Actual Behavior

## Logs / Payloads
<!-- Stack traces, logs, request/response JSON. Remove secrets and real account data. -->

```
```

## Environment
- .NET SDK version:
- OS:
- Docker container status (`docker compose ps`):
- Commit / branch:

## Additional Context