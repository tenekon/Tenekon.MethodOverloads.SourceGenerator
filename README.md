# Tenekon.MethodOverloads.SourceGenerator

A C# source generator that creates extension method overloads by treating a selected parameter window as optional and emitting legal, unique subsequences.

## Quickstart
1) Mark a method with `[GenerateOverloads]` or a type with `[GenerateMethodOverloads(Matchers = ...)]`.
2) (Optional) Add `[OverloadGenerationOptions(...)]` to control matching and output.
3) Build the project. Generated overloads appear in `MethodOverloads_<Namespace>.g.cs`.

## Examples

### A) GenerateOverloadsAttribute

Input:
```csharp
namespace Demo;

using Tenekon.MethodOverloads.SourceGenerator;

public sealed class Calculator
{
    [GenerateOverloads(Begin = nameof(param_2))]
    public void Add(int param_1, string? param_2, bool param_3) { }
}
```

Output:
```csharp
namespace Demo;

public static class MethodOverloads
{

    public static void Add(this Calculator source, int param_1, string? param_2) =>
        source.Add(param_1, param_2, param_3: default);

    public static void Add(this Calculator source, int param_1, bool param_3) =>
        source.Add(param_1, param_2: default, param_3);

    public static void Add(this Calculator source, int param_1) =>
        source.Add(param_1, param_2: default, param_3: default);
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
    public void Update(string param_1, int param_2) { }
}

internal static class CustomerMatcher
{
    [GenerateOverloads(nameof(param_2))]
    internal static extern void Update(string param_1, int param_2);
}
```

Output:
```csharp
namespace Demo;

public static class MethodOverloads
{
    public static void Update(this Customer source, string param_1) =>
        source.Update(param_1, param_2: default);
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
    public static void Multiply(int param_1, int param_2) { }
}

internal static class MathMatchers
{
    [GenerateOverloads(nameof(param_2))]
    internal static extern void Multiply(int param_1, int param_2);
}
```

Output:
```csharp
namespace Demo;

public static class MethodOverloads
{
    extension(MathUtils)
    {
        public static void Multiply(int param_1) =>
            MathUtils.Multiply(param_1, param_2: default);
    }
}
```

## Key behavior
- Generates overloads from a contiguous optional window of parameters.
- Omits only legal subsets (no ref/out/in removal, no empty parameter list).
- Dedupes by signature and never duplicates existing overloads.

See [docs/Generator.md](docs/Generator.md) for full behavior and rules.
