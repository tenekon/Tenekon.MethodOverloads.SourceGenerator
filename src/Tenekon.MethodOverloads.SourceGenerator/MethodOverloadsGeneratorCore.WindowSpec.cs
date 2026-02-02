using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Builds optional-parameter windows from attributes and syntax.
    /// </summary>
    private static bool TryCreateWindowSpecFromSyntax(
        IMethodSymbol targetMethod,
        MethodDeclarationSyntax methodSyntax,
        out WindowSpec windowSpec,
        out WindowSpecFailure failure)
    {
        return TryCreateWindowSpecFromSyntax(
            targetMethod,
            targetMethod,
            methodSyntax,
            ParameterMatch.Identity(targetMethod.Parameters.Length),
            out windowSpec,
            out failure);
    }

    private static bool TryCreateWindowSpecFromSyntax(
        IMethodSymbol targetMethod,
        IMethodSymbol matcherMethod,
        MethodDeclarationSyntax methodSyntax,
        ParameterMatch match,
        out WindowSpec windowSpec,
        out WindowSpecFailure failure)
    {
        windowSpec = default;
        failure = new WindowSpecFailure(WindowSpecFailureKind.None, null, null);

        if (!TryGetGenerateOverloadsArgs(methodSyntax, out var args))
        {
            return false;
        }

        return TryCreateWindowSpecFromArgs(targetMethod, matcherMethod, args, match, out windowSpec, out failure);
    }
    private static bool TryGetGenerateOverloadsArgs(MethodDeclarationSyntax methodSyntax, out GenerateOverloadsArgs args)
    {
        args = default;

        foreach (var attributeList in methodSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (!IsAttributeNameMatch(attribute.Name.ToString(), "GenerateOverloads"))
                {
                    continue;
                }

                if (attribute.ArgumentList is null)
                {
                    return true;
                }

                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    if (argument.NameEquals is null)
                    {
                        if (args.BeginEnd is null)
                        {
                            var positionalValue = GetAttributeStringValue(argument.Expression);
                            if (!string.IsNullOrEmpty(positionalValue))
                            {
                                args.BeginEnd = positionalValue;
                            }
                        }
                        continue;
                    }

                    var name = argument.NameEquals.Name.Identifier.ValueText;
                    var value = GetAttributeStringValue(argument.Expression);

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    if (string.Equals(name, "Begin", StringComparison.Ordinal))
                    {
                        args.Begin = value;
                    }
                    else if (string.Equals(name, "BeginExclusive", StringComparison.Ordinal))
                    {
                        args.BeginExclusive = value;
                    }
                    else if (string.Equals(name, "End", StringComparison.Ordinal))
                    {
                        args.End = value;
                    }
                    else if (string.Equals(name, "EndExclusive", StringComparison.Ordinal))
                    {
                        args.EndExclusive = value;
                    }
                }

                return true;
            }
        }

        return false;
    }
    private static string? GetAttributeStringValue(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        if (expression is InvocationExpressionSyntax invocation &&
            invocation.Expression is IdentifierNameSyntax identifier &&
            string.Equals(identifier.Identifier.ValueText, "nameof", StringComparison.Ordinal) &&
            invocation.ArgumentList.Arguments.Count == 1)
        {
            var argExpression = invocation.ArgumentList.Arguments[0].Expression;
            return argExpression switch
            {
                IdentifierNameSyntax id => id.Identifier.ValueText,
                MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
                _ => null
            };
        }

        return null;
    }
    private bool TryCreateWindowSpec(IMethodSymbol method, AttributeData generateOverloads, out WindowSpec windowSpec, out WindowSpecFailure failure)
    {
        if (!TryGetGenerateOverloadsArgs(generateOverloads, out var args))
        {
            windowSpec = default;
            failure = new WindowSpecFailure(WindowSpecFailureKind.None, null, null);
            return false;
        }

        return TryCreateWindowSpecFromArgs(method, method, args, ParameterMatch.Identity(method.Parameters.Length), out windowSpec, out failure);
    }
    private bool TryCreateWindowSpec(
        IMethodSymbol targetMethod,
        IMethodSymbol matcherMethod,
        AttributeData generateOverloads,
        ParameterMatch match,
        out WindowSpec windowSpec,
        out WindowSpecFailure failure)
    {
        if (!TryGetGenerateOverloadsArgs(generateOverloads, out var args))
        {
            windowSpec = default;
            failure = new WindowSpecFailure(WindowSpecFailureKind.None, null, null);
            return false;
        }

        return TryCreateWindowSpecFromArgs(targetMethod, matcherMethod, args, match, out windowSpec, out failure);
    }
    private static bool TryCreateWindowSpecFromArgs(
        IMethodSymbol targetMethod,
        IMethodSymbol matcherMethod,
        GenerateOverloadsArgs args,
        ParameterMatch match,
        out WindowSpec windowSpec,
        out WindowSpecFailure failure)
    {
        windowSpec = default;
        failure = new WindowSpecFailure(WindowSpecFailureKind.None, null, null);

        var matcherParams = matcherMethod.Parameters;

        var startIndex = 0;
        var endIndex = targetMethod.Parameters.Length - 1;
        if (match.TargetIndices.Length > 0)
        {
            startIndex = match.TargetIndices[0];
            endIndex = match.TargetIndices[match.TargetIndices.Length - 1];
        }

        if (!string.IsNullOrEmpty(args.BeginEnd) &&
            (!string.IsNullOrEmpty(args.Begin) ||
             !string.IsNullOrEmpty(args.End) ||
             !string.IsNullOrEmpty(args.BeginExclusive) ||
             !string.IsNullOrEmpty(args.EndExclusive)))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.ConflictingAnchors, null, null);
            return false;
        }

        if (!string.IsNullOrEmpty(args.Begin) && !string.IsNullOrEmpty(args.BeginExclusive))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.ConflictingBeginAnchors, "Begin", args.Begin);
            return false;
        }

        if (!string.IsNullOrEmpty(args.End) && !string.IsNullOrEmpty(args.EndExclusive))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.ConflictingEndAnchors, "End", args.End);
            return false;
        }

        if (!string.IsNullOrEmpty(args.BeginEnd))
        {
            args.Begin = args.BeginEnd;
            args.End = args.BeginEnd;
        }
        else if (!string.IsNullOrEmpty(args.Begin) &&
                 !string.IsNullOrEmpty(args.End) &&
                 string.Equals(args.Begin, args.End, StringComparison.Ordinal))
        {
            failure = new WindowSpecFailure(WindowSpecFailureKind.RedundantAnchors, "BeginEnd", args.Begin);
        }

        if (!string.IsNullOrEmpty(args.Begin))
        {
            var beginIdx = IndexOfParameter(matcherParams, args.Begin!);
            if (beginIdx < 0)
            {
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "Begin", args.Begin);
                return false;
            }

            startIndex = match.TargetIndices[beginIdx];
        }

        if (!string.IsNullOrEmpty(args.BeginExclusive))
        {
            var beginIdx = IndexOfParameter(matcherParams, args.BeginExclusive!);
            if (beginIdx < 0)
            {
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "BeginExclusive", args.BeginExclusive);
                return false;
            }

            startIndex = match.TargetIndices[beginIdx] + 1;
        }

        if (!string.IsNullOrEmpty(args.End))
        {
            var endIdx = IndexOfParameter(matcherParams, args.End!);
            if (endIdx < 0)
            {
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "End", args.End);
                return false;
            }

            endIndex = match.TargetIndices[endIdx];
        }

        if (!string.IsNullOrEmpty(args.EndExclusive))
        {
            var endIdx = IndexOfParameter(matcherParams, args.EndExclusive!);
            if (endIdx < 0)
            {
                failure = new WindowSpecFailure(WindowSpecFailureKind.MissingAnchor, "EndExclusive", args.EndExclusive);
                return false;
            }

            endIndex = match.TargetIndices[endIdx] - 1;
        }

        windowSpec = new WindowSpec(startIndex, endIndex);
        return true;
    }
    private static int IndexOfParameter(ImmutableArray<IParameterSymbol> parameters, string name)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            if (string.Equals(parameters[i].Name, name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }
    private static string? GetNamedString(AttributeData attribute, string name)
    {
        foreach (var arg in attribute.NamedArguments)
        {
            if (string.Equals(arg.Key, name, StringComparison.Ordinal) && arg.Value.Value is string value)
            {
                return value;
            }
        }

        return null;
    }
    private static bool TryGetGenerateOverloadsArgs(AttributeData attribute, out GenerateOverloadsArgs args)
    {
        args = default;

        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive &&
            attribute.ConstructorArguments[0].Value is string ctorValue)
        {
            args.BeginEnd = ctorValue;
        }

        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Value.Kind != TypedConstantKind.Primitive || arg.Value.Value is not string value)
            {
                continue;
            }

            if (string.Equals(arg.Key, "Begin", StringComparison.Ordinal))
            {
                args.Begin = value;
            }
            else if (string.Equals(arg.Key, "BeginExclusive", StringComparison.Ordinal))
            {
                args.BeginExclusive = value;
            }
            else if (string.Equals(arg.Key, "End", StringComparison.Ordinal))
            {
                args.End = value;
            }
            else if (string.Equals(arg.Key, "EndExclusive", StringComparison.Ordinal))
            {
                args.EndExclusive = value;
            }
        }

        return true;
    }
}

