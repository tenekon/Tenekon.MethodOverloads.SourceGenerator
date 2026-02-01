using System.Diagnostics.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.AcceptanceCriterias;

/// <summary>
/// Diagnostic coverage cases.
/// </summary>
public static class Class_21
{
    // 1) Invalid window anchors (Begin/End not found).
    [SuppressMessage("MethodOverloadsGenerator", "MOG001")]
    [GenerateOverloads(Begin = "missing", End = "also_missing")]
    public static void InvalidAnchors(int value) { }

    // 2) Matcher present but no subsequence matches target.
    [GenerateOverloads(Begin = nameof(m), End = nameof(m))]
    public static void MatcherSignature(string m) { }

    // 3) Defaults/optional inside window.
    [SuppressMessage("MethodOverloadsGenerator", "MOG003")]
    [GenerateOverloads(Begin = nameof(optional), End = nameof(optional))]
    public static void DefaultInWindow(int required, int optional = 42) { }

    // 4) Params outside window.
    [SuppressMessage("MethodOverloadsGenerator", "MOG004")]
    [GenerateOverloads(Begin = nameof(required), End = nameof(required))]
    public static void ParamsOutsideWindow(int required, params int[] values) { }

    // 5) Ref/out/in omitted.
    [SuppressMessage("MethodOverloadsGenerator", "MOG005")]
    [GenerateOverloads(Begin = nameof(refValue), End = nameof(refValue))]
    public static void RefOmitted(int required, ref int refValue) { }

    // 6) Duplicate signature skipped.
    [SuppressMessage("MethodOverloadsGenerator", "MOG006")]
    [GenerateOverloads(Begin = nameof(optional), End = nameof(optional))]
    public static void Duplicate(int value, string optional) { }

    public static void Duplicate(int value) { }
}

internal static class Class_21_Matcher
{
    [SuppressMessage("MethodOverloadsGenerator", "MOG002")]
    [GenerateOverloads(Begin = nameof(m), End = nameof(m))]
    internal static void MatcherSignature(int m) { }
}

[GenerateMethodOverloads(Matchers = [typeof(Class_21_Matcher)])]
public abstract class Class_21_Matched
{
    public abstract void MatcherSignature(string m);
}

[SuppressMessage("ReSharper", "PreferConcreteValueOverDefault")]
public static class Class_21_AcceptanceCriterias
{
    // No overloads expected; each case triggers a diagnostic and is skipped.
}
