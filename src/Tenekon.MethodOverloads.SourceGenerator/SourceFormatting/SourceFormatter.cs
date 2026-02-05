using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Model;

namespace Tenekon.MethodOverloads.SourceGenerator.SourceFormatting;

internal static partial class SourceFormatter
{
    public static void GenerateSourceFiles(SourceProductionContext context, GeneratorModel model)
    {
        var engine = new GenerationEngine(model);
        var result = engine.Generate();

        EmitMethods(context, result);
    }
}