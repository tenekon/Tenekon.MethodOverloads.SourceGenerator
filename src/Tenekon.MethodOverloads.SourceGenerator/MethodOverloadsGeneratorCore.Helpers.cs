using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Shared helpers for parsing attributes and enum values.
    /// </summary>
    private static bool TryGetOverloadOptionsFromSyntax(ISymbol symbol, out OverloadOptionsSyntax options)
    {
        options = default;

        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not MemberDeclarationSyntax memberSyntax)
            {
                continue;
            }

            if (TryGetOverloadOptionsFromSyntax(memberSyntax, out options))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryGetOverloadOptionsFromSyntax(MemberDeclarationSyntax memberSyntax, out OverloadOptionsSyntax options)
    {
        options = default;

        foreach (var attributeList in memberSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                if (!IsAttributeNameMatch(name, "OverloadGenerationOptions"))
                {
                    continue;
                }

                if (attribute.ArgumentList is null)
                {
                    return false;
                }

                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    if (argument.NameEquals is null)
                    {
                        continue;
                    }

                    var argName = argument.NameEquals.Name.Identifier.ValueText;
                    if (string.Equals(argName, "RangeAnchorMatchMode", StringComparison.Ordinal))
                    {
                        if (TryParseEnumValue(argument.Expression, out RangeAnchorMatchMode value))
                        {
                            options.RangeAnchorMatchMode = value;
                        }
                    }
                    else if (string.Equals(argName, "SubsequenceStrategy", StringComparison.Ordinal))
                    {
                        if (TryParseEnumValue(argument.Expression, out OverloadSubsequenceStrategy value))
                        {
                            options.SubsequenceStrategy = value;
                        }
                    }
                    else if (string.Equals(argName, "OverloadVisibility", StringComparison.Ordinal))
                    {
                        if (TryParseEnumValue(argument.Expression, out OverloadVisibility value))
                        {
                            options.OverloadVisibility = value;
                        }
                    }
                }

                return options.HasAny;
            }
        }

        return false;
    }

    private static bool IsAttributeNameMatch(string name, string expected)
    {
        if (string.Equals(name, expected, StringComparison.Ordinal) ||
            string.Equals(name, expected + "Attribute", StringComparison.Ordinal))
        {
            return true;
        }

        return name.EndsWith("." + expected, StringComparison.Ordinal) ||
               name.EndsWith("." + expected + "Attribute", StringComparison.Ordinal);
    }

    private static bool TryParseEnumValue<TEnum>(ExpressionSyntax expression, out TEnum value)
        where TEnum : struct
    {
        value = default;
        var name = expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(name))
        {
            var text = expression.ToString();
            var parts = text.Split('.');
            name = parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
        }

        return Enum.TryParse(name, out value);
    }
}
