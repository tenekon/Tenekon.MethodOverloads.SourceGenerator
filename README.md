# Tenekon.MethodOverloads.SourceGenerator

[![Build](https://github.com/tenekon/Tenekon.MethodOverloads.SourceGenerator/actions/workflows/coverage.yml/badge.svg?branch=main)](https://github.com/tenekon/Tenekon.MethodOverloads.SourceGenerator/actions/workflows/coverage.yml)
[![NuGet](https://img.shields.io/nuget/v/Tenekon.MethodOverloads.SourceGenerator.svg)](https://www.nuget.org/packages/Tenekon.MethodOverloads.SourceGenerator)
[![Codecov](https://codecov.io/gh/tenekon/Tenekon.MethodOverloads.SourceGenerator/branch/main/graph/badge.svg)](https://codecov.io/gh/tenekon/Tenekon.MethodOverloads.SourceGenerator)
[![License](https://img.shields.io/github/license/tenekon/Tenekon.MethodOverloads.SourceGenerator.svg)](LICENSE)

A C# source generator that creates combinatorial extension method overloads by treating a selected parameter window as optional and emitting legal, unique subsequences.

## Quickstart

1) Mark a method with `[GenerateOverloads]` or a type with `[GenerateMethodOverloads(Matchers = ...)]`.
2) (Optional) Add `[OverloadGenerationOptions(...)]` to control matching and output.
3) Build the project. Generated overloads appear in `MethodOverloads_<Namespace>.g.cs`.

## MSBuild options

Emit attributes only (skip overload generation):

```xml
<PropertyGroup>
  <TenekonMethodOverloadsSourceGeneratorAttributesOnly>true</TenekonMethodOverloadsSourceGeneratorAttributesOnly>
</PropertyGroup>
```

## Examples

### A) GenerateOverloadsAttribute

Input:
```csharp
namespace Demo;

using Tenekon.MethodOverloads.SourceGenerator;

public sealed class Calculator
{
    [GenerateOverloads(Begin = nameof(unit))]
    public void Add(int value, string? unit, bool useRounding) { }
}
```

Output:
```csharp
namespace Demo;

public static class MethodOverloads
{
    public static void Add(this Calculator source, int value) =>
        source.Add(value, unit: default(string?), useRounding: default(bool));

    public static void Add(this Calculator source, int value, string? unit) =>
        source.Add(value, unit, useRounding: default(bool));

    public static void Add(this Calculator source, int value, bool useRounding) =>
        source.Add(value, unit: default(string?), useRounding);
}
```

### B) Method-level matcher

Input:
```csharp
namespace Demo;

using Tenekon.MethodOverloads.SourceGenerator;

public sealed class Customer
{
    [GenerateOverloads(Matchers = [typeof(CustomerMatcher)])]
    public void Update(string name, int loyaltyLevel, bool isActive) { }
}

internal interface CustomerMatcher
{
    [GenerateOverloads(nameof(paramB))]
    void Update(int paramA, bool paramB);
}
```

Output:
```csharp
namespace Demo;

public static class MethodOverloads
{
    public static void Update(this Customer source, string name, int loyaltyLevel) =>
        source.Update(name, loyaltyLevel, isActive: default(bool));
}
```

### C) Type-level matcher (static target)

Input:
```csharp
namespace Demo;

using Tenekon.MethodOverloads.SourceGenerator;

[GenerateMethodOverloads(Matchers = [typeof(MathMatchers)])]
public static class MathUtils
{
    public static void Multiply(int left, int right, bool checkedOverflow) { }
}

internal interface MathMatchers
{
    [GenerateOverloads(nameof(paramB))]
    void Multiply(int paramA, bool paramB);
}
```

Output:
```csharp
namespace Demo;

public static class MethodOverloads
{
    extension(MathUtils)
    {
        public static void Multiply(int left, int right) =>
            MathUtils.Multiply(left, right, checkedOverflow: default(bool));
    }
}
```

## Key behavior
- Generates overloads from a contiguous optional window of parameters.
- Omits only legal subsets (no ref/out/in removal).
- Dedupes by signature and never duplicates existing overloads.

See [docs/generator.md](docs/generator.md) for full behavior and rules.
