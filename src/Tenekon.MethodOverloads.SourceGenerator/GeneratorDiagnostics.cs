using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal static class GeneratorDiagnostics
{
    public static readonly DiagnosticDescriptor InvalidWindowAnchor = new(
        id: "MOG001",
        title: "Invalid overload window anchor",
        messageFormat: "Overload window anchor '{0}' refers to a missing parameter '{1}'",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MatcherHasNoSubsequenceMatch = new(
        id: "MOG002",
        title: "Matcher has no subsequence match",
        messageFormat: "Matcher '{0}' has no subsequence match for any target method",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DefaultsInWindow = new(
        id: "MOG003",
        title: "Defaults inside overload window",
        messageFormat: "Method '{0}' contains default or optional parameters inside the overload window",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ParamsOutsideWindow = new(
        id: "MOG004",
        title: "Params outside overload window",
        messageFormat: "Method '{0}' has a params parameter outside the overload window",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RefOutInOmitted = new(
        id: "MOG005",
        title: "Ref/out/in parameters cannot be omitted",
        messageFormat: "Method '{0}' has ref/out/in parameters that would be omitted by an overload",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateSignatureSkipped = new(
        id: "MOG006",
        title: "Duplicate overload signature",
        messageFormat: "Method '{0}' would generate a duplicate overload signature and was skipped",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConflictingWindowAnchors = new(
        id: "MOG007",
        title: "Conflicting overload window anchors",
        messageFormat: "GenerateOverloadsAttribute(string beginEnd) cannot be combined with Begin/End/BeginExclusive/EndExclusive on method '{0}'",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RedundantBeginEndAnchors = new(
        id: "MOG008",
        title: "Begin and End anchors are identical",
        messageFormat: "GenerateOverloads Begin and End are identical on method '{0}'. Prefer GenerateOverloadsAttribute(string beginEnd).",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BeginAndBeginExclusiveConflict = new(
        id: "MOG009",
        title: "Conflicting begin anchors",
        messageFormat: "GenerateOverloads cannot specify both Begin and BeginExclusive on method '{0}'",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndAndEndExclusiveConflict = new(
        id: "MOG010",
        title: "Conflicting end anchors",
        messageFormat: "GenerateOverloads cannot specify both End and EndExclusive on method '{0}'",
        category: "MethodOverloadsGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
