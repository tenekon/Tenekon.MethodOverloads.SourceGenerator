using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal sealed record GeneratorModel(
    EquatableArray<TypeModel> Types,
    EquatableArray<TypeTargetModel> TypeTargets,
    EquatableArray<MethodTargetModel> MethodTargets,
    EquatableArray<MatcherTypeModel> MatcherTypes,
    EquatableArray<EquatableDiagnostic> Diagnostics);

internal readonly record struct EquatableDiagnostic(
    DiagnosticDescriptor Descriptor,
    SourceLocationModel? Location,
    EquatableArray<string> MessageArgs)
{
    public Diagnostic CreateDiagnostic()
    {
        var args = MessageArgs.Items.Length == 0 ? [] : MessageArgs.Items.Cast<object?>().ToArray();
        return Diagnostic.Create(Descriptor, Location?.ToLocation(), args);
    }
}