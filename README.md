# Tenekon.MethodOverloads.SourceGenerator

A C# source generator that creates extension method overloads by treating a selected parameter window as optional and emitting legal, unique subsequences.

## Quickstart
1) Mark a method with `[GenerateOverloads]` or a type with `[GenerateMethodOverloads(Matchers = ...)]`.
2) (Optional) Add `[OverloadGenerationOptions(...)]` to control matching and output.
3) Build the project. Generated overloads appear in `MethodOverloadsExtensions_<Namespace>.g.cs`.

## Key behavior
- Generates overloads from a contiguous optional window of parameters.
- Omits only legal subsets (no ref/out/in removal, no empty parameter list).
- Dedupes by signature and never duplicates existing overloads.

See `docs/GENERATOR.md` for full behavior and rules.
