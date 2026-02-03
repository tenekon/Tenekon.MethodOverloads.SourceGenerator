namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    private string BuildMethodIdentityKey(MethodModel method)
    {
        var builder = new System.Text.StringBuilder();
        builder.Append(method.ContainingTypeDisplay);
        builder.Append("|");
        builder.Append(method.Name);
        builder.Append("|");
        builder.Append(method.TypeParameterCount);

        foreach (var parameter in method.Parameters.Items)
        {
            builder.Append("|");
            builder.Append(parameter.SignatureTypeDisplay);
            builder.Append(":");
            builder.Append(parameter.RefKind);
            builder.Append(":");
            builder.Append(parameter.IsParams ? "params" : "noparams");
        }

        return builder.ToString();
    }

    private bool TryGetOverloadOptions(MethodModel method, out OverloadOptionsModel options)
    {
        options = method.Options;
        return options.HasAny;
    }

    private static bool TryGetOverloadVisibilityOverride(MethodModel method, out OverloadVisibility visibility)
    {
        visibility = default;
        if (method.OptionsFromAttribute && method.Options.OverloadVisibility.HasValue)
        {
            visibility = method.Options.OverloadVisibility.Value;
            return true;
        }

        return false;
    }
}
