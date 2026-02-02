using System.Collections.Generic;
using System.Linq;
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
        private readonly IMethodSymbol _method;
        private readonly IParameterSymbol[] _keptParameters;
        private readonly IParameterSymbol[] _omittedParameters;
        private readonly OverloadVisibility _overloadVisibility;

        public GeneratedMethod(IMethodSymbol method, IParameterSymbol[] keptParameters, IParameterSymbol[] omittedParameters, OverloadVisibility overloadVisibility)
        {
            _method = method;
            _keptParameters = keptParameters;
            _omittedParameters = omittedParameters;
            _overloadVisibility = overloadVisibility;

            Namespace = method.ContainingType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        }

        public string Namespace { get; }
        public IMethodSymbol Method => _method;

        public string Render()
        {
            return _method.IsStatic
                ? RenderStaticExtensionBlock()
                : RenderClassicExtensionMethod();
        }

        private string RenderClassicExtensionMethod()
        {
            var builder = new StringBuilder();
            var accessibility = RenderAccessibility();
            var returnType = _method.ReturnType.ToDisplayString(TypeDisplayFormat);
            var typeParams = RenderTypeParameters(_method);
            var constraints = RenderTypeParameterConstraints(_method);

            builder.Append("    ").Append(accessibility).Append(" static ").Append(returnType).Append(" ")
                .Append(_method.Name).Append(typeParams).Append("(");

            builder.Append("this ").Append(_method.ContainingType.ToDisplayString(TypeDisplayFormat)).Append(" source");

            foreach (var parameter in _keptParameters)
            {
                builder.Append(", ").Append(RenderParameter(parameter));
            }

            builder.Append(")");

            if (!string.IsNullOrWhiteSpace(constraints))
            {
                builder.Append(" ").Append(constraints);
            }

            builder.Append(" => ").Append(RenderInvocation("source")).Append(";");

            return builder.ToString();
        }

        private string RenderStaticExtensionBlock()
        {
            var builder = new StringBuilder();
            var accessibility = RenderAccessibility();
            var returnType = _method.ReturnType.ToDisplayString(TypeDisplayFormat);
            var typeParams = RenderTypeParameters(_method);
            var constraints = RenderTypeParameterConstraints(_method);
            var receiverType = _method.ContainingType.ToDisplayString(TypeDisplayFormat);

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

        private string RenderInvocation(string receiver)
        {
            var builder = new StringBuilder();
            builder.Append(receiver).Append(".").Append(_method.Name).Append(RenderTypeArguments(_method)).Append("(");

            var first = true;
            foreach (var parameter in _method.Parameters)
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

        private string RenderArgument(IParameterSymbol parameter)
        {
            if (_keptParameters.Any(p => SymbolEqualityComparer.Default.Equals(p, parameter)))
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

            if (parameter.IsParams && parameter.Type is IArrayTypeSymbol arrayType)
            {
                var elementType = arrayType.ElementType.ToDisplayString(TypeDisplayFormat);
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

        private static string RenderParameter(IParameterSymbol parameter)
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

            builder.Append(parameter.Type.ToDisplayString(TypeDisplayFormat));
            builder.Append(" ").Append(parameter.Name);
            return builder.ToString();
        }

        private static string RenderTypeParameters(IMethodSymbol method)
        {
            if (method.TypeParameters.Length == 0)
            {
                return string.Empty;
            }

            return "<" + string.Join(", ", method.TypeParameters.Select(tp => tp.Name)) + ">";
        }

        private static string RenderTypeArguments(IMethodSymbol method)
        {
            if (method.TypeParameters.Length == 0)
            {
                return string.Empty;
            }

            return "<" + string.Join(", ", method.TypeParameters.Select(tp => tp.Name)) + ">";
        }

        private static string RenderTypeParameterConstraints(IMethodSymbol method)
        {
            if (method.TypeParameters.Length == 0)
            {
                return string.Empty;
            }

            var constraints = new List<string>();
            foreach (var typeParam in method.TypeParameters)
            {
                var parts = new List<string>();

                if (typeParam.HasReferenceTypeConstraint)
                {
                    parts.Add("class");
                }

                if (typeParam.HasValueTypeConstraint)
                {
                    parts.Add("struct");
                }

                foreach (var constraintType in typeParam.ConstraintTypes)
                {
                    parts.Add(constraintType.ToDisplayString(TypeDisplayFormat));
                }

                if (typeParam.HasConstructorConstraint)
                {
                    parts.Add("new()");
                }

                if (parts.Count > 0)
                {
                    constraints.Add("where " + typeParam.Name + " : " + string.Join(", ", parts));
                }
            }

            return string.Join(" ", constraints);
        }
    }
}

