using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tenekon.MethodOverloads.SourceGenerator.Helpers;
using Tenekon.MethodOverloads.SourceGenerator.Models;

namespace Tenekon.MethodOverloads.SourceGenerator.Parsing;

internal static partial class Parser
{
    private static TypeModel BuildTypeModel(INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
    {
        var methods = new List<MethodModel>();
        var signatures = new List<MethodSignatureModel>();

        foreach (var member in typeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var methodModel = BuildMethodModel(member, cancellationToken, out _, out _);
            methods.Add(methodModel);

            if (methodModel.IsOrdinary)
            {
                var signatureParameters = methodModel.Parameters.Items
                    .Select(p => new ParameterSignatureModel(p.SignatureTypeDisplay, p.RefKind, p.IsParams))
                    .ToImmutableArray();

                signatures.Add(
                    new MethodSignatureModel(
                        methodModel.Name,
                        methodModel.TypeParameterCount,
                        new EquatableArray<ParameterSignatureModel>(signatureParameters)));
            }
        }

        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var (options, _) = ExtractOverloadOptions(typeSymbol, cancellationToken, out _);

        return new TypeModel(
            typeSymbol.ToDisplayString(RoslynHelpers.TypeDisplayFormat),
            namespaceName,
            new EquatableArray<MethodModel>([..methods]),
            new EquatableArray<MethodSignatureModel>([..signatures]),
            options);
    }

    private static MethodModel BuildMethodModel(
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken,
        out OverloadOptionsModel syntaxOptions,
        out bool optionsFromAttribute)
    {
        var defaultMap = new Dictionary<string, bool>(StringComparer.Ordinal);
        SourceLocationModel? identifierLocation = null;

        foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syntaxRef.GetSyntax(cancellationToken) is MethodDeclarationSyntax methodSyntax)
            {
                identifierLocation ??= SourceLocationModel.FromSyntaxToken(methodSyntax.Identifier);
                foreach (var parameter in methodSyntax.ParameterList.Parameters)
                    defaultMap[parameter.Identifier.ValueText] = parameter.Default is not null;
            }
        }

        var parameters = new List<ParameterModel>();
        foreach (var parameter in methodSymbol.Parameters)
        {
            var name = parameter.Name;
            var hasDefault = defaultMap.TryGetValue(name, out var isDefault) && isDefault;

            parameters.Add(
                new ParameterModel(
                    name,
                    parameter.Type.ToDisplayString(RoslynHelpers.TypeDisplayFormat),
                    parameter.Type.ToDisplayString(RoslynHelpers.SignatureDisplayFormat),
                    parameter.RefKind,
                    parameter.IsParams,
                    parameter.IsOptional,
                    parameter.HasExplicitDefaultValue,
                    hasDefault));
        }

        var typeParameterNames = methodSymbol.TypeParameters.Select(tp => tp.Name).ToImmutableArray();

        var constraints = BuildTypeParameterConstraints(methodSymbol);
        var (options, fromAttribute) = ExtractOverloadOptions(methodSymbol, cancellationToken, out syntaxOptions);
        optionsFromAttribute = fromAttribute;

        return new MethodModel(
            methodSymbol.Name,
            methodSymbol.ContainingType?.ToDisplayString(RoslynHelpers.TypeDisplayFormat) ?? string.Empty,
            methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            methodSymbol.ReturnType.ToDisplayString(RoslynHelpers.TypeDisplayFormat),
            methodSymbol.IsStatic,
            methodSymbol.IsExtensionMethod,
            methodSymbol.DeclaredAccessibility,
            methodSymbol.TypeParameters.Length,
            new EquatableArray<string>(typeParameterNames),
            constraints,
            new EquatableArray<ParameterModel>([..parameters]),
            identifierLocation,
            methodSymbol.MethodKind == MethodKind.Ordinary,
            options,
            optionsFromAttribute);
    }

    private static string BuildTypeParameterConstraints(IMethodSymbol method)
    {
        if (method.TypeParameters.Length == 0) return string.Empty;

        var constraints = new List<string>();
        foreach (var typeParam in method.TypeParameters)
        {
            var parts = new List<string>();

            if (typeParam.HasReferenceTypeConstraint) parts.Add("class");

            if (typeParam.HasValueTypeConstraint) parts.Add("struct");

            foreach (var constraintType in typeParam.ConstraintTypes)
                parts.Add(constraintType.ToDisplayString(RoslynHelpers.TypeDisplayFormat));

            if (typeParam.HasConstructorConstraint) parts.Add("new()");

            if (parts.Count > 0) constraints.Add("where " + typeParam.Name + " : " + string.Join(", ", parts));
        }

        return string.Join(" ", constraints);
    }

    private static (ImmutableArray<GenerateOverloadsAttributeModel> AttributeModels,
            ImmutableArray<GenerateOverloadsAttributeModel> SyntaxModels)
        ExtractGenerateOverloadsAttributes(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        var attributes = RoslynHelpers.GetAttributes(methodSymbol, "GenerateOverloadsAttribute");
        var attributeModels = ExtractGenerateOverloadsAttributesFromAttributeData(
            attributes,
            methodSymbol,
            cancellationToken);
        var syntaxModels = ExtractGenerateOverloadsAttributesFromSyntax(methodSymbol, cancellationToken);

        return (attributeModels, syntaxModels);
    }

    private static (OverloadOptionsModel Options, bool FromAttribute) ExtractOverloadOptions(
        ISymbol symbol,
        CancellationToken cancellationToken,
        out OverloadOptionsModel syntaxOptions)
    {
        syntaxOptions = default;

        var syntax = GetMemberSyntax(symbol, cancellationToken);
        if (syntax is not null) syntaxOptions = ExtractOverloadOptionsFromSyntax(syntax);

        var attribute = RoslynHelpers.GetAttribute(symbol, "OverloadGenerationOptionsAttribute");
        if (attribute is not null) return (ExtractOverloadOptionsFromAttribute(attribute), true);

        if (syntaxOptions.HasAny) return (syntaxOptions, false);

        return (default, false);
    }

    private static OverloadOptionsModel ExtractOverloadOptionsFromAttribute(AttributeData attribute)
    {
        RangeAnchorMatchMode? rangeAnchorMatchMode = null;
        OverloadSubsequenceStrategy? subsequenceStrategy = null;
        OverloadVisibility? overloadVisibility = null;

        foreach (var arg in attribute.NamedArguments)
            if (string.Equals(arg.Key, "RangeAnchorMatchMode", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out RangeAnchorMatchMode value)) rangeAnchorMatchMode = value;
            }
            else if (string.Equals(arg.Key, "SubsequenceStrategy", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out OverloadSubsequenceStrategy value)) subsequenceStrategy = value;
            }
            else if (string.Equals(arg.Key, "OverloadVisibility", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out OverloadVisibility value)) overloadVisibility = value;
            }

        return new OverloadOptionsModel(rangeAnchorMatchMode, subsequenceStrategy, overloadVisibility);
    }

    private static OverloadOptionsModel ExtractOverloadOptionsFromSyntax(MemberDeclarationSyntax syntax)
    {
        RangeAnchorMatchMode? rangeAnchorMatchMode = null;
        OverloadSubsequenceStrategy? subsequenceStrategy = null;
        OverloadVisibility? overloadVisibility = null;

        foreach (var attributeList in syntax.AttributeLists)
        foreach (var attribute in attributeList.Attributes)
        {
            if (!RoslynHelpers.IsAttributeNameMatch(attribute.Name.ToString(), "OverloadGenerationOptions")) continue;

            if (attribute.ArgumentList is null) continue;

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                if (argument.NameEquals is null) continue;

                var name = argument.NameEquals.Name.Identifier.ValueText;
                if (string.Equals(name, "RangeAnchorMatchMode", StringComparison.Ordinal))
                {
                    if (TryParseEnumValue(argument.Expression, out RangeAnchorMatchMode value))
                        rangeAnchorMatchMode = value;
                }
                else if (string.Equals(name, "SubsequenceStrategy", StringComparison.Ordinal))
                {
                    if (TryParseEnumValue(argument.Expression, out OverloadSubsequenceStrategy value))
                        subsequenceStrategy = value;
                }
                else if (string.Equals(name, "OverloadVisibility", StringComparison.Ordinal))
                {
                    if (TryParseEnumValue(argument.Expression, out OverloadVisibility value))
                        overloadVisibility = value;
                }
            }
        }

        return new OverloadOptionsModel(rangeAnchorMatchMode, subsequenceStrategy, overloadVisibility);
    }

    private static (ImmutableArray<string> Displays, ImmutableArray<MatcherTypeModel> Models) ExtractMatcherTypes(
        ImmutableArray<AttributeData> attributes,
        CancellationToken cancellationToken)
    {
        if (attributes.IsDefaultOrEmpty)
            return (ImmutableArray<string>.Empty, ImmutableArray<MatcherTypeModel>.Empty);

        var symbols = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        var displays = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var attribute in attributes)
        foreach (var named in attribute.NamedArguments)
        {
            if (!string.Equals(named.Key, "Matchers", StringComparison.Ordinal)) continue;

            if (named.Value.Kind != TypedConstantKind.Array) continue;

            foreach (var constant in named.Value.Values)
                if (constant.Value is INamedTypeSymbol matcherType)
                {
                    var display = matcherType.ToDisplayString(RoslynHelpers.TypeDisplayFormat);
                    if (!seen.Add(display)) continue;

                    symbols.Add(matcherType);
                    displays.Add(display);
                }
        }

        var models = BuildMatcherTypeModels(symbols.ToImmutable(), cancellationToken);
        return (displays.ToImmutable(), models);
    }

    private static ImmutableArray<MatcherTypeModel> BuildMatcherTypeModels(
        ImmutableArray<INamedTypeSymbol> matcherSymbols,
        CancellationToken cancellationToken)
    {
        if (matcherSymbols.IsDefaultOrEmpty) return ImmutableArray<MatcherTypeModel>.Empty;

        var builder = ImmutableArray.CreateBuilder<MatcherTypeModel>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var matcherTypeSymbol in matcherSymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var display = matcherTypeSymbol.ToDisplayString(RoslynHelpers.TypeDisplayFormat);
            if (!seen.Add(display)) continue;

            var typeModel = BuildTypeModel(matcherTypeSymbol, cancellationToken);
            var matcherMethods = new List<MatcherMethodModel>();

            foreach (var member in matcherTypeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!HasGenerateOverloadsAttribute(member)) continue;

                var methodModel = BuildMethodModel(
                    member,
                    cancellationToken,
                    out var syntaxOptions,
                    out var optionsFromAttribute);
                var (attributeModels, syntaxModels) = ExtractGenerateOverloadsAttributes(member, cancellationToken);

                matcherMethods.Add(
                    new MatcherMethodModel(
                        methodModel,
                        new EquatableArray<GenerateOverloadsAttributeModel>(attributeModels),
                        new EquatableArray<GenerateOverloadsAttributeModel>(syntaxModels),
                        methodModel.Options,
                        syntaxOptions.HasAny ? syntaxOptions : null));
            }

            builder.Add(
                new MatcherTypeModel(
                    typeModel,
                    typeModel.Options,
                    new EquatableArray<MatcherMethodModel>([..matcherMethods])));
        }

        return builder.ToImmutable();
    }

    private static bool HasGenerateOverloadsAttribute(IMethodSymbol methodSymbol)
    {
        return !RoslynHelpers.GetAttributes(methodSymbol, "GenerateOverloadsAttribute").IsDefaultOrEmpty;
    }

    private static ImmutableArray<GenerateOverloadsAttributeModel>
        ExtractGenerateOverloadsAttributesFromAttributeData(
            ImmutableArray<AttributeData> attributes,
            IMethodSymbol methodSymbol,
            CancellationToken cancellationToken)
    {
        if (attributes.IsDefaultOrEmpty) return ImmutableArray<GenerateOverloadsAttributeModel>.Empty;

        var builder = ImmutableArray.CreateBuilder<GenerateOverloadsAttributeModel>();

        foreach (var attribute in attributes)
        {
            var attrSyntax = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken);
            SourceLocationModel? attributeLocation = attrSyntax is null
                ? null
                : SourceLocationModel.FromSyntaxNode(attrSyntax);
            var args = ExtractGenerateOverloadsArgsFromAttribute(
                attribute,
                attributeLocation,
                GetMethodIdentifierLocation(methodSymbol, cancellationToken));
            var hasMatchers = HasMatchersArgument(attribute);

            builder.Add(new GenerateOverloadsAttributeModel(args, hasMatchers));
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<GenerateOverloadsAttributeModel>
        ExtractGenerateOverloadsAttributesFromSyntax(
            IMethodSymbol methodSymbol,
            CancellationToken cancellationToken)
    {
        var syntax = GetMethodSyntax(methodSymbol, cancellationToken);
        if (syntax is null) return ImmutableArray<GenerateOverloadsAttributeModel>.Empty;

        return ExtractGenerateOverloadsAttributesFromSyntax(
            syntax,
            GetMethodIdentifierLocation(methodSymbol, cancellationToken));
    }

    private static ImmutableArray<GenerateOverloadsAttributeModel>
        ExtractGenerateOverloadsAttributesFromSyntax(
            MethodDeclarationSyntax methodSyntax,
            SourceLocationModel? identifierLocation)
    {
        var builder = ImmutableArray.CreateBuilder<GenerateOverloadsAttributeModel>();

        foreach (var attributeList in methodSyntax.AttributeLists)
        foreach (var attribute in attributeList.Attributes)
        {
            if (!RoslynHelpers.IsAttributeNameMatch(attribute.Name.ToString(), "GenerateOverloads")) continue;

            var args = ExtractGenerateOverloadsArgsFromSyntax(attribute, identifierLocation);
            var hasMatchers = HasMatchersArgument(attribute);

            builder.Add(new GenerateOverloadsAttributeModel(args, hasMatchers));
        }

        return builder.ToImmutable();
    }

    private static GenerateOverloadsArgsModel ExtractGenerateOverloadsArgsFromAttribute(
        AttributeData attribute,
        SourceLocationModel? attributeLocation,
        SourceLocationModel? identifierLocation)
    {
        string? beginEnd = null;
        string? begin = null;
        string? beginExclusive = null;
        string? end = null;
        string? endExclusive = null;

        if (attribute.ConstructorArguments.Length > 0
            && attribute.ConstructorArguments[index: 0].Kind == TypedConstantKind.Primitive
            && attribute.ConstructorArguments[index: 0].Value is string ctorValue)
            beginEnd = ctorValue;

        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Value.Kind != TypedConstantKind.Primitive || arg.Value.Value is not string value) continue;

            if (string.Equals(arg.Key, "Begin", StringComparison.Ordinal))
                begin = value;
            else if (string.Equals(arg.Key, "BeginExclusive", StringComparison.Ordinal))
                beginExclusive = value;
            else if (string.Equals(arg.Key, "End", StringComparison.Ordinal))
                end = value;
            else if (string.Equals(arg.Key, "EndExclusive", StringComparison.Ordinal)) endExclusive = value;
        }

        return new GenerateOverloadsArgsModel(
            beginEnd,
            begin,
            beginExclusive,
            end,
            endExclusive,
            attributeLocation,
            identifierLocation,
            SyntaxAttributeLocation: null);
    }

    private static GenerateOverloadsArgsModel ExtractGenerateOverloadsArgsFromSyntax(
        AttributeSyntax attribute,
        SourceLocationModel? identifierLocation)
    {
        string? beginEnd = null;
        string? begin = null;
        string? beginExclusive = null;
        string? end = null;
        string? endExclusive = null;

        if (attribute.ArgumentList is not null)
            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                if (argument.NameEquals is null)
                {
                    if (beginEnd is null)
                    {
                        var positionalValue = GetAttributeStringValue(argument.Expression);
                        if (!string.IsNullOrEmpty(positionalValue)) beginEnd = positionalValue;
                    }

                    continue;
                }

                var name = argument.NameEquals.Name.Identifier.ValueText;
                var value = GetAttributeStringValue(argument.Expression);

                if (string.IsNullOrEmpty(value)) continue;

                if (string.Equals(name, "Begin", StringComparison.Ordinal))
                    begin = value;
                else if (string.Equals(name, "BeginExclusive", StringComparison.Ordinal))
                    beginExclusive = value;
                else if (string.Equals(name, "End", StringComparison.Ordinal))
                    end = value;
                else if (string.Equals(name, "EndExclusive", StringComparison.Ordinal)) endExclusive = value;
            }

        return new GenerateOverloadsArgsModel(
            beginEnd,
            begin,
            beginExclusive,
            end,
            endExclusive,
            AttributeLocation: null,
            identifierLocation,
            SourceLocationModel.FromSyntaxNode(attribute));
    }

    private static bool HasMatchersArgument(AttributeData attribute)
    {
        foreach (var argument in attribute.NamedArguments)
            if (string.Equals(argument.Key, "Matchers", StringComparison.Ordinal))
                return true;

        return false;
    }

    private static bool HasMatchersArgument(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList is null) return false;

        foreach (var argument in attribute.ArgumentList.Arguments)
        {
            if (argument.NameEquals is null) continue;

            var name = argument.NameEquals.Name.Identifier.ValueText;
            if (string.Equals(name, "Matchers", StringComparison.Ordinal)) return true;
        }

        return false;
    }

    private static string? GetAttributeStringValue(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
            return literal.Token.ValueText;

        if (expression is InvocationExpressionSyntax invocation
            && invocation.Expression is IdentifierNameSyntax identifier
            && string.Equals(identifier.Identifier.ValueText, "nameof", StringComparison.Ordinal)
            && invocation.ArgumentList.Arguments.Count == 1)
        {
            var argExpression = invocation.ArgumentList.Arguments[index: 0].Expression;
            return argExpression switch
            {
                IdentifierNameSyntax id => id.Identifier.ValueText,
                MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
                _ => null
            };
        }

        return null;
    }

    private static SourceLocationModel? GetMethodIdentifierLocation(
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        var syntax = GetMethodSyntax(methodSymbol, cancellationToken);
        return syntax is null ? null : SourceLocationModel.FromSyntaxToken(syntax.Identifier);
    }

    private static MethodDeclarationSyntax? GetMethodSyntax(
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
            if (syntaxRef.GetSyntax(cancellationToken) is MethodDeclarationSyntax methodSyntax)
                return methodSyntax;

        return null;
    }

    private static MemberDeclarationSyntax? GetMemberSyntax(ISymbol symbol, CancellationToken cancellationToken)
    {
        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
            if (syntaxRef.GetSyntax(cancellationToken) is MemberDeclarationSyntax memberSyntax)
                return memberSyntax;

        return null;
    }

    private static bool TryParseEnumValue<TEnum>(ExpressionSyntax expression, out TEnum value) where TEnum : struct
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
            if (string.IsNullOrEmpty(name)) return false;
        }

        return Enum.TryParse(name, out value);
    }

    private static bool TryGetEnumConstant<TEnum>(TypedConstant constant, out TEnum value) where TEnum : struct
    {
        value = default;
        if (constant.Value is null) return false;

        try
        {
            value = (TEnum)Enum.ToObject(typeof(TEnum), constant.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildMethodIdentityKey(MethodModel method)
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
}
