# .github/upgrades/tasks.md

This file contains sequenced execution tasks for the `.NET 10` upgrade (branch: `upgrade-to-NET10`).

> IMPORTANT: This file is authoritative for execution. Do not edit it manually while an execution agent runs it.

---

## Task list (sequential)

### [?] TASK-000: Prerequisites verification
- Actions:
  1. Verify .NET 10 SDK is installed on the machine and global.json (if present) is compatible.
  2. Verify Git working tree is clean on `upgrade-to-NET10` branch.
  3. Ensure CI has capacity to run full-solution builds and tests.
- Validation:
  - `dotnet --list-sdks` shows SDK for `net10.0` or upgrade validation tool confirms.
  - Git status shows no uncommitted changes.

### [ ] TASK-001: Generate per-tier project lists and create Tier branches
- Actions:
  1. From `assessment.md`, produce definitive lists of projects for Tier 1..Tier 4.
  2. Create feature branches for each tier: `upgrade/net10/tier-1`, `upgrade/net10/tier-2`, `upgrade/net10/tier-3`, `upgrade/net10/tier-4` (branches off `upgrade-to-NET10`).
- Validation:
  - Branches created and visible locally.
  - Per-tier lists present in tasks or attached artifact.

### [ ] TASK-002: Tier 1 — Update Target Frameworks (Leaf nodes)
- Scope: All Tier 1 projects (see `assessment.md` Tier 1 list)
- Actions:
  1. On branch `upgrade/net10/tier-1` update `TargetFramework`/`TargetFrameworks` to include `net10.0` as proposed.
  2. Restore and build each project targeting `net10.0`.
  3. Run project-local unit tests or smoke checks.
- Validation:
  - Each project restores and builds targeting `net10.0` with 0 errors.
  - Local unit tests (if present) pass.
- Commit message: `TASK-002: Tier 1 - Add net10.0 targets`

### [ ] TASK-003: Tier 2 — Core libraries (TFMs + package updates)
- Scope: Tier 2 projects (`PdfSharp.System`, `PdfSharp.Testing`, `PdfSharp.Fonts`, `PdfSharp.Cryptography`, `PdfSharp.Snippets`, etc.)
- Actions:
  1. On branch `upgrade/net10/tier-2` update TFMs to include `net10.0` where proposed.
  2. Apply package updates per plan (example):
     - `Microsoft.Extensions.Logging.Abstractions` -> `10.0.2`
     - `System.Security.Cryptography.Pkcs` -> `10.0.2`
  3. Restore, build tier projects and run unit tests.
- Validation:
  - Tier build succeeds (0 errors).
  - Unit tests pass.
- Commit message: `TASK-003: Tier 2 - TFMs and package upgrades`

### [ ] TASK-004: Tier 2 verification and stabilization
- Actions:
  1. Run integration smoke tests that exercise Tier 2 APIs used by Tier 3.
  2. Fix any compilation or test failures in Tier 2 code.
- Validation:
  - Integration smoke tests pass.
  - No unresolved compilation issues.
- If failures: mark task as failed and stop.

### [ ] TASK-005: Tier 3 — Feature libraries (TFMs + package updates)
- Scope: `PdfSharp.Charting`, `MigraDoc.Rendering`, `MigraDoc.DocumentObjectModel`, etc.
- Actions:
  1. On branch `upgrade/net10/tier-3` update TFMs to include `net10.0` per plan.
  2. Apply package updates affecting Tier 3 (e.g., `Microsoft.Extensions.Logging.Console` -> `10.0.2`, `System.Resources.Extensions` -> `10.0.2`).
  3. Restore and build the entire tier; run feature tests and sample rendering smoke scenarios (generate PDFs).
- Validation:
  - Full tier builds successfully.
  - Sample rendering produces expected outputs without runtime exceptions.
- Commit message: `TASK-005: Tier 3 - TFMs and package upgrades`

### [ ] TASK-006: Tier 3 verification and stabilization
- Actions:
  1. Run broader integration tests involving Tier 1-3 interactions.
  2. Fix regressions.
- Validation:
  - Integration tests pass.
  - No critical runtime regressions on sample outputs.

### [ ] TASK-007: Tier 4 — Applications, samples, tools, and tests
- Scope: All application projects, sample projects, test projects
- Actions:
  1. On branch `upgrade/net10/tier-4` update TFMs to include `net10.0` for apps and samples.
  2. Replace deprecated test packages (example: replace `Xunit.Core` 2.9.3 with supported xUnit packages / upgrade test runner packages).
  3. Update test SDKs if necessary.
  4. Restore, build and run full test suites.
- Validation:
  - Full-solution tests pass (unit + integration).
  - Test runner outputs show 0 failed tests (or acceptable baseline if documented).
- Commit message: `TASK-007: Tier 4 - Apps, samples and test upgrades`

### [ ] TASK-008: Full-solution verification
- Actions:
  1. Merge tiers into `upgrade-to-NET10` (or rebase as per branch strategy) after each tier is marked complete.
  2. Run CI full-solution build and test pipeline.
  3. Execute manual smoke tests (render sample PDFs, launch sample apps if feasible).
- Validation:
  - CI build: 0 errors, tests passing
  - Manual smoke test checklist passed
- If CI fails: stop execution and report failure.

### [ ] TASK-009: Finalize and prepare PRs to main
- Actions:
  1. Ensure all PRs per tier are open with description and checklist.
  2. Tag and document any known issues or follow-ups.
  3. Create final merge or release branch as per repo policy.
- Validation:
  - PRs contain pass/fail logs and validation checklist.
  - Approval process begins.

---

## Notes
- Each task must be executed in order. Do not begin a later task until the previous task is complete and validated.
- All state changes (Starting/Completing/Failing tasks) must be reported using the `upgrade_track_tasks_execution_progress` tool before and after execution of the task actions.
- When ready for execution, confirm and I will: open `tasks.md` in the editor and wait for your confirmation to start execution. After your confirmation I will call `upgrade_track_tasks_execution_progress` with an empty `stateChanges` to retrieve the next task to run.


