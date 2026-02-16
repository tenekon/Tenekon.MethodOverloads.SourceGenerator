using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal static class GeneratorDiagnostics
{
    public static readonly DiagnosticDescriptor InvalidWindowAnchor = new(
        "MOG001",
        "Invalid overload window anchor",
        "Overload window anchor '{0}' refers to a missing parameter '{1}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MatcherHasNoSubsequenceMatch = new(
        "MOG002",
        "Matcher has no subsequence match",
        "Matcher '{0}' has no subsequence match for any target method",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DefaultsInWindow = new(
        "MOG003",
        "Defaults inside overload window",
        "Method '{0}' contains default or optional parameters inside the overload window",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ParamsOutsideWindow = new(
        "MOG004",
        "Params outside overload window",
        "Method '{0}' has a params parameter outside the overload window",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RefOutInOmitted = new(
        "MOG005",
        "Ref/out/in parameters cannot be omitted",
        "Method '{0}' has ref/out/in parameters that would be omitted by an overload",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateSignatureSkipped = new(
        "MOG006",
        "Duplicate overload signature",
        "Method '{0}' would generate a duplicate overload signature and was skipped",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConflictingWindowAnchors = new(
        "MOG007",
        "Conflicting overload window anchors",
        "GenerateOverloadsAttribute(string beginEnd) cannot be combined with Begin/End/BeginExclusive/EndExclusive on method '{0}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RedundantBeginEndAnchors = new(
        "MOG008",
        "Begin and End anchors are identical",
        "GenerateOverloads Begin and End are identical on method '{0}'. Prefer GenerateOverloadsAttribute(string beginEnd).",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BeginAndBeginExclusiveConflict = new(
        "MOG009",
        "Conflicting begin anchors",
        "GenerateOverloads cannot specify both Begin and BeginExclusive on method '{0}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EndAndEndExclusiveConflict = new(
        "MOG010",
        "Conflicting end anchors",
        "GenerateOverloads cannot specify both End and EndExclusive on method '{0}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ParameterlessTargetMethod = new(
        "MOG011",
        "Parameterless method cannot generate overloads",
        "Method '{0}' has no parameters; overload generation requires at least one parameter",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor WindowAndMatchersConflict = new(
        "MOG012",
        "GenerateOverloads cannot combine window anchors and matchers",
        "GenerateOverloads on method '{0}' cannot specify window anchors and Matchers at the same time",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidBucketType = new(
        "MOG013",
        "Invalid bucket type",
        "Bucket type '{0}' is invalid: {1}",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidSupplyParameterType = new(
        "MOG014",
        "Invalid SupplyParameterType",
        "SupplyParameterType on method '{0}' is invalid: {1}",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SupplyParameterTypeMissingTypeParameter = new(
        "MOG015",
        "SupplyParameterType references missing type parameter",
        "SupplyParameterType refers to missing type parameter '{0}' on method '{1}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SupplyParameterTypeConflicting = new(
        "MOG016",
        "Conflicting SupplyParameterType mappings",
        "SupplyParameterType specifies multiple types for '{0}' on method '{1}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MatchersAndExcludeAnyConflict = new(
        "MOG017",
        "GenerateOverloads cannot combine Matchers and ExcludeAny",
        "GenerateOverloads cannot combine Matchers and ExcludeAny on method '{0}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidExcludeAnyParameter = new(
        "MOG018",
        "ExcludeAny refers to missing window parameter",
        "GenerateOverloads ExcludeAny refers to '{0}', which is not a parameter within the resolved window on method '{1}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidExcludeAnyEntry = new(
        "MOG019",
        "ExcludeAny contains invalid entries",
        "GenerateOverloads ExcludeAny contains invalid entries on method '{0}'",
        "MethodOverloadsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
