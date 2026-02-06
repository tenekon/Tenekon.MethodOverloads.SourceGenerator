using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Generation;

internal static class GenerationOptionsResolver
{
    public static GenerationOptions ResolveGenerationOptions(
        MethodModel targetMethod,
        TypeModel? targetType,
        MethodModel? matcherMethod,
        MatcherTypeModel? matcherType)
    {
        var options = CreateDefaultOptions();

        var targetHasOptions = targetMethod.Options.HasAny
            || (targetType is not null && targetType.Value.Options.HasAny);

        if (targetHasOptions)
        {
            if (targetType is not null && targetType.Value.Options.HasAny)
                ApplyOptions(ref options, targetType.Value.Options);

            if (targetMethod.Options.HasAny) ApplyOptions(ref options, targetMethod.Options);

            return options;
        }

        if (matcherType is not null && matcherType.Value.Options.HasAny)
            ApplyOptions(ref options, matcherType.Value.Options);

        if (matcherMethod is not null && matcherMethod.Value.Options.HasAny)
            ApplyOptions(ref options, matcherMethod.Value.Options);

        return options;
    }

    private static GenerationOptions CreateDefaultOptions()
    {
        return new GenerationOptions
        {
            RangeAnchorMatchMode = RangeAnchorMatchMode.TypeOnly,
            SubsequenceStrategy = OverloadSubsequenceStrategy.UniqueBySignature,
            OverloadVisibility = OverloadVisibility.MatchTarget
        };
    }

    private static void ApplyOptions(ref GenerationOptions options, OverloadOptionsModel optionsSyntax)
    {
        if (optionsSyntax.RangeAnchorMatchMode.HasValue)
            options.RangeAnchorMatchMode = optionsSyntax.RangeAnchorMatchMode.Value;

        if (optionsSyntax.SubsequenceStrategy.HasValue)
            options.SubsequenceStrategy = optionsSyntax.SubsequenceStrategy.Value;

        if (optionsSyntax.OverloadVisibility.HasValue)
            options.OverloadVisibility = optionsSyntax.OverloadVisibility.Value;
    }
}