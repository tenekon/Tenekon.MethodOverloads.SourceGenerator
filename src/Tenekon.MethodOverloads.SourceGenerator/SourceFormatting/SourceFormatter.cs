using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Model;

namespace Tenekon.MethodOverloads.SourceGenerator.SourceFormatting;

internal static partial class SourceFormatter
{
    public static void GenerateSourceFiles(SourceProductionContext context, GeneratorModel model)
    {
        foreach (var diagnostic in model.Diagnostics.Items)
        {
            context.ReportDiagnostic(diagnostic.CreateDiagnostic());
        }

        var engine = new GenerationEngine(model);
        var result = engine.Generate();

        foreach (var diagnostic in result.Diagnostics.Items)
        {
            context.ReportDiagnostic(diagnostic.CreateDiagnostic());
        }

        EmitMethods(context, result);
    }
}
