using System.Text;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Renders a generated overload into source text.
    /// </summary>
    private sealed class GeneratedMethod
    {
        private readonly MethodModel _method;
        private readonly ParameterModel[] _keptParameters;
        private readonly ParameterModel[] _omittedParameters;
        private readonly OverloadVisibility _overloadVisibility;
        private readonly MatcherMethodReference[] _matchedMatcherMethods;

        public GeneratedMethod(
            MethodModel method,
            ParameterModel[] keptParameters,
            ParameterModel[] omittedParameters,
            OverloadVisibility overloadVisibility,
            IReadOnlyCollection<MatcherMethodReference>? matchedMatcherMethods)
        {
            _method = method;
            _keptParameters = keptParameters;
            _omittedParameters = omittedParameters;
            _overloadVisibility = overloadVisibility;
            _matchedMatcherMethods = NormalizeMatchedMatcherMethods(matchedMatcherMethods);

            Namespace = method.ContainingNamespace;
        }

        public string Namespace { get; }
        public MethodModel Method => _method;

        public string Render()
        {
            return _method.IsStatic && !_method.IsExtensionMethod
                ? RenderStaticExtensionBlock()
                : RenderClassicExtensionMethod();
        }

        private string RenderClassicExtensionMethod()
        {
            var builder = new StringBuilder();
            var accessibility = RenderAccessibility();
            var returnType = _method.ReturnTypeDisplay;
            var typeParams = RenderTypeParameters(_method);
            var constraints = _method.TypeParameterConstraints;
            var isExtensionMethod = _method.IsExtensionMethod;
            var receiverParameter = isExtensionMethod ? _method.Parameters.Items.FirstOrDefault() : (ParameterModel?)null;
            var receiverType = receiverParameter?.TypeDisplay
                ?? _method.ContainingTypeDisplay;
            var receiverName = receiverParameter?.Name ?? "source";
            var invocationReceiver = isExtensionMethod
                ? _method.ContainingTypeDisplay
                : receiverName;

            builder.Append("    ").Append(accessibility).Append(" static ").Append(returnType).Append(" ")
                .Append(_method.Name).Append(typeParams).Append("(");

            builder.Append("this ").Append(receiverType).Append(" ").Append(receiverName);

            foreach (var parameter in _keptParameters)
            {
                if (receiverParameter.HasValue && string.Equals(parameter.Name, receiverParameter.Value.Name, StringComparison.Ordinal))
                {
                    continue;
                }

                builder.Append(", ").Append(RenderParameter(parameter));
            }

            builder.Append(")");

            if (!string.IsNullOrWhiteSpace(constraints))
            {
                builder.Append(" ").Append(constraints);
            }

            builder.Append(" => ").Append(RenderInvocation(invocationReceiver)).Append(";");

            return builder.ToString();
        }

        private string RenderStaticExtensionBlock()
        {
            var builder = new StringBuilder();
            var accessibility = RenderAccessibility();
            var returnType = _method.ReturnTypeDisplay;
            var typeParams = RenderTypeParameters(_method);
            var constraints = _method.TypeParameterConstraints;
            var receiverType = _method.ContainingTypeDisplay;

            builder.AppendLine("    extension(" + receiverType + ")");
            builder.AppendLine("    {");
            builder.Append("        ").Append(accessibility).Append(" static ").Append(returnType).Append(" ")
                .Append(_method.Name).Append(typeParams).Append("(");

            var first = true;
            foreach (var parameter in _keptParameters)
            {
                if (!first)
                {
                    builder.Append(", ");
                }

                builder.Append(RenderParameter(parameter));
                first = false;
            }

            builder.Append(")");

            if (!string.IsNullOrWhiteSpace(constraints))
            {
                builder.Append(" ").Append(constraints);
            }

            builder.Append(" => ").Append(RenderInvocation(receiverType)).Append(";");
            builder.AppendLine();
            builder.Append("    }");

            return builder.ToString();
        }

        internal static MatcherMethodReference[] NormalizeMatchedMatcherMethods(IReadOnlyCollection<MatcherMethodReference>? matchedMatcherMethods)
        {
            if (matchedMatcherMethods is null || matchedMatcherMethods.Count == 0)
            {
                return [];
            }

            return matchedMatcherMethods
                .Distinct()
                .OrderBy(method => method.ContainingTypeDisplay, StringComparer.Ordinal)
                .ThenBy(method => method.MethodName, StringComparer.Ordinal)
                .ThenBy(method => method.ParameterCount)
                .ToArray();
        }

        private string RenderInvocation(string receiver)
        {
            var builder = new StringBuilder();
            builder.Append(receiver).Append(".").Append(_method.Name).Append(RenderTypeArguments(_method)).Append("(");

            var first = true;
            foreach (var parameter in _method.Parameters.Items)
            {
                if (!first)
                {
                    builder.Append(", ");
                }

                builder.Append(RenderArgument(parameter));
                first = false;
            }

            builder.Append(")");
            return builder.ToString();
        }

        private string RenderArgument(ParameterModel parameter)
        {
            if (_keptParameters.Any(p => string.Equals(p.Name, parameter.Name, StringComparison.Ordinal)))
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
                var elementType = parameter.TypeDisplay.Substring(0, parameter.TypeDisplay.Length - 2);
                return "global::System.Array.Empty<" + elementType + ">()";
            }

            return "default";
        }

        private string RenderAccessibility()
        {
            var accessibility = _overloadVisibility switch
            {
                OverloadVisibility.Public => Accessibility.Public,
                OverloadVisibility.Internal => Accessibility.Internal,
                OverloadVisibility.Private => Accessibility.Private,
                _ => _method.DeclaredAccessibility
            };

            if (_overloadVisibility == OverloadVisibility.MatchTarget &&
                TryGetOverloadVisibilityOverride(_method, out var overrideVisibility))
            {
                accessibility = overrideVisibility switch
                {
                    OverloadVisibility.Public => Accessibility.Public,
                    OverloadVisibility.Internal => Accessibility.Internal,
                    OverloadVisibility.Private => Accessibility.Private,
                    _ => accessibility
                };
            }

            if (_overloadVisibility == OverloadVisibility.MatchTarget &&
                (accessibility == Accessibility.ProtectedOrInternal || accessibility == Accessibility.ProtectedAndInternal))
            {
                accessibility = Accessibility.Internal;
            }

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
            if (parameter.IsParams)
            {
                builder.Append("params ");
            }

            builder.Append(parameter.RefKind switch
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
            if (method.TypeParameterCount == 0)
            {
                return string.Empty;
            }

            return "<" + string.Join(", ", method.TypeParameterNames.Items) + ">";
        }

        private static string RenderTypeArguments(MethodModel method)
        {
            if (method.TypeParameterCount == 0)
            {
                return string.Empty;
            }

            return "<" + string.Join(", ", method.TypeParameterNames.Items) + ">";
        }
    }
}
