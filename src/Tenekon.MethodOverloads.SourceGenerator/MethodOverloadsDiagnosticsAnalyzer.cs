using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Tenekon.MethodOverloads.SourceGenerator.Generation;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Parsing;
using Tenekon.MethodOverloads.SourceGenerator.Parsing.Inputs;

namespace Tenekon.MethodOverloads.SourceGenerator;

/// <summary>
/// Reports diagnostics for overload generation by analyzing attributed symbols and delegating to the
/// shared parsing and generation pipeline.
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
        GeneratorDiagnostics.InvalidExcludeAnyEntry
    ];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Supported;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            if (IsAttributesOnly(startContext.Options.AnalyzerConfigOptionsProvider)) return;

            var typeTargets = new ConcurrentBag<TypeTargetInput>();
            var methodTargets = new ConcurrentBag<MethodTargetInput>();
            
            startContext.RegisterOperationAction(
                operationContext => AnalyzeAttributeOperation(operationContext, typeTargets, methodTargets),
                OperationKind.Attribute);

            startContext.RegisterCompilationEndAction(endContext =>
            {
                if (typeTargets.IsEmpty && methodTargets.IsEmpty) return;

                var model = Parser.Parse([.. typeTargets], [.. methodTargets], endContext.CancellationToken);
                if (model is null) return;

                foreach (var diagnostic in model.Diagnostics.Items)
                    endContext.ReportDiagnostic(diagnostic.CreateDiagnostic());

                var builder = new OverloadPlanBuilder(model);
                var result = builder.Build();

                foreach (var diagnostic in result.Diagnostics.Items)
                    endContext.ReportDiagnostic(diagnostic.CreateDiagnostic());
            });

            return;

            void AnalyzeAttributeOperation(
                OperationAnalysisContext operationContext,
                ConcurrentBag<TypeTargetInput> collectedTypeTargets,
                ConcurrentBag<MethodTargetInput> collectedMethodTargets)
            {
                if (operationContext.Operation is not IAttributeOperation attributeOperation) return;

                var attributeClass = GetAttributeClass(attributeOperation);
                if (attributeClass is null) return;

                if (IsTargetAttribute(attributeClass, AttributeNames.GenerateOverloadsAttribute))
                {
                    if (operationContext.ContainingSymbol is not IMethodSymbol methodSymbol) return;

                    var target = TargetFactory.CreateMethodTargetFromSymbol(
                        methodSymbol,
                        operationContext.CancellationToken);
                    if (target.HasValue) collectedMethodTargets.Add(target.Value);

                    return;
                }

                if (IsTargetAttribute(attributeClass, AttributeNames.GenerateMethodOverloadsAttribute))
                {
                    if (operationContext.ContainingSymbol is not INamedTypeSymbol typeSymbol) return;

                    var target = TargetFactory.CreateTypeTargetFromSymbol(
                        typeSymbol,
                        operationContext.CancellationToken);
                    if (target.HasValue) collectedTypeTargets.Add(target.Value);
                }

                return;

                static bool IsTargetAttribute(INamedTypeSymbol attributeClass, string expectedFullName)
                {
                    var display = attributeClass.ToDisplayString(RoslynHelpers.TypeDisplayFormat);
                    if (display.StartsWith("global::", StringComparison.Ordinal))
                        display = display.Substring("global::".Length);

                    return string.Equals(display, expectedFullName, StringComparison.Ordinal);
                }

                static INamedTypeSymbol? GetAttributeClass(IAttributeOperation attributeOperation)
                {
                    var operation = attributeOperation.Operation;

                    return operation switch
                    {
                        IObjectCreationOperation creation => creation.Constructor?.ContainingType
                            ?? creation.Type as INamedTypeSymbol,
                        IInvalidOperation invalid => invalid.Type as INamedTypeSymbol,
                        _ => operation.Type as INamedTypeSymbol
                    };
                }
            }

            static bool IsAttributesOnly(AnalyzerConfigOptionsProvider optionsProvider)
            {
                if (optionsProvider.GlobalOptions.TryGetValue(
                        "build_property.TenekonMethodOverloadsSourceGeneratorAttributesOnly",
                        out var raw))
                    return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(raw, "1", StringComparison.Ordinal);

                return false;
            }
        });
    }
}
