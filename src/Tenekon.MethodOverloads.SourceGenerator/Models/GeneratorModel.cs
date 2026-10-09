using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal sealed record GeneratorModel(
    EquatableArray<TypeModel> Types,
    EquatableArray<TypeTargetModel> TypeTargets,
    EquatableArray<MethodTargetModel> MethodTargets,
    EquatableArray<MatcherTypeModel> MatcherTypes,
    EquatableArray<EquatableDiagnostic> Diagnostics);

// MatcherTypeDisplay is set for diagnostics about a matcher usage: the matcher type whose typeof reference in the
// target carries the diagnostic.
internal readonly record struct EquatableDiagnostic(
    DiagnosticDescriptor Descriptor,
    SourceLocationModel? Location,
    EquatableArray<string> MessageArgs,
    string? MatcherTypeDisplay = null)
{
    public Diagnostic CreateDiagnostic(Location? location, params string[] additionalMessageArgs)
    {
        object?[] args = [.. MessageArgs.Items, .. additionalMessageArgs];
        return Diagnostic.Create(Descriptor, location, args);
    }
}
