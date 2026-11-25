# Finance — implementation and acceptance

Updated 2026-11-25. Baseline: Finance workbook sheets MoSCoW build, BR FIN, Handoff and UAT r2. This is an implementation record, not evidence that every business rule or legal requirement has passed.

## Implemented controls

| Area | Implementation | Verification |
| --- | --- | --- |
| Access | Cookie identity, API role checks, scoped EF queries, scoped CSV/download | API authentication/scope tests |
| DoA | Configured role/operation approval limits; no client-supplied identity; independent payout receiving-account change with history/stale proposal guard | API monetary-limit test and extension account/refund tests |
| Import | Read-only preview, explicit commit, canonical fingerprint, row source identity, quarantine, legacy source-key lookup | API preview/idempotency tests, CSV parser tests |
| Classification | Effective mapping, no overlap/version collision, retry Unmapped, use event date, transactional exceptions | API reclassification test |
| Reconciliation | Signed statement allocations, order links, one/many bank allocations, bank/statement/order caps, server policy tolerance, independent checker | API allocation tests and browser workflow |
| Exceptions | Open/WaitingExternal/PendingApproval/Closed, verified result or accept-variance proposal, independent reviewer, evidence and activity history | API state transition tests |
| Ledger | Balanced debit/credit, event unique key, conflicting retry rejection, month normalization, transactional period recheck, reversal, approved reopen | API journal/period tests |
| Refund | External confirmation, remaining eligible cap, source retry, independent internal request approval, no payment execution or physical inventory update | Extension SQLite tests |
| Cost | Frozen FIFO/weighted-average policy, receipt layers, provisional/final delta, return QC restock cost reversal and quantity cap | Extension SQLite tests |
| Delivery fee | Source event dedup, evidence, links existing statement or posts a single fee journal; future duplicated fee import rejected for manual linking | Extension SQLite tests |
| Documents | Bytes/hash, versions, immutable reviewed revisions, independent review, scoped download | Extension SQLite tests |
| Reports | Ledger accounts, management revenue/cost/profit, matched bank cash, source status, snapshot/definition, scoped CSV and journal drill-down | API cash/scope tests |
| Source authority | Version/checkpoint/mapping/open-work manifest, Admin proposal and Checker approval, old writer locked, rollback guarded | API source-switch test |

## Acceptance boundaries requiring further evidence

- Legal invoice issuance, statutory fields/timing, signing/provider integration and retention policy require a confirmed provider and legal validation. Document verification currently confirms evidence review only.
- H02 and H07–H13 have local finance models and related controls, but live OMS/WMS/Procurement transport, schema/envelope/retry interoperability and real-shop UAT are not accepted by local tests.
- Source switching preserves local IDs and an open-work/mapping manifest. Live connector checkpoint handover, historical alias transformation and external writer shutdown require connector-specific acceptance.
- The original overview/P&L/balance-sheet/cash-flow/channel/store/customer charts continue to show labelled illustrative datasets. The new ledger report uses API data. Complete statutory statements and all retail dimensions need approved account mappings, opening balances and operational source coverage.
- Reconciliation API supports multiple bank allocations; the first UI uses one selected bank transaction. Allocation evidence is user-supplied and independently reviewed, not automatically validated against external platforms.
- DoA has configurable role/operation monetary limits and a payout-account proposal/review workflow. The approved company policy, currency-specific limits, permission administration, splitting-detection policy and actual independent bank/channel validation still require business signoff and further acceptance.
- Core API activities and extension actions are recorded, but a tamper-resistant external audit archive and retention administration are not implemented.
- No real transfers, tax filing, marketplace refund execution or automatic inventory adjustment are performed.

## UAT mapping

| Case | Evidence available | Remaining evidence |
| --- | --- | --- |
| FIN-IM / IT-11 | Preview, duplicate batch/event controls, row quarantine tests | Real channel file schemas and adjustment mapping samples |
| IT-06 / FIN-SA | Dispute/tolerance guard and order/statement/bank allocation controls | Same-period real shop settlement and bank sources |
| FIN-EX | WaitingExternal and closure result/independent review tests | Real external ticket and recovery outcome verification |
| FIN-SC | API/read/export scoped tests | Full role/DoA policy signed off by owner |
| FIN-SW | Versioned local authority-switch test | Live connector checkpoint and writer handover |
| IT-04 / AC05 | Refund dedup/cap and return cost tests; no physical stock writes | Actual RMA/QC integration and real-shop reconciliation |

Do not infer full UAT acceptance from build, lint or happy-path browser tests. Test reports are generated locally and ignored by Git; CI generates its own artifacts when run remotely.

## Verification record

- Backend regression: 22 passing tests, 0 failures/skips after final migration `ApprovalAccountsAndJournalEvidence` (2026-11-25).
- CSV parser: 3 passing tests, including BOM/CRLF/quotes, missing ID quarantine input, invalid money/date rejection.
- Browser regression: 4/4 Chromium tests passed on 2026-11-25: real login/import/commit/order/statement/bank approval, journal balancing and monthly lock, classification/external waiting/exception approval, and payout-account approval. Results must be regenerated after UI changes.
- Migrations preserve developer database files; the tests use their own temporary databases. No deletion or reset of the developer database was needed for this implementation.
- `.db-wal` and `.db-shm` sidecars were removed from the Git index; existing local files were not deleted by that operation.
