# Method Overloads Source Generator - Architecture & Behavior

This doc explains what the generator looks at, how it decides what to emit, and the rules it enforces.

## Quickstart

1) Mark a method with [GenerateOverloads] or a type with [GenerateMethodOverloads(Matchers = [...])]. Mark matcher types with [OverloadMatcher].
2) (Optional) Add [OverloadGenerationOptions(...)] to control matching and output.
3) Build the project. Generated overloads appear in MethodOverloads_<Namespace>.g.cs.

## 1) Purpose

Create extension overloads from a single method by treating a parameter span as optional and emitting only legal, unique overloads.

## 2) Terms used here

- Target method: the method the generator is trying to add overloads for.
- Optional window: a contiguous range of parameters that can be omitted.
- Subsequence: an order-preserving omission set inside the optional window.
- Matcher type: a type marked with [OverloadMatcher]. Its GenerateOverloads methods are matcher methods.
- Matcher method: a method signature used to match target methods by subsequence.
- Direct generation: GenerateOverloads on the target method itself.
- Matcher-based generation: GenerateMethodOverloads on a type (or Matchers on a method).

## 3) What the generator considers

The generator scans all syntax trees and records:
- All declared types and their methods.
- GenerateMethodOverloads on types (type-level matchers). Multiple attributes are allowed; matcher types are unioned.
- GenerateOverloads on methods, including Matchers = [...] on the method (method-level matchers).
- OverloadMatcher on types (matcher types).

Matcher types are explicit:
- Only types marked with [OverloadMatcher] are used as matchers. An unmarked type in Matchers is ignored and
  reported (MOG020); it keeps its own role, so its own GenerateOverloads methods are still direct targets.
- A matcher type is a matcher whether or not a target lists it in Matchers.
- GenerateMethodOverloads on a matcher type, or Matchers on a GenerateOverloads of one of its methods, is ignored and
  reported (MOG021). Other GenerateOverloads attributes of that matcher method still apply.
- Methods of a matcher type without GenerateOverloads are neither matcher methods nor targets.

Only ordinary methods are considered (no constructors, operators, etc.).

Methods are skipped if they are:
- Declared private or protected
- Declared in a type marked with [OverloadMatcher] (matcher types are never targets)

## 4) Optional window rules

For each target method, the generator builds one or more windows:
- Direct: from one or more GenerateOverloads attributes on the method itself.
- Any GenerateOverloads attribute that specifies Matchers must not specify window anchors; if it does, MOG012 is reported and no overloads are generated for that method.
- Multiple Matchers-only GenerateOverloads attributes are allowed; their matcher types are unioned.
- Direct windows and matcher-derived windows can be combined on the same target method.
- Matcher: from GenerateOverloads on the matcher method that matched the target.
- When a matcher method matches a target in multiple places, each matched subsequence becomes its own window.
- If a matcher method has multiple windows, a union window is computed only within that matcher group (never across different matcher methods).
- ExcludeAny: list of parameter names that must be omitted in every overload for that attribute. ExcludeAny cannot be combined with Matchers (MOG017).
- ExcludeAny entries must resolve to parameters inside the resolved window; invalid or out-of-window entries produce MOG018/MOG019 and skip that attribute.
- If ExcludeAny covers the entire window, that window produces no overloads.

## 5) Matching rules

Matcher parameters are matched against target parameters as a subsequence:
- TypeOnly: type + ref kind + params must match.
- TypeAndName: same as above, plus exact parameter name match (case-sensitive).

Matching is nullability-aware for reference types (string vs string? are different).

OverloadGenerationOptions apply in two independent frames. The matcher frame only applies when no
options are provided in the target frame. This precedence applies to RangeAnchorMatchMode,
SubsequenceStrategy, and OverloadVisibility. RangeAnchorMatchMode affects matching; the other
options affect overload generation.

Target frame (used for the target method being generated):

| Order | Scope         | Where you set it                                   |
| ----: | ------------- | -------------------------------------------------- |
|     1 | Target method | `[OverloadGenerationOptions]` on the target method |
|     2 | Target type   | `[OverloadGenerationOptions]` on the target type   |
|     3 | Default       | TypeOnly                                           |

Matcher frame (used for the matcher method that matched):

| Order | Scope          | Where you set it                                    |
| ----: | -------------- | --------------------------------------------------- |
|     1 | Matcher method | `[OverloadGenerationOptions]` on the matcher method |
|     2 | Matcher type   | `[OverloadGenerationOptions]` on the matcher type   |
|     3 | Default        | TypeOnly                                            |

All matcher methods are considered as candidates; there is no exact-count-only filter.

## 6) Overload generation rules

Given a window:
- Compute all omission sets (subsequences) based on OverloadSubsequenceStrategy:
  - PrefixOnly -> omit suffixes only
  - UniqueBySignature -> all unique non-empty omission subsets
- Skip any omission set that:
  - Removes any ref, out, or in parameter
  - Is redundant because omitted parameters already have defaults
- Skip entire method if:
  - Any parameter inside the optional window already has defaults
  - A params parameter exists outside the optional window

## 7) De-duplication and collisions

- Generated overloads are deduped by signature (nullability is ignored here to match C# signature rules).
- Existing overloads in the target type are never duplicated.

## 8) Accessibility rules

- OverloadVisibility can override generated method visibility:
  - Public, Internal, or Private
- Default is MatchTarget, which means:
  - public stays public
  - internal stays internal
  - protected internal / protected are emitted as internal (protected is not emitted)

## 9) Output shape

- One generated static class per namespace:
  - MethodOverloads_<Namespace>.g.cs
- Instance targets -> classic extension methods (this T source).
- Static targets -> C# 14 extension blocks:
  extension(SomeStaticType) { public static ... }
- Omitted parameters are passed as typed defaults, e.g. default(int) or default(string?).

## 10) Diagnostics

Diagnostics are reported by the analyzer (not by the generator):
- Analyzer: src/Tenekon.MethodOverloads.SourceGenerator/MethodOverloadsDiagnosticsAnalyzer.cs
- Generator: src/Tenekon.MethodOverloads.SourceGenerator/MethodOverloadsGenerator.cs

The analyzer analyzes each type on its own (symbol action) with the same parsing and planning logic as the
generator:
- Diagnostics are reported live in the IDE, not only at the end of a build.
- Each type reports only diagnostics located in its own declaration.
- Formal diagnostics on matcher methods (MOG001, MOG007–MOG011, MOG013–MOG016, MOG018, MOG019) are reported once,
  when the matcher type itself is analyzed, whether or not a target uses it. Targets skip invalid matcher attributes
  silently. Generation diagnostics (MOG003–MOG006) are reported only at targets.
- MOG020 and MOG021 are declaration checks: MOG020 is reported at every `typeof` reference to an unmarked type in
  `Matchers`; MOG021 is reported at the offending attribute in the matcher type.
- Locations are in source, so `#pragma warning disable`, `[SuppressMessage]` and per-file `.editorconfig`
  severities apply.
- MOG002 is evaluated per target: it is reported at the `typeof` reference in `Matchers` when a matcher method has
  no subsequence match in that target.

Attributes-only mode suppresses diagnostics the same way it suppresses overload generation.

## 11) Entry point & files

- Generator: src/Tenekon.MethodOverloads.SourceGenerator/MethodOverloadsGenerator.cs
- Attributes are emitted via RegisterPostInitializationOutput.

## 12) Known non-goals

See [unsupported.md](unsupported.md) for out-of-scope behavior.

## 13) GenerateOverloads shorthand

If the optional window is a single parameter, you can use the ctor shorthand:
```
[GenerateOverloads(nameof(param_2))]
```
This is equivalent to:
```
[GenerateOverloads(Begin = nameof(param_2), End = nameof(param_2))]
```

The ctor shorthand cannot be combined with Begin/End/BeginExclusive/EndExclusive. If mixed, the generator reports an error diagnostic and skips overload generation for that method.
