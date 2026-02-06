# Acceptance Criterias Project

This document explains how the acceptance criterias project is structured, how it is used in tests, and how
its editor/analyzer configuration affects diagnostics.

## Project location and purpose

Project path:
- `ref/Tenekon.MethodOverloads.AcceptanceCriterias/Tenekon.MethodOverloads.AcceptanceCriterias.csproj`

Purpose:
- Acts as the source of truth for expected overloads and diagnostics.
- Provides a real MSBuild project that the tests can build with the generator enabled or disabled.

## Structure

Source files:
- `ref/Tenekon.MethodOverloads.AcceptanceCriterias/*.cs`
- Each `Class_*.cs` file contains:
  - Target methods with generator attributes.
  - A `Class_*_AcceptanceCriterias` static class that defines expected overloads.
  - Optional `[SuppressMessage]` attributes to declare expected diagnostics.

Support files:
- `.editorconfig` (IDE/inspection suppression + generated code marker)
- `.globalconfig` (diagnostic severity overrides)

## How tests consume it

There are two main test paths:

1) **Acceptance criteria comparison (in-memory)**
   - Tests read the `ref/*.cs` files and build an in-memory compilation.
   - Expected overload signatures come from `Class_*_AcceptanceCriterias`.
   - Expected diagnostics are inferred from `[SuppressMessage]` attributes (MOG IDs).
   - See: `tests/Tenekon.MethodOverloads.SourceGenerator.Tests/Infrastructure/AcceptanceTestData.cs`.

2) **Project build validation (real MSBuild)**
   - Tests build the acceptance criterias project twice:
     - With attributes-only enabled (default).
     - With attributes-only disabled (generator fully active).
   - See: `tests/Tenekon.MethodOverloads.SourceGenerator.Tests/RefProjectBuildTests.cs`.

## Attributes-only mode

The acceptance project sets `TenekonMethodOverloadsSourceGeneratorAttributesOnly` to `true` by default:
- `ref/Tenekon.MethodOverloads.AcceptanceCriterias/Tenekon.MethodOverloads.AcceptanceCriterias.csproj`

This matches the generator/analyzer behavior:
- Generator emits attribute definitions only.
- Analyzer/overload generation are suppressed.

The tests explicitly build the project with:
- `TenekonMethodOverloadsSourceGeneratorAttributesOnly=false`
to ensure a full generation build also succeeds.

## .globalconfig (diagnostic severities)

File:
- `ref/Tenekon.MethodOverloads.AcceptanceCriterias/.globalconfig`

Role:
- Downgrades certain MOG diagnostics from **error** to **warning** so the project can build while still
  exercising those diagnostics in acceptance criteria.

Current overrides:
- `MOG007`, `MOG009`, `MOG010`, `MOG012` → warning

These diagnostics are treated as errors by default and are not suppressable in source;
the globalconfig allows the project to compile while still surfacing them for tests.

## .editorconfig (IDE suppression and generated code)

File:
- `ref/Tenekon.MethodOverloads.AcceptanceCriterias/.editorconfig`

Role:
- Suppresses ReSharper/IDE warnings that are noisy in test fixtures.
- Marks `*generated.cs` as generated code to avoid editor inspections.

## How expected diagnostics are declared

Use `[SuppressMessage]` on the target class with a `CheckId` that includes the MOG ID:
- Example: `[SuppressMessage("MethodOverloadsGenerator", "MOG012")]`

Tests interpret these attributes as **expected diagnostics** for that class, even if the suppression is only
used for documentation/clarity in the source.

