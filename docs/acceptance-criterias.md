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

## How tests consume it

There are two main test paths:

1) **Acceptance criteria comparison (in-memory)**
   - Tests read the `ref/*.cs` files and build an in-memory compilation.
   - Expected overload signatures come from `Class_*_AcceptanceCriterias`.
   - Expected diagnostics are inferred from `[SuppressMessage]` attributes (MOG IDs).
   - The analyzer runs with `reportSuppressedDiagnostics: true`, so diagnostics suppressed by those attributes
     are still compared (`Diagnostic.IsSuppressed == true`).
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

## Error-level diagnostics

Some MOG diagnostics are errors by default (e.g. `MOG007`, `MOG012`). The project still compiles because the
`[SuppressMessage]` attributes that declare them as expected also suppress them in the build. No severity
overrides are needed.

## .editorconfig (IDE suppression and generated code)

File:
- `ref/Tenekon.MethodOverloads.AcceptanceCriterias/.editorconfig`

Role:
- Suppresses ReSharper/IDE warnings that are noisy in test fixtures.
- Marks `*generated.cs` as generated code to avoid editor inspections.

## How expected diagnostics are declared

Use `[SuppressMessage]` with a `CheckId` that includes the MOG ID:
- Example: `[SuppressMessage("MethodOverloadsGenerator", "MOG012")]`

Place it on the class or member that contains the diagnostic location. It does two things:
- Tests interpret it as an **expected diagnostic** for the enclosing class.
- It suppresses the diagnostic in the real build and in the IDE.

`MOG002` is reported at the `typeof` reference in the target's `Matchers`, so its `[SuppressMessage]` belongs on
the target class, not on the matcher.

