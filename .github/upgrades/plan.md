# .github/upgrades/plan.md

## Table of Contents
- [1 Executive Summary](#executive-summary)
- [2 Migration Strategy](#migration-strategy)
- [3 Dependency Analysis & Tiering](#dependency-analysis--tiering)
- [4 Per-Tier Specifications](#per-tier-specifications)
  - [Tier 1 (Leaf nodes)](#tier-1-leaf-nodes)
  - [Tier 2](#tier-2)
  - [Tier 3](#tier-3)
  - [Tier 4 (Applications / Samples / Tools)](#tier-4-applications--samples--tools)
- [5 Package Update Reference (grouped by tier)](#package-update-reference-grouped-by-tier)
- [6 Breaking Changes Catalog (expectations)](#breaking-changes-catalog-expectations)
- [7 Testing & Validation Strategy](#testing--validation-strategy)
- [8 Risk Management](#risk-management)
- [9 Source Control Strategy](#source-control-strategy)
- [10 Success Criteria](#success-criteria)
- [11 Execution Sequence & Checklist](#execution-sequence--checklist)
- [12 Notes & Assumptions](#notes--assumptions)

---

# 1 Executive Summary

Scenario: Upgrade the solution to target `.NET 10.0` (Bottom-Up strategy).

Key facts (from assessment.md):
- Total projects: **72** (mix of ClassLibrary, DotNetCoreApp, WPF variants, test projects)
- Total NuGet packages inventoried: **13** (4 recommended upgrades, 1 incompatible, some deprecated test packages)
- Major package targets: `10.0.2` (for several Microsoft/System packages)
- Branch active for planning: `upgrade-to-NET10` (no pending working-tree changes at time of analysis)

Goal: Produce a tiered, dependency-first plan that upgrades library projects (leaf nodes) first, stabilizes each tier, then proceeds upward until applications and test projects are migrated.

Primary constraints:
- Must respect dependency order (Bottom-Up)
- Include every suggested NuGet package upgrade flagged in the assessment
- Handle deprecated test packages (xUnit.*) as part of the plan

Quick recommendation:
- Accept package updates as part of this upgrade (you already selected this). All packages flagged in the assessment will be included in per-tier package update lists.

# 2 Migration Strategy

Selected approach: **Bottom-Up (Dependency-First)**

Rationale:
- Large solution (72 projects) with clear dependency tiers and many shared libraries.
- Lower risk model: upgrade leaf libraries first, so dependent projects always build against upgraded dependencies.
- Avoids multi-targeting complexity by keeping consumers on older TFs until their dependencies are upgraded and validated.

Strategy highlights:
- Each tier is a migration milestone. Work is batched per tier (all projects in the tier updated and stabilized together).
- After a tier is complete (build & tests pass), proceed to the next tier.
- Test projects are migrated last (they depend on other projects).

# 3 Dependency Analysis & Tiering

Method:
- Used dependency data from `assessment.md` to group projects into tiers. Leaf nodes (projects with zero internal project references) are Tier 1.
- Verified that no project in Tier N depends on Tier N+1. Where cycles would appear, they are merged into a single tier (no cycles present in the assessment graph).

Summary tier breakdown (top-level):

- Tier 1 (Leaf nodes — foundational libraries with no internal project references)
  - Representative projects: `docs\docs-dummy.csproj`, `src\foundation\src\MigraDoc\features\MigraDoc.Features\MigraDoc.Features.csproj`, `src\tools\src\CopyAsLink\CopyAsLink.csproj`, `src\tools\src\NRT-Tests\NRT-Tests.csproj`, `src\foundation\src\shared\src\PdfSharp.WPFonts\PdfSharp.WPFonts.csproj`, `src\foundation\src\shared\src\PdfSharp.Shared\PdfSharp.Shared.csproj` (see full list in `assessment.md`).
  - Reason: these projects have zero or minimal internal dependencies and are safe to upgrade first.

- Tier 2 (Core libraries used across many projects)
  - Representative projects: `PdfSharp.System`, `PdfSharp.Testing`, `PdfSharp.Fonts`, `PdfSharp.Cryptography`, `PdfSharp.Snippets`.
  - Reason: these are foundational shared libraries consumed by many other projects.

- Tier 3 (Feature libraries, rendering, charting)
  - Representative projects: `PdfSharp.Charting`, `MigraDoc.Rendering`, `MigraDoc.DocumentObjectModel`.
  - Reason: depend on Tier 2 shared libraries; they implement business logic and rendering.

- Tier 4 (Applications, samples, test apps, and test projects)
  - Representative projects: `PdfSharp` app variants, `HelloWorld` samples, WPF/GDI variants and all test projects.
  - Reason: these projects depend on many previous tiers and should be migrated last.

Dependency visualization: See `assessment.md` "Projects Relationship Graph" mermaid chart (it is derived from the same dependency data used here).

Notes:
- Full per-project dependency counts and dependants are available in `assessment.md`.
- If any circular dependencies are discovered during execution, treat the entire cycle as a single tier and upgrade together.

# 4 Per-Tier Specifications

This section defines per-tier metadata, package updates, expected breaking-change exposure, and validation requirements.

## Tier 1 (Leaf nodes)

Tier metadata:
- Tier number/name: Tier 1 — Leaf nodes
- Projects included (representative; full list in assessment.md):
  - `docs\docs-dummy.csproj`
  - `src\foundation\src\MigraDoc\features\MigraDoc.Features\MigraDoc.Features.csproj`
  - `src\tools\src\CopyAsLink\CopyAsLink.csproj`
  - `src\tools\src\NRT-Tests\NRT-Tests.csproj`
  - `src\foundation\src\shared\src\PdfSharp.WPFonts\PdfSharp.WPFonts.csproj`
  - `src\foundation\src\shared\src\PdfSharp.Shared\PdfSharp.Shared.csproj`
- Dependencies on previous tiers: External NuGet packages only
- Estimated complexity: Low

Upgrade details:
- Framework change: change `TargetFramework`/`TargetFrameworks` to include `net10.0` per assessment proposals for those projects.
- Package updates: none mandatory for many leaf nodes (assessment shows most leaf projects compatible). Any package with Suggested Version that affects these projects will be applied (see Section 5).

Breaking-change exposure:
- Low — leaf projects are small and rarely use APIs that changed substantially.

Validation requirements:
- Build each upgraded project targeting `net10.0`.
- Run any unit tests local to the project (if present) or small smoke tests.
- Confirm that projects publish (for NuGet-packaged projects) and produce expected artifacts.

Tier completion criteria:
- All projects in tier build successfully for `net10.0`.
- No new critical NuGet vulnerabilities introduced.
- Local unit tests (if any) pass.

## Tier 2 (Core libraries)

Tier metadata:
- Projects included (representative): `PdfSharp.System`, `PdfSharp.Testing`, `PdfSharp.Fonts`, `PdfSharp.Cryptography`, `PdfSharp.Snippets`.
- Dependencies: Tier 1
- Estimated complexity: Medium

Upgrade details:
- Framework change: add/ensure `net10.0` TFM per assessment.
- Package updates: critical package updates reported in assessment (see Section 5). Notable packages affecting Tier 2 include `Microsoft.Extensions.Logging.Abstractions` (target `10.0.2`) and `System.Security.Cryptography.Pkcs` (target `10.0.2`) used by crypto and system layers.

Breaking-change exposure:
- Medium — cryptography packages may have API surface changes; test coverage matters.

Validation requirements:
- Build all Tier 2 projects together.
- Run unit tests that exercise core functionality.
- Execute integration checks that Tier 3 projects might rely on (simple API contract checks).

Tier completion criteria:
- Full tier build passes
- Unit tests pass
- No runtime exceptions in smoke integration runs

## Tier 3 (Feature libraries)

Tier metadata:
- Projects included (representative): `PdfSharp.Charting`, `MigraDoc.Rendering`, `MigraDoc.DocumentObjectModel`.
- Dependencies: Tier 1 & Tier 2
- Estimated complexity: Medium ? High (rendering + large LOC)

Upgrade details:
- Framework change: add or ensure `net10.0` TFM for multi-targeted projects where appropriate.
- Package updates: packages used by feature layers that were flagged in assessment (logging packages for test harnesses or samples will be upgraded when present).

Breaking-change exposure:
- Medium-to-high for rendering and charting code — larger LOC and possible API changes in graphics/IO areas require careful testing.

Validation requirements:
- Build entire tier and run feature test suites.
- Run sample rendering scenarios (generate sample PDFs) to validate runtime behavior.

Tier completion criteria:
- Tier builds successfully
- Feature tests / smoke scenarios complete without major regressions

## Tier 4 (Applications / Samples / Tools / Tests)

Tier metadata:
- Projects included: application binaries, sample apps, all test projects
- Dependencies: all previous tiers
- Estimated complexity: Medium

Upgrade details:
- Migrate program entry points (if necessary) and update project `TargetFramework` to include `net10.0`.
- Update test SDKs and test runner packages to compatible versions, taking special care with deprecated packages (e.g., `Xunit.Core` flagged as deprecated).

Breaking-change exposure:
- Low-to-medium for app startup; medium for tests due to deprecated runner packages.

Validation requirements:
- Full-solution build
- Execute test suites (unit + integration)
- Run manual/end-to-end smoke tests for samples and apps (render sample docs)

Tier completion criteria:
- Full-solution build with `net10.0` targets present where applied
- All tests pass
- No outstanding critical vulnerabilities from NuGet packages

# 5 Package Update Reference (grouped by tier)

Per the assessment, include all suggested package updates. Grouped here by tier scope (packages may affect projects in multiple tiers; group by where they primarily matter):

Tier 1 Package Updates:
- None required by assessment for pure leaf nodes in most cases (verify project by project during tier update)

Tier 2 Package Updates (core/shared libraries):
- `Microsoft.Extensions.Logging.Abstractions`: 8.0.3 ? 10.0.2
  - Affects: `PdfSharp.BarCodes`, `PdfSharp.Cryptography`, `PdfSharp.Fonts`, `PdfSharp.Shared`, `PdfSharp.System`, `PdfSharp.Testing` (per assessment)
  - Reason: Align logging abstractions to .NET 10 runtime; avoid mixed binding.

- `System.Security.Cryptography.Pkcs`: 8.0.1 ? 10.0.2
  - Affects: `PdfSharp.Cryptography`, `PdfSharp.Snippets` and others that consume crypto functionality.
  - Reason: Use latest crypto implementations and security fixes.

Tier 3 Package Updates (feature libraries):
- `Microsoft.Extensions.Logging.Console`: 8.0.1 ? 10.0.2
  - Affects sample apps and some test harnesses (listed projects in assessment)

- `System.Resources.Extensions`: 8.0.0 ? 10.0.2
  - Affects `PdfSharp-gdi` per assessment (resource handling in Windows variants)

Tier 4 Package Updates (tests & runners):
- Test SDKs: `Microsoft.NET.Test.Sdk` is present (17.12.0) — assessment marked compatible.
- Deprecated packages to address (special handling):
  - `Xunit.Core` 2.9.3 — flagged as deprecated in `assessment.md` and used in many test projects.
    - Plan: Replace deprecated xUnit internals with supported `xunit` packages (e.g., update test projects to xUnit 2.10+ or the recommended runner packages) or migrate test projects to `dotnet test` compatible packages. This is a test-tier activity and should be done during Tier 4.

Notes on package updates:
- Use exact suggested versions from `assessment.md` (e.g., `10.0.2` where the tool returned that supported version).
- Apply package updates together with project TF changes for tier: update `PackageReference` entries after changing `TargetFramework`.
- For EF/Core or other frameworks (none flagged as major here) follow vendor migration docs.

# 6 Breaking Changes Catalog (expectations)

The assessment shows no automated binary incompatible API findings, but expect the usual categories when moving to .NET 10:
- API removals/deprecations in certain BCL areas — detect during compilation
- Crypto package APIs may have additions or altered behavior — validate cryptographic flows and signatures
- Resource handling and Windows-specific behaviors (GDI/WPF) — test rendering codepaths
- Test runner changes: deprecated xUnit packages may require updating test project references and possibly adjusting attributes or runner settings

Mitigation:
- Rely on compilation errors as initial discovery; use unit and integration tests to detect behavioral/regression issues.
- Flag risky libraries (crypto, rendering) for additional manual validation and smoke tests.

# 7 Testing & Validation Strategy

Testing is multi-level and cumulative.

Per-tier testing steps:
- Per-project build and unit tests (if present) after TF+package updates
- Tier-level integration tests: build full tier and run tests that exercise cross-project contracts
- Lower-tier regression checks: after upgrading tier N, run a minimal set of functionality tests for dependent higher-tier projects to validate ABI/contract compatibility

Full-solution validation (final tier completion):
- Full solution build (all projects targeting proposed TFs where applied)
- Execute complete unit & integration test suites
- Run a set of end-to-end smoke tests that cover PDF generation, rendering, and sample-run flows

Testing checklist (for each tier):
- [ ] All projects in tier build for `net10.0` target(s)
- [ ] Unit tests in tier pass
- [ ] Integration/smoke tests touching tier pass
- [ ] No new critical warnings or vulnerabilities introduced by package updates

# 8 Risk Management

Top risks:
- Cryptography package changes cause subtle behavioral differences — mitigations: add unit tests for crypto flows and run sample vector checks.
- Rendering/graphics regressions — mitigations: run sample rendering smoke scenarios, compare outputs where possible.
- Deprecated test packages (xUnit) — mitigations: plan test package upgrade during Tier 4 with compatibility layer if needed.

Contingency / rollback:
- Each tier should be a single PR. If issues discovered, revert that PR (or fix within the branch) and document the root cause.
- Keep branches small and review changes carefully. If a package update causes failures, revert that package update and address in a follow-up PR if necessary.

# 9 Source Control Strategy

- Working branch for planning and eventual changes: `upgrade-to-NET10` (already created and active).
- Branching approach per tier:
  - Create a feature branch for each tier upgrade (ex: `upgrade/net10/tier-1`, `upgrade/net10/tier-2`, ...), or a single PR per tier with multiple project changes batched together.
  - Each PR should contain:
    - Project file TF updates for all projects in tier
    - PackageReference updates for the tier
    - Unit test fixes (if any)
    - A checklist with validation steps and test results
- PR gating: require CI build and test run before merging to the upgrade branch

# 10 Success Criteria

The migration is complete when:
1. All projects target their proposed `net10.0` (where assessment proposed it)
2. All package updates listed in assessment are applied
3. Solution builds without errors and critical warnings
4. Unit and integration tests pass
5. No remaining flagged security vulnerabilities from assessment
6. Manual smoke tests for rendering and sample apps succeed

# 11 Execution Sequence & Checklist

Ordering rules:
1. Must upgrade tiers in strict bottom-up order (Tier 1 ? Tier 2 ? Tier 3 ? Tier 4)
2. Do not start Tier N+1 until Tier N meets completion criteria

Per-tier operations (batched across projects within a tier):
- Preparation: review projects in tier and dependencies
- Update: change TFs in project files and update all package references (single PR per tier)
- Compile & fix compilation errors
- Test: run unit and integration tests
- Stabilize: address findings, update documentation, mark tier complete

Tasks per tier (suggested PR content):
1. Preparation & branch create: `upgrade/net10/tier-X`
2. Update project TFs & packages (single commit or coherent commits)
3. Compile and fix obvious API breaks
4. Run tests and capture results
5. Stabilization & PR review

# 12 Notes & Assumptions

- All package target versions were taken from `assessment.md` and from checks performed earlier (`10.0.2` where indicated).
- Test-related deprecated packages (ex: `Xunit.Core` 2.9.3) are flagged to be replaced in Tier 4.
- This plan is planning-only: no files are modified by this document. Execution must follow plan steps, using separate execution agent or developer actions.
- For exact per-project package references and line-by-line project-file change instructions, generate per-tier tasks (tasks.md) from this plan when ready.

---

Appendix: Reference to assessment
- Full project-by-project current/proposed TFs, package mappings, and dependency graphs are in `.github/upgrades/assessment.md` created during Analysis. Use that file as the authoritative source for exact project lists when applying changes.




