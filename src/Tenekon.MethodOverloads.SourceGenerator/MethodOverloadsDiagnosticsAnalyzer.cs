using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Tenekon.MethodOverloads.SourceGenerator.Generation;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;
using Tenekon.MethodOverloads.SourceGenerator.Parsing;
using Tenekon.MethodOverloads.SourceGenerator.Parsing.Inputs;

namespace Tenekon.MethodOverloads.SourceGenerator;

/// <summary>
/// Reports diagnostics for overload generation. Each type is analyzed on its own with the shared parsing and
/// generation pipeline, so diagnostics are reported live in the IDE and can be suppressed in source.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodOverloadsDiagnosticsAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<DiagnosticDescriptor> Supported =
    [
        GeneratorDiagnostics.InvalidWindowAnchor,
        GeneratorDiagnostics.MatcherHasNoSubsequenceMatch,
        GeneratorDiagnostics.DefaultsInWindow,
        GeneratorDiagnostics.ParamsOutsideWindow,
        GeneratorDiagnostics.RefOutInOmitted,
        GeneratorDiagnostics.DuplicateSignatureSkipped,
        GeneratorDiagnostics.ConflictingWindowAnchors,
        GeneratorDiagnostics.RedundantBeginEndAnchors,
        GeneratorDiagnostics.BeginAndBeginExclusiveConflict,
        GeneratorDiagnostics.EndAndEndExclusiveConflict,
        GeneratorDiagnostics.ParameterlessTargetMethod,
        GeneratorDiagnostics.WindowAndMatchersConflict,
        GeneratorDiagnostics.InvalidBucketType,
        GeneratorDiagnostics.InvalidSupplyParameterType,
        GeneratorDiagnostics.SupplyParameterTypeMissingTypeParameter,
        GeneratorDiagnostics.SupplyParameterTypeConflicting,
        GeneratorDiagnostics.MatchersAndExcludeAnyConflict,
        GeneratorDiagnostics.InvalidExcludeAnyParameter,
        GeneratorDiagnostics.InvalidExcludeAnyEntry,
        GeneratorDiagnostics.UnmarkedMatcherType,
        GeneratorDiagnostics.MatcherTypeAsTarget
    ];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Supported;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static startContext =>
        {
            if (IsAttributesOnly(startContext.Options.AnalyzerConfigOptionsProvider)) return;

            startContext.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol) return;

        var cancellationToken = context.CancellationToken;
        ReportMatcherRoleDiagnostics(context, typeSymbol);

        var typeTargets = ImmutableArray.CreateBuilder<TypeTargetInput>();
        var methodTargets = ImmutableArray.CreateBuilder<MethodTargetInput>();

        if (HasAttribute(typeSymbol, AttributeNames.GenerateMethodOverloadsAttribute)
            && TargetFactory.CreateTypeTargetFromSymbol(typeSymbol, cancellationToken) is { } typeTarget)
            typeTargets.Add(typeTarget);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol
                || !HasAttribute(methodSymbol, AttributeNames.GenerateOverloadsAttribute))
                continue;

            if (TargetFactory.CreateMethodTargetFromSymbol(methodSymbol, cancellationToken) is { } methodTarget)
                methodTargets.Add(methodTarget);
        }

        if (typeTargets.Count == 0 && methodTargets.Count == 0) return;

        // The model only knows this type, so every diagnostic is evaluated for this type alone.
        var model = Parser.Parse(typeTargets.ToImmutable(), methodTargets.ToImmutable(), cancellationToken);
        if (model is null) return;

        var plan = new OverloadPlanBuilder(model).Build();
        var reported = new HashSet<EquatableDiagnostic>();

        foreach (var diagnostic in model.Diagnostics.Items.Concat(plan.Diagnostics.Items))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!reported.Add(diagnostic)) continue;

            if (diagnostic.MatcherTypeDisplay is { } matcherTypeDisplay)
            {
                var referenceLocation = FindMatcherReference(typeSymbol, matcherTypeDisplay, cancellationToken)
                    ?? typeSymbol.Locations.FirstOrDefault();
                context.ReportDiagnostic(diagnostic.CreateDiagnostic(referenceLocation, typeSymbol.Name));
                continue;
            }

            // Diagnostics located in other types (e.g. at a matcher) are reported by the analysis of that type.
            if (diagnostic.Location is not { } sourceLocation) continue;

            var location = ResolveOwnLocation(typeSymbol, sourceLocation, cancellationToken);
            if (location is null) continue;

            context.ReportDiagnostic(diagnostic.CreateDiagnostic(location));
        }
    }

    /// <summary>
    /// Reports how this type uses the matcher role. These are declaration checks that do not need the overload plan:
    /// a matcher type must not be a target (MOG021), and every type listed in Matchers must be marked (MOG020).
    /// </summary>
    private static void ReportMatcherRoleDiagnostics(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol)
    {
        var isMatcherType = Parser.IsMatcherType(typeSymbol);

        foreach (var attribute in RoslynHelpers.GetAttributes(typeSymbol, "GenerateMethodOverloadsAttribute"))
            if (isMatcherType)
                ReportMatcherTypeAsTarget(context, typeSymbol, attribute);
            else
                ReportUnmarkedMatcherTypes(context, typeSymbol, attribute);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol) continue;

            foreach (var attribute in RoslynHelpers.GetAttributes(methodSymbol, "GenerateOverloadsAttribute"))
                if (!isMatcherType)
                    ReportUnmarkedMatcherTypes(context, typeSymbol, attribute);
                else if (GetMatchersArgument(attribute) is not null)
                    ReportMatcherTypeAsTarget(context, typeSymbol, attribute);
        }
    }

    private static void ReportMatcherTypeAsTarget(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        AttributeData attribute)
    {
        if (attribute.ApplicationSyntaxReference is not { } reference) return;

        context.ReportDiagnostic(
            Diagnostic.Create(
                GeneratorDiagnostics.MatcherTypeAsTarget,
                reference.GetSyntax(context.CancellationToken).GetLocation(),
                typeSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
    }

    private static void ReportUnmarkedMatcherTypes(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        AttributeData attribute)
    {
        if (GetMatchersArgument(attribute) is not { } matchers
            || attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) is not AttributeSyntax syntax)
            return;

        for (var index = 0; index < matchers.Length; index++)
        {
            if (matchers[index].Value is not INamedTypeSymbol matcherType || Parser.IsMatcherType(matcherType))
                continue;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    GeneratorDiagnostics.UnmarkedMatcherType,
                    (GetMatchersElement(syntax, index) ?? syntax).GetLocation(),
                    matcherType.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    typeSymbol.Name));
        }
    }

    private static ImmutableArray<TypedConstant>? GetMatchersArgument(AttributeData attribute)
    {
        foreach (var named in attribute.NamedArguments)
            if (string.Equals(named.Key, "Matchers", StringComparison.Ordinal))
                return named.Value.Kind == TypedConstantKind.Array && !named.Value.IsNull
                    ? named.Value.Values
                    : ImmutableArray<TypedConstant>.Empty;

        return null;
    }

    /// <summary>
    /// Maps a stored source location to an in-source location when it lies directly in this type, so that
    /// #pragma and [SuppressMessage] apply. Returns null for locations owned by other types.
    /// </summary>
    private static Location? ResolveOwnLocation(
        INamedTypeSymbol typeSymbol,
        SourceLocationModel sourceLocation,
        CancellationToken cancellationToken)
    {
        var filePath = sourceLocation.SourceTreeFilePath ?? string.Empty;
        var span = sourceLocation.SourceSpan;
        Location? resolved = null;
        var matches = 0;

        foreach (var reference in typeSymbol.DeclaringSyntaxReferences)
        {
            var tree = reference.SyntaxTree;
            if (!string.Equals(tree.FilePath, filePath, StringComparison.Ordinal) || !reference.Span.Contains(span))
                continue;

            var declaration = reference.GetSyntax(cancellationToken);
            var node = tree.GetRoot(cancellationToken).FindNode(span, getInnermostNodeForTie: true);
            if (node.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>() != declaration) continue;

            resolved = Location.Create(tree, span);
            matches++;
        }

        // Several declarations share the path (e.g. in-memory trees without a path), so the tree is ambiguous.
        return matches > 1 ? sourceLocation.ToLocation() : resolved;
    }

    private static Location? FindMatcherReference(
        INamedTypeSymbol typeSymbol,
        string matcherTypeDisplay,
        CancellationToken cancellationToken)
    {
        var location = FindMatcherReference(
            RoslynHelpers.GetAttributes(typeSymbol, "GenerateMethodOverloadsAttribute"),
            matcherTypeDisplay,
            cancellationToken);
        if (location is not null) return location;

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol) continue;

            location = FindMatcherReference(
                RoslynHelpers.GetAttributes(methodSymbol, "GenerateOverloadsAttribute"),
                matcherTypeDisplay,
                cancellationToken);
            if (location is not null) return location;
        }

        return null;
    }

    private static Location? FindMatcherReference(
        ImmutableArray<AttributeData> attributes,
        string matcherTypeDisplay,
        CancellationToken cancellationToken)
    {
        foreach (var attribute in attributes)
        foreach (var named in attribute.NamedArguments)
        {
            if (!string.Equals(named.Key, "Matchers", StringComparison.Ordinal)
                || named.Value.Kind != TypedConstantKind.Array)
                continue;

            var matchers = named.Value.Values;
            for (var index = 0; index < matchers.Length; index++)
            {
                if (matchers[index].Value is not INamedTypeSymbol matcherType
                    || !string.Equals(
                        Parser.GetMatcherTypeDisplay(matcherType),
                        matcherTypeDisplay,
                        StringComparison.Ordinal))
                    continue;

                if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is not AttributeSyntax syntax)
                    continue;

                return (GetMatchersElement(syntax, index) ?? syntax).GetLocation();
            }
        }

        return null;
    }

    private static SyntaxNode? GetMatchersElement(AttributeSyntax attribute, int index)
    {
        var argument = attribute.ArgumentList?.Arguments.FirstOrDefault(argument => string.Equals(
            argument.NameEquals?.Name.Identifier.ValueText,
            "Matchers",
            StringComparison.Ordinal));
        if (argument is null) return null;

        IReadOnlyList<SyntaxNode>? elements = argument.Expression switch
        {
            CollectionExpressionSyntax collection when collection.Elements.All(e => e is ExpressionElementSyntax) =>
                collection.Elements.Select(e => (SyntaxNode)((ExpressionElementSyntax)e).Expression).ToList(),
            ArrayCreationExpressionSyntax { Initializer: { } initializer } => initializer.Expressions,
            ImplicitArrayCreationExpressionSyntax implicitArray => implicitArray.Initializer.Expressions,
            _ => null
        };

        return elements is not null && index < elements.Count ? elements[index] : argument;
    }

    private static bool HasAttribute(ISymbol symbol, string expectedFullName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass is not { } attributeClass) continue;

            var display = attributeClass.ToDisplayString(RoslynHelpers.TypeDisplayFormat);
            if (display.StartsWith("global::", StringComparison.Ordinal))
                display = display.Substring("global::".Length);

            if (string.Equals(display, expectedFullName, StringComparison.Ordinal)) return true;
        }

        return false;
    }

    private static bool IsAttributesOnly(AnalyzerConfigOptionsProvider optionsProvider)
    {
        if (optionsProvider.GlobalOptions.TryGetValue(
                "build_property.TenekonMethodOverloadsSourceGeneratorAttributesOnly",
                out var raw))
            return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "1", StringComparison.Ordinal);

        return false;
    }
}
