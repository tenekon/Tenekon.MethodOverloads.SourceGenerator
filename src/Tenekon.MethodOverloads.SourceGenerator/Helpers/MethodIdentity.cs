using System.Text;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Helpers;

internal static class MethodIdentity
{
    public static string BuildMethodIdentityKey(MethodModel method)
    {
        var builder = new StringBuilder();
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

    public static string BuildSignatureKey(string name, int arity, IEnumerable<ParameterModel> parameters)
    {
        var builder = new StringBuilder();
        builder.Append(name);
        builder.Append("|");
        builder.Append(arity);

        foreach (var parameter in parameters)
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

    public static string BuildSignatureKey(string name, int arity, IEnumerable<ParameterSignatureModel> parameters)
    {
        var builder = new StringBuilder();
        builder.Append(name);
        builder.Append("|");
        builder.Append(arity);

        foreach (var parameter in parameters)
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
}