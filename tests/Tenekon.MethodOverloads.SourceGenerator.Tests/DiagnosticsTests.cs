using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public class DiagnosticsTests
{
    [Fact]
    public Task Diagnostics_HaveNoChanges()
    {
        var ids = typeof(GeneratorDiagnostics).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .Select(f => (DiagnosticDescriptor?)f.GetValue(obj: null)!)
            .Select(descriptor => new DiagnosticDescriptorRecord
            {
                Id = descriptor.Id,
                Title = descriptor.Title.ToString(),
                MessageFormat = descriptor.MessageFormat.ToString(),
                Category = descriptor.Category,
                DefaultSeverity = descriptor.DefaultSeverity,
                IsEnabledByDefault = descriptor.IsEnabledByDefault
            })
            .OrderBy(x => x.Id)
            .ToArray();

        var idsJson = JsonSerializer.Serialize(ids);
        return VerifyJson(idsJson);
    }

    public sealed record DiagnosticDescriptorRecord
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public required string MessageFormat { get; init; }
        public required string Category { get; init; }
        public required DiagnosticSeverity DefaultSeverity { get; init; }
        public required bool IsEnabledByDefault { get; init; }
    }
}