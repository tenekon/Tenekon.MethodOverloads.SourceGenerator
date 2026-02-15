using System.Text;
using Microsoft.CodeAnalysis;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.SourceFormatting;

internal sealed class GeneratedMethod(
    MethodModel method,
    ParameterModel[] keptParameters,
    ParameterModel[] omittedParameters,
    OverloadVisibility overloadVisibility,
    IReadOnlyCollection<MatcherMethodReference>? matchedMatcherMethods)
{
    private readonly ParameterModel[] _omittedParameters = omittedParameters;
    private readonly MatcherMethodReference[] _matchedMatcherMethods = NormalizeMatchedMatcherMethods(matchedMatcherMethods);

    public string Namespace { get; } = method.ContainingNamespace;
    public MethodModel Method => method;

    public string Render()
    {
        return method.IsStatic && !method.IsExtensionMethod
            ? RenderStaticExtensionBlock()
            : RenderClassicExtensionMethod();
    }

    private string RenderClassicExtensionMethod()
    {
        var builder = new StringBuilder();
        var accessibility = RenderAccessibility();
        var returnType = method.ReturnTypeDisplay;
        var typeParams = RenderTypeParameters(method);
        var constraints = RenderTypeParameterConstraints(method);
        var isExtensionMethod = method.IsExtensionMethod;
        var receiverParameter = isExtensionMethod ? method.Parameters.Items.FirstOrDefault() : (ParameterModel?)null;
        var receiverType = receiverParameter?.TypeDisplay ?? method.ContainingTypeDisplay;
        var receiverName = receiverParameter?.Name ?? "source";
        var invocationReceiver = isExtensionMethod ? method.ContainingTypeDisplay : receiverName;

        builder.Append("    ")
            .Append(accessibility)
            .Append(" static ")
            .Append(returnType)
            .Append(" ")
            .Append(method.Name)
            .Append(typeParams)
            .Append("(");

        builder.Append("this ").Append(receiverType).Append(" ").Append(receiverName);

        foreach (var parameter in keptParameters)
        {
            if (receiverParameter.HasValue && string.Equals(
                    parameter.Name,
                    receiverParameter.Value.Name,
                    StringComparison.Ordinal)) continue;

            builder.Append(", ").Append(RenderParameter(parameter));
        }

        builder.Append(")");

        if (!string.IsNullOrWhiteSpace(constraints)) builder.Append(" ").Append(constraints);

        builder.Append(" => ").Append(RenderInvocation(invocationReceiver)).Append(";");

        return builder.ToString();
    }

    private string RenderStaticExtensionBlock()
    {
        var builder = new StringBuilder();
        var accessibility = RenderAccessibility();
        var returnType = method.ReturnTypeDisplay;
        var typeParams = RenderTypeParameters(method);
        var constraints = RenderTypeParameterConstraints(method);
        var receiverType = method.ContainingTypeDisplay;

        builder.AppendLine("    extension(" + receiverType + ")");
        builder.AppendLine("    {");
        builder.Append("        ")
            .Append(accessibility)
            .Append(" static ")
            .Append(returnType)
            .Append(" ")
            .Append(method.Name)
            .Append(typeParams)
            .Append("(");

        var first = true;
        foreach (var parameter in keptParameters)
        {
            if (!first) builder.Append(", ");

            builder.Append(RenderParameter(parameter));
            first = false;
        }

        builder.Append(")");

        if (!string.IsNullOrWhiteSpace(constraints)) builder.Append(" ").Append(constraints);

        builder.Append(" => ").Append(RenderInvocation(receiverType)).Append(";");
        builder.AppendLine();
        builder.Append("    }");

        return builder.ToString();
    }

    internal static MatcherMethodReference[] NormalizeMatchedMatcherMethods(
        IReadOnlyCollection<MatcherMethodReference>? matchedMatcherMethods)
    {
        if (matchedMatcherMethods is null || matchedMatcherMethods.Count == 0) return [];

        return matchedMatcherMethods.Distinct()
            .OrderBy(method => method.ContainingTypeDisplay, StringComparer.Ordinal)
            .ThenBy(method => method.MethodName, StringComparer.Ordinal)
            .ThenBy(method => method.ParameterCount)
            .ToArray();
    }

    private string RenderInvocation(string receiver)
    {
        var builder = new StringBuilder();
        builder.Append(receiver).Append(".").Append(method.Name).Append(RenderTypeArguments(method)).Append("(");

        var first = true;
        foreach (var parameter in method.Parameters.Items)
        {
            if (!first) builder.Append(", ");

            builder.Append(RenderArgument(parameter));
            first = false;
        }

        builder.Append(")");
        return builder.ToString();
    }

    private string RenderArgument(ParameterModel parameter)
    {
        if (keptParameters.Any(p => string.Equals(p.Name, parameter.Name, StringComparison.Ordinal)))
        {
            var name = parameter.Name;
            return parameter.RefKind switch
            {
                RefKind.Ref => "ref " + name,
                RefKind.Out => "out " + name,
                RefKind.In => "in " + name,
                _ => name
            };
        }

        if (parameter.IsParams && parameter.TypeDisplay.EndsWith("[]", StringComparison.Ordinal))
        {
            var elementType = parameter.TypeDisplay.Substring(startIndex: 0, parameter.TypeDisplay.Length - 2);
            return "global::System.Array.Empty<" + elementType + ">()";
        }

        return "default(" + parameter.TypeDisplay + ")";
    }

    private string RenderAccessibility()
    {
        var accessibility = overloadVisibility switch
        {
            OverloadVisibility.Public => Accessibility.Public,
            OverloadVisibility.Internal => Accessibility.Internal,
            OverloadVisibility.Private => Accessibility.Private,
            _ => method.DeclaredAccessibility
        };

        if (overloadVisibility == OverloadVisibility.MatchTarget
            && TryGetOverloadVisibilityOverride(method, out var overrideVisibility))
            accessibility = overrideVisibility switch
            {
                OverloadVisibility.Public => Accessibility.Public,
                OverloadVisibility.Internal => Accessibility.Internal,
                OverloadVisibility.Private => Accessibility.Private,
                _ => accessibility
            };

        if (overloadVisibility == OverloadVisibility.MatchTarget && (accessibility == Accessibility.ProtectedOrInternal
                || accessibility == Accessibility.ProtectedAndInternal))
            accessibility = Accessibility.Internal;

        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            _ => "public"
        };
    }

    private static string RenderParameter(ParameterModel parameter)
    {
        var builder = new StringBuilder();
        if (parameter.IsParams) builder.Append("params ");

        builder.Append(
            parameter.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty
            });

        builder.Append(parameter.TypeDisplay);
        builder.Append(" ").Append(parameter.Name);
        return builder.ToString();
    }

    private static string RenderTypeParameters(MethodModel method)
    {
        if (method.TypeParameterCount == 0 && method.ContainingTypeParameterNames.Items.Length == 0)
            return string.Empty;

        var names = new List<string>();
        names.AddRange(method.ContainingTypeParameterNames.Items);
        names.AddRange(method.TypeParameterNames.Items);
        return "<" + string.Join(", ", names) + ">";
    }

    private static string RenderTypeArguments(MethodModel method)
    {
        if (method.InvocationTypeArguments.Items.Length == 0) return string.Empty;

        return "<" + string.Join(", ", method.InvocationTypeArguments.Items) + ">";
    }

    private static string RenderTypeParameterConstraints(MethodModel method)
    {
        if (string.IsNullOrWhiteSpace(method.ContainingTypeParameterConstraints))
            return method.TypeParameterConstraints;

        if (string.IsNullOrWhiteSpace(method.TypeParameterConstraints))
            return method.ContainingTypeParameterConstraints;

        return method.ContainingTypeParameterConstraints + " " + method.TypeParameterConstraints;
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
