using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Generation;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.SourceFormatting;

internal static partial class SourceFormatter
{
    public static void GenerateSourceFiles(SourceProductionContext context, GeneratorModel model)
    {
        var builder = new OverloadPlanBuilder(model);
        var plan = builder.Build();

        EmitMethods(context, plan);
    }
}