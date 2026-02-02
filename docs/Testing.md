# Testing the Generator

Tests are acceptance-criteria driven: expected overloads live in the ref files and the generator output is compared against them.

## Quickstart
1) Run `dotnet test --nologo` at repo root.
2) Add or update a Class_*.cs acceptance criteria file, then re-run tests.

## What is tested
- Which overloads are generated
- Which overloads are not generated
- Accessibility of generated overloads
- Classic extension vs C# 14 extension block output
- Collision avoidance and de-duplication
- Expected diagnostics (MOG*** IDs) via SuppressMessage markers in acceptance criteria
- Public API surface snapshots

## Source of truth
The expected overloads live in:
- ref/Tenekon.MethodOverloads.AcceptanceCriterias/*.cs

Each file defines:
- One or more target classes/methods with attributes
- A Class_*_AcceptanceCriterias static class that describes the expected extension methods

## How it works
1. Load all reference .cs files
2. Extract expected signatures
   - Looks at the Class_*_AcceptanceCriterias methods
   - Also parses C# 14 extension(...) { ... } blocks
3. Prepare generator inputs
   - Removes the acceptance criteria helper classes
   - Keeps only the actual target classes and attributes
4. Run the generator on the filtered compilation
5. Extract actual signatures from generated .g.cs output
6. Compare
   - Expected and actual signatures are compared as sets
   - Any missing or extra signatures fail the test, with a diff list
   - Expected diagnostics are compared by ID per Class_* file

## Running tests
```
dotnet test --nologo
```

## Adding new acceptance criteria
1. Add or update a Class_*.cs file in ref/Tenekon.MethodOverloads.AcceptanceCriterias.
2. Add the expected overloads to the corresponding Class_*_AcceptanceCriterias class.
3. Run tests; update the generator until the test passes.
