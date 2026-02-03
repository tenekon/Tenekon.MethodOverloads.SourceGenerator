using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tenekon.MethodOverloads.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class MethodOverloadsGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat TypeDisplayFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static readonly SymbolDisplayFormat SignatureDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postContext =>
        {
            postContext.AddSource("EmbeddedAttribute.g.cs", GeneratorAttributesSource.EmbeddedAttribute);
            postContext.AddSource("GenerateOverloadsAttribute.g.cs", GeneratorAttributesSource.GenerateOverloadsAttribute);
            postContext.AddSource("GenerateMethodOverloadsAttribute.g.cs", GeneratorAttributesSource.GenerateMethodOverloadsAttribute);
            postContext.AddSource("OverloadGenerationOptionsAttribute.g.cs", GeneratorAttributesSource.OverloadGenerationOptionsAttribute);
            postContext.AddSource("MatcherUsageAttribute.g.cs", GeneratorAttributesSource.MatcherUsageAttribute);
        });

        var attributesOnlyProvider = context.AnalyzerConfigOptionsProvider
            .Select((provider, _) =>
            {
                if (provider.GlobalOptions.TryGetValue(
                        "build_property.TenekonMethodOverloadsSourceGeneratorAttributesOnly",
                        out var raw))
                {
                    return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(raw, "1", StringComparison.Ordinal);
                }

                return false;
            });

        var typeAttributeProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Tenekon.MethodOverloads.SourceGenerator.GenerateMethodOverloadsAttribute",
            static (node, _) => node is BaseTypeDeclarationSyntax,
            static (attributeContext, _) => attributeContext.TargetSymbol as INamedTypeSymbol);

        var methodAttributeProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Tenekon.MethodOverloads.SourceGenerator.GenerateOverloadsAttribute",
            static (node, _) => node is MethodDeclarationSyntax,
            static (attributeContext, _) => attributeContext.TargetSymbol as IMethodSymbol);

        var typeSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is BaseTypeDeclarationSyntax typeDecl && HasAttribute(typeDecl.AttributeLists, "GenerateMethodOverloads"),
            static (syntaxContext, _) => syntaxContext.SemanticModel.GetDeclaredSymbol((BaseTypeDeclarationSyntax)syntaxContext.Node) as INamedTypeSymbol);

        var methodSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is MethodDeclarationSyntax methodDecl && HasAttribute(methodDecl.AttributeLists, "GenerateOverloads"),
            static (syntaxContext, _) => syntaxContext.SemanticModel.GetDeclaredSymbol((MethodDeclarationSyntax)syntaxContext.Node) as IMethodSymbol);

        var targetTypesProvider = typeAttributeProvider
            .Where(static symbol => symbol is not null)
            .Select(static (symbol, _) => symbol!)
            .Collect()
            .Combine(typeSyntaxProvider.Where(static symbol => symbol is not null)
                .Select(static (symbol, _) => symbol!)
                .Collect())
            .Select(static (pair, _) => MergeTypes(pair.Left, pair.Right))
            .WithComparer(NamedTypeSymbolArrayComparer.Instance);

        var targetMethodsProvider = methodAttributeProvider
            .Where(static symbol => symbol is not null)
            .Select(static (symbol, _) => symbol!)
            .Collect()
            .Combine(methodSyntaxProvider.Where(static symbol => symbol is not null)
                .Select(static (symbol, _) => symbol!)
                .Collect())
            .Select(static (pair, _) => MergeMethods(pair.Left, pair.Right))
            .WithComparer(MethodSymbolArrayComparer.Instance);

        var inputModelProvider = context.CompilationProvider
            .Combine(targetTypesProvider)
            .Combine(targetMethodsProvider)
            .Select(static (tuple, ct) => BuildInputModel(tuple.Left.Left, tuple.Left.Right, tuple.Right, ct));

        var syntaxTreesProvider = context.CompilationProvider
            .Select(static (compilation, _) => compilation.SyntaxTrees.ToImmutableArray());

        var combined = inputModelProvider
            .Combine(syntaxTreesProvider)
            .Combine(attributesOnlyProvider);

        context.RegisterSourceOutput(combined, static (productionContext, tuple) =>
        {
            var inputModel = tuple.Left.Left;
            var syntaxTrees = tuple.Left.Right;
            var attributesOnly = tuple.Right;
            if (attributesOnly)
            {
                return;
            }

            var syntaxTreesByPath = new Dictionary<string, SyntaxTree>(StringComparer.Ordinal);
            foreach (var tree in syntaxTrees)
            {
                if (!string.IsNullOrEmpty(tree.FilePath))
                {
                    syntaxTreesByPath[tree.FilePath] = tree;
                }
            }

            var generator = new MethodOverloadsGeneratorCore(inputModel, productionContext, syntaxTreesByPath);
            generator.Execute();
        });
    }

    private static MethodOverloadsGeneratorCore.GeneratorInputModel BuildInputModel(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> typeSymbols,
        ImmutableArray<IMethodSymbol> methodSymbols,
        CancellationToken cancellationToken)
    {
        var typeTargets = new List<MethodOverloadsGeneratorCore.TypeTargetModel>();
        var methodTargets = new List<MethodOverloadsGeneratorCore.MethodTargetModel>();
        var matcherTypeSymbols = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var typeModelSymbols = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var typeSymbol in typeSymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            typeModelSymbols.Add(typeSymbol);

            var typeModel = BuildTypeModel(compilation, typeSymbol, cancellationToken);
            var matchers = ExtractMatcherTypes(typeSymbol, compilation, cancellationToken, out var matcherSymbols);
            foreach (var matcherSymbol in matcherSymbols)
            {
                matcherTypeSymbols.Add(matcherSymbol);
                typeModelSymbols.Add(matcherSymbol);
            }

            typeTargets.Add(new MethodOverloadsGeneratorCore.TypeTargetModel(
                typeModel,
                true,
                new MethodOverloadsGeneratorCore.EquatableArray<string>(matchers),
                typeModel.Options));
        }

        foreach (var methodSymbol in methodSymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (methodSymbol.ContainingType is null)
            {
                continue;
            }

            typeModelSymbols.Add(methodSymbol.ContainingType);

            var methodModel = BuildMethodModel(compilation, methodSymbol, cancellationToken);
            var matcherTypes = ExtractMatcherTypes(methodSymbol, compilation, cancellationToken, out var matcherSymbols);
            foreach (var matcherSymbol in matcherSymbols)
            {
                matcherTypeSymbols.Add(matcherSymbol);
                typeModelSymbols.Add(matcherSymbol);
            }

            var (attributeArgs, syntaxArgs) = ExtractGenerateOverloadsArgs(methodSymbol, compilation, cancellationToken);
            var (options, optionsFromAttribute) = ExtractOverloadOptions(methodSymbol, compilation, cancellationToken, out var syntaxOptions);

            methodTargets.Add(new MethodOverloadsGeneratorCore.MethodTargetModel(
                methodModel,
                true,
                attributeArgs,
                syntaxArgs,
                new MethodOverloadsGeneratorCore.EquatableArray<string>(matcherTypes),
                options,
                syntaxOptions.HasAny ? syntaxOptions : null,
                optionsFromAttribute));
        }

        var matcherTypeModels = new List<MethodOverloadsGeneratorCore.MatcherTypeModel>();
        foreach (var matcherTypeSymbol in matcherTypeSymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var typeModel = BuildTypeModel(compilation, matcherTypeSymbol, cancellationToken);
            var matcherMethods = new List<MethodOverloadsGeneratorCore.MatcherMethodModel>();

            foreach (var member in matcherTypeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (member.MethodKind != MethodKind.Ordinary)
                {
                    continue;
                }

                var hasGenerateOverloads = HasGenerateOverloadsAttribute(member, compilation, cancellationToken);
                if (!hasGenerateOverloads)
                {
                    continue;
                }

                var methodModel = BuildMethodModel(compilation, member, cancellationToken);
                var (attributeArgs, syntaxArgs) = ExtractGenerateOverloadsArgs(member, compilation, cancellationToken);
                var (options, _) = ExtractOverloadOptions(member, compilation, cancellationToken, out var syntaxOptions);

                matcherMethods.Add(new MethodOverloadsGeneratorCore.MatcherMethodModel(
                    methodModel,
                    attributeArgs,
                    syntaxArgs,
                    options,
                    syntaxOptions.HasAny ? syntaxOptions : null));
            }

            matcherTypeModels.Add(new MethodOverloadsGeneratorCore.MatcherTypeModel(
                typeModel,
                typeModel.Options,
                new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.MatcherMethodModel>([..matcherMethods])));
        }

        var types = new List<MethodOverloadsGeneratorCore.TypeModel>();
        foreach (var typeSymbol in typeModelSymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            types.Add(BuildTypeModel(compilation, typeSymbol, cancellationToken));
        }

        return new MethodOverloadsGeneratorCore.GeneratorInputModel(
            new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.TypeModel>([..types]),
            new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.TypeTargetModel>([..typeTargets]),
            new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.MethodTargetModel>([..methodTargets]),
            new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.MatcherTypeModel>([..matcherTypeModels]));
    }

    private static MethodOverloadsGeneratorCore.TypeModel BuildTypeModel(
        Compilation compilation,
        INamedTypeSymbol typeSymbol,
        CancellationToken cancellationToken)
    {
        var methods = new List<MethodOverloadsGeneratorCore.MethodModel>();
        var signatures = new List<MethodOverloadsGeneratorCore.MethodSignatureModel>();

        foreach (var member in typeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var methodModel = BuildMethodModel(compilation, member, cancellationToken);
            methods.Add(methodModel);

            if (methodModel.IsOrdinary)
            {
                var signatureParameters = methodModel.Parameters.Items
                    .Select(p => new MethodOverloadsGeneratorCore.ParameterSignatureModel(p.SignatureTypeDisplay, p.RefKind, p.IsParams))
                    .ToImmutableArray();

                signatures.Add(new MethodOverloadsGeneratorCore.MethodSignatureModel(
                    methodModel.Name,
                    methodModel.TypeParameterCount,
                    new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.ParameterSignatureModel>(signatureParameters)));
            }
        }

        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var (options, _) = ExtractOverloadOptions(typeSymbol, compilation, cancellationToken, out _);
        return new MethodOverloadsGeneratorCore.TypeModel(
            typeSymbol.ToDisplayString(TypeDisplayFormat),
            namespaceName,
            new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.MethodModel>([..methods]),
            new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.MethodSignatureModel>([..signatures]),
            options);
    }

    private static MethodOverloadsGeneratorCore.MethodModel BuildMethodModel(
        Compilation compilation,
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        var defaultMap = new Dictionary<string, bool>(StringComparer.Ordinal);
        MethodOverloadsGeneratorCore.SourceLocationModel? identifierLocation = null;

        foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syntaxRef.GetSyntax(cancellationToken) is MethodDeclarationSyntax methodSyntax)
            {
                identifierLocation ??= GetLocationModel(methodSyntax.Identifier);
                foreach (var parameter in methodSyntax.ParameterList.Parameters)
                {
                    defaultMap[parameter.Identifier.ValueText] = parameter.Default is not null;
                }
            }
        }

        var parameters = new List<MethodOverloadsGeneratorCore.ParameterModel>();
        foreach (var parameter in methodSymbol.Parameters)
        {
            var name = parameter.Name;
            var hasDefault = defaultMap.TryGetValue(name, out var isDefault) && isDefault;

            parameters.Add(new MethodOverloadsGeneratorCore.ParameterModel(
                name,
                parameter.Type.ToDisplayString(TypeDisplayFormat),
                parameter.Type.ToDisplayString(SignatureDisplayFormat),
                parameter.RefKind,
                parameter.IsParams,
                parameter.IsOptional,
                parameter.HasExplicitDefaultValue,
                hasDefault));
        }

        var typeParameterNames = methodSymbol.TypeParameters
            .Select(tp => tp.Name)
            .ToImmutableArray();

        var constraints = BuildTypeParameterConstraints(methodSymbol);
        var (options, optionsFromAttribute) = ExtractOverloadOptions(methodSymbol, compilation, cancellationToken, out _);

        return new MethodOverloadsGeneratorCore.MethodModel(
            methodSymbol.Name,
            methodSymbol.ContainingType?.ToDisplayString(TypeDisplayFormat) ?? string.Empty,
            methodSymbol.ContainingType?.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            methodSymbol.ReturnType.ToDisplayString(TypeDisplayFormat),
            methodSymbol.IsStatic,
            methodSymbol.IsExtensionMethod,
            methodSymbol.DeclaredAccessibility,
            methodSymbol.TypeParameters.Length,
            new MethodOverloadsGeneratorCore.EquatableArray<string>(typeParameterNames),
            constraints,
            new MethodOverloadsGeneratorCore.EquatableArray<MethodOverloadsGeneratorCore.ParameterModel>([..parameters]),
            identifierLocation,
            methodSymbol.MethodKind == MethodKind.Ordinary,
            options,
            optionsFromAttribute);
    }

    private static string BuildTypeParameterConstraints(IMethodSymbol method)
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

    private static (MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel? AttributeArgs,
        MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel? SyntaxArgs)
        ExtractGenerateOverloadsArgs(IMethodSymbol methodSymbol, Compilation compilation, CancellationToken cancellationToken)
    {
        MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel? attributeArgs = null;
        MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel? syntaxArgs = null;

        var attribute = GetAttribute(methodSymbol, "GenerateOverloadsAttribute");
        if (attribute is not null)
        {
            var attrSyntax = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken);
            MethodOverloadsGeneratorCore.SourceLocationModel? attributeLocation =
                attrSyntax is null ? null : GetLocationModel(attrSyntax);
            attributeArgs = ExtractGenerateOverloadsArgsFromAttribute(attribute, attributeLocation, GetMethodIdentifierLocation(methodSymbol, cancellationToken));
        }

        var syntax = GetMethodSyntax(methodSymbol, cancellationToken);
        if (syntax is not null)
        {
            syntaxArgs = ExtractGenerateOverloadsArgsFromSyntax(syntax, GetMethodIdentifierLocation(methodSymbol, cancellationToken));
        }

        return (attributeArgs, syntaxArgs);
    }

    private static (OverloadOptionsModel Options, bool FromAttribute)
        ExtractOverloadOptions(ISymbol symbol, Compilation compilation, CancellationToken cancellationToken, out OverloadOptionsModel syntaxOptions)
    {
        syntaxOptions = default;

        var syntax = GetMemberSyntax(symbol, cancellationToken);
        if (syntax is not null)
        {
            syntaxOptions = ExtractOverloadOptionsFromSyntax(syntax);
        }

        var attribute = GetAttribute(symbol, "OverloadGenerationOptionsAttribute");
        if (attribute is not null)
        {
            return (ExtractOverloadOptionsFromAttribute(attribute), true);
        }

        if (syntaxOptions.HasAny)
        {
            return (syntaxOptions, false);
        }

        return (default, false);
    }

    private static OverloadOptionsModel ExtractOverloadOptionsFromAttribute(AttributeData attribute)
    {
        RangeAnchorMatchMode? rangeAnchorMatchMode = null;
        OverloadSubsequenceStrategy? subsequenceStrategy = null;
        OverloadVisibility? overloadVisibility = null;

        foreach (var arg in attribute.NamedArguments)
        {
            if (string.Equals(arg.Key, "RangeAnchorMatchMode", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out RangeAnchorMatchMode value))
                {
                    rangeAnchorMatchMode = value;
                }
            }
            else if (string.Equals(arg.Key, "SubsequenceStrategy", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out OverloadSubsequenceStrategy value))
                {
                    subsequenceStrategy = value;
                }
            }
            else if (string.Equals(arg.Key, "OverloadVisibility", StringComparison.Ordinal))
            {
                if (TryGetEnumConstant(arg.Value, out OverloadVisibility value))
                {
                    overloadVisibility = value;
                }
            }
        }

        return new OverloadOptionsModel(rangeAnchorMatchMode, subsequenceStrategy, overloadVisibility);
    }

    private static OverloadOptionsModel ExtractOverloadOptionsFromSyntax(MemberDeclarationSyntax memberSyntax)
    {
        RangeAnchorMatchMode? rangeAnchorMatchMode = null;
        OverloadSubsequenceStrategy? subsequenceStrategy = null;
        OverloadVisibility? overloadVisibility = null;

        foreach (var attributeList in memberSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (!IsAttributeNameMatch(attribute.Name.ToString(), "OverloadGenerationOptions"))
                {
                    continue;
                }

                if (attribute.ArgumentList is null)
                {
                    return default;
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
                            rangeAnchorMatchMode = value;
                        }
                    }
                    else if (string.Equals(argName, "SubsequenceStrategy", StringComparison.Ordinal))
                    {
                        if (TryParseEnumValue(argument.Expression, out OverloadSubsequenceStrategy value))
                        {
                            subsequenceStrategy = value;
                        }
                    }
                    else if (string.Equals(argName, "OverloadVisibility", StringComparison.Ordinal))
                    {
                        if (TryParseEnumValue(argument.Expression, out OverloadVisibility value))
                        {
                            overloadVisibility = value;
                        }
                    }
                }

                return new OverloadOptionsModel(rangeAnchorMatchMode, subsequenceStrategy, overloadVisibility);
            }
        }

        return default;
    }

    private static ImmutableArray<string> ExtractMatcherTypes(ISymbol symbol, Compilation compilation, CancellationToken cancellationToken, out ImmutableArray<INamedTypeSymbol> matcherSymbols)
    {
        matcherSymbols = ImmutableArray<INamedTypeSymbol>.Empty;
        var attribute = GetAttribute(symbol, symbol is INamedTypeSymbol ? "GenerateMethodOverloadsAttribute" : "GenerateOverloadsAttribute");
        var attributeMatchers = ExtractMatcherTypesFromAttribute(attribute, out var attributeSymbols);
        if (!attributeMatchers.IsDefaultOrEmpty)
        {
            matcherSymbols = attributeSymbols;
            return [..attributeMatchers.Select(type => type.ToDisplayString(TypeDisplayFormat))];
        }

        var syntax = GetMemberSyntax(symbol, cancellationToken);
        if (syntax is not null)
        {
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            var syntaxMatchers = ExtractMatcherTypesFromSyntax(syntax, semanticModel, out var syntaxSymbols);
            matcherSymbols = syntaxSymbols;
            return [..syntaxMatchers.Select(type => type.ToDisplayString(TypeDisplayFormat))];
        }

        return ImmutableArray<string>.Empty;
    }

    private static bool HasGenerateOverloadsAttribute(IMethodSymbol methodSymbol, Compilation compilation, CancellationToken cancellationToken)
    {
        if (GetAttribute(methodSymbol, "GenerateOverloadsAttribute") is not null)
        {
            return true;
        }

        var syntax = GetMethodSyntax(methodSymbol, cancellationToken);
        return syntax is not null && HasAttribute(syntax.AttributeLists, "GenerateOverloads");
    }

    private static MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel ExtractGenerateOverloadsArgsFromAttribute(
        AttributeData attribute,
        MethodOverloadsGeneratorCore.SourceLocationModel? attributeLocation,
        MethodOverloadsGeneratorCore.SourceLocationModel? identifierLocation)
    {
        string? beginEnd = null;
        string? begin = null;
        string? beginExclusive = null;
        string? end = null;
        string? endExclusive = null;

        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive &&
            attribute.ConstructorArguments[0].Value is string ctorValue)
        {
            beginEnd = ctorValue;
        }

        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Value.Kind != TypedConstantKind.Primitive || arg.Value.Value is not string value)
            {
                continue;
            }

            if (string.Equals(arg.Key, "Begin", StringComparison.Ordinal))
            {
                begin = value;
            }
            else if (string.Equals(arg.Key, "BeginExclusive", StringComparison.Ordinal))
            {
                beginExclusive = value;
            }
            else if (string.Equals(arg.Key, "End", StringComparison.Ordinal))
            {
                end = value;
            }
            else if (string.Equals(arg.Key, "EndExclusive", StringComparison.Ordinal))
            {
                endExclusive = value;
            }
        }

        return new MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel(
            beginEnd,
            begin,
            beginExclusive,
            end,
            endExclusive,
            attributeLocation,
            identifierLocation,
            null);
    }

    private static MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel? ExtractGenerateOverloadsArgsFromSyntax(
        MethodDeclarationSyntax methodSyntax,
        MethodOverloadsGeneratorCore.SourceLocationModel? identifierLocation)
    {
        foreach (var attributeList in methodSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (!IsAttributeNameMatch(attribute.Name.ToString(), "GenerateOverloads"))
                {
                    continue;
                }

                string? beginEnd = null;
                string? begin = null;
                string? beginExclusive = null;
                string? end = null;
                string? endExclusive = null;

                if (attribute.ArgumentList is not null)
                {
                    foreach (var argument in attribute.ArgumentList.Arguments)
                    {
                        if (argument.NameEquals is null)
                        {
                            if (beginEnd is null)
                            {
                                var positionalValue = GetAttributeStringValue(argument.Expression);
                                if (!string.IsNullOrEmpty(positionalValue))
                                {
                                    beginEnd = positionalValue;
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
                            begin = value;
                        }
                        else if (string.Equals(name, "BeginExclusive", StringComparison.Ordinal))
                        {
                            beginExclusive = value;
                        }
                        else if (string.Equals(name, "End", StringComparison.Ordinal))
                        {
                            end = value;
                        }
                        else if (string.Equals(name, "EndExclusive", StringComparison.Ordinal))
                        {
                            endExclusive = value;
                        }
                    }
                }

                return new MethodOverloadsGeneratorCore.GenerateOverloadsArgsModel(
                    beginEnd,
                    begin,
                    beginExclusive,
                    end,
                    endExclusive,
                    null,
                    identifierLocation,
                    GetLocationModel(attribute));
            }
        }

        return null;
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

    private static MethodOverloadsGeneratorCore.SourceLocationModel? GetMethodIdentifierLocation(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        var syntax = GetMethodSyntax(methodSymbol, cancellationToken);
        return syntax is null ? null : GetLocationModel(syntax.Identifier);
    }

    private static MethodDeclarationSyntax? GetMethodSyntax(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax(cancellationToken) is MethodDeclarationSyntax methodSyntax)
            {
                return methodSyntax;
            }
        }

        return null;
    }

    private static MemberDeclarationSyntax? GetMemberSyntax(ISymbol symbol, CancellationToken cancellationToken)
    {
        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax(cancellationToken) is MemberDeclarationSyntax memberSyntax)
            {
                return memberSyntax;
            }
        }

        return null;
    }

    private static AttributeData? GetAttribute(ISymbol symbol, string attributeName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            var name = attribute.AttributeClass?.Name;
            if (name is null)
            {
                continue;
            }

            if (string.Equals(name, attributeName, StringComparison.Ordinal) ||
                (attributeName.EndsWith("Attribute", StringComparison.Ordinal) &&
                 string.Equals(name, attributeName.Substring(0, attributeName.Length - "Attribute".Length), StringComparison.Ordinal)))
            {
                return attribute;
            }
        }

        return null;
    }

    private static ImmutableArray<INamedTypeSymbol> ExtractMatcherTypesFromAttribute(AttributeData? attribute, out ImmutableArray<INamedTypeSymbol> matcherSymbols)
    {
        matcherSymbols = ImmutableArray<INamedTypeSymbol>.Empty;
        if (attribute is null)
        {
            return ImmutableArray<INamedTypeSymbol>.Empty;
        }

        foreach (var named in attribute.NamedArguments)
        {
            if (!string.Equals(named.Key, "Matchers", StringComparison.Ordinal))
            {
                continue;
            }

            if (named.Value.Kind != TypedConstantKind.Array)
            {
                continue;
            }

            var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
            foreach (var constant in named.Value.Values)
            {
                if (constant.Value is INamedTypeSymbol matcherType)
                {
                    builder.Add(matcherType);
                }
            }

            matcherSymbols = builder.ToImmutable();
            return matcherSymbols;
        }

        return ImmutableArray<INamedTypeSymbol>.Empty;
    }

    private static ImmutableArray<INamedTypeSymbol> ExtractMatcherTypesFromSyntax(
        MemberDeclarationSyntax memberSyntax,
        SemanticModel semanticModel,
        out ImmutableArray<INamedTypeSymbol> matcherSymbols)
    {
        matcherSymbols = ImmutableArray<INamedTypeSymbol>.Empty;

        foreach (var attributeList in memberSyntax.AttributeLists)
        {
            foreach (var attr in attributeList.Attributes)
            {
                if (!IsAttributeNameMatch(attr.Name.ToString(), memberSyntax is BaseTypeDeclarationSyntax ? "GenerateMethodOverloads" : "GenerateOverloads"))
                {
                    continue;
                }

                if (attr.ArgumentList is null)
                {
                    continue;
                }

                foreach (var argument in attr.ArgumentList.Arguments)
                {
                    if (argument.NameEquals is null || !string.Equals(argument.NameEquals.Name.Identifier.ValueText, "Matchers", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var matcherTypes = ExtractTypesFromExpression(argument.Expression, semanticModel);
                    if (matcherTypes.Length > 0)
                    {
                        matcherSymbols = matcherTypes;
                        return matcherTypes;
                    }
                }
            }
        }

        return ImmutableArray<INamedTypeSymbol>.Empty;
    }

    private static ImmutableArray<INamedTypeSymbol> ExtractTypesFromExpression(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        switch (expression)
        {
            case CollectionExpressionSyntax collection:
                foreach (var element in collection.Elements.OfType<ExpressionElementSyntax>())
                {
                    AddTypeFromExpression(element.Expression, semanticModel, builder);
                }
                break;
            case ArrayCreationExpressionSyntax arrayCreation:
                if (arrayCreation.Initializer is not null)
                {
                    foreach (var element in arrayCreation.Initializer.Expressions)
                    {
                        AddTypeFromExpression(element, semanticModel, builder);
                    }
                }
                break;
            case ImplicitArrayCreationExpressionSyntax implicitArray:
                if (implicitArray.Initializer is not null)
                {
                    foreach (var element in implicitArray.Initializer.Expressions)
                    {
                        AddTypeFromExpression(element, semanticModel, builder);
                    }
                }
                break;
            default:
                AddTypeFromExpression(expression, semanticModel, builder);
                break;
        }

        return builder.ToImmutable();
    }

    private static void AddTypeFromExpression(ExpressionSyntax expression, SemanticModel semanticModel, ImmutableArray<INamedTypeSymbol>.Builder builder)
    {
        if (expression is TypeOfExpressionSyntax typeOfExpression)
        {
            var typeSymbol = semanticModel.GetTypeInfo(typeOfExpression.Type).Type as INamedTypeSymbol;
            if (typeSymbol is not null)
            {
                builder.Add(typeSymbol);
            }
        }
    }

    private static MethodOverloadsGeneratorCore.SourceLocationModel GetLocationModel(SyntaxNode node)
    {
        var location = node.GetLocation();
        var lineSpan = location.GetLineSpan();
        return new MethodOverloadsGeneratorCore.SourceLocationModel(
            location.SourceTree?.FilePath ?? string.Empty,
            location.SourceSpan.Start,
            location.SourceSpan.Length,
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line,
            lineSpan.EndLinePosition.Character);
    }

    private static MethodOverloadsGeneratorCore.SourceLocationModel GetLocationModel(SyntaxToken token)
    {
        var location = token.GetLocation();
        var lineSpan = location.GetLineSpan();
        return new MethodOverloadsGeneratorCore.SourceLocationModel(
            location.SourceTree?.FilePath ?? string.Empty,
            location.SourceSpan.Start,
            location.SourceSpan.Length,
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line,
            lineSpan.EndLinePosition.Character);
    }

    private static bool HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                if (string.Equals(name, attributeName, StringComparison.Ordinal) ||
                    string.Equals(name, attributeName + "Attribute", StringComparison.Ordinal))
                {
                    return true;
                }

                if (name.EndsWith("." + attributeName, StringComparison.Ordinal) ||
                    name.EndsWith("." + attributeName + "Attribute", StringComparison.Ordinal))
                {
                    return true;
                }
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

    private static bool TryGetEnumConstant<TEnum>(TypedConstant constant, out TEnum value)
        where TEnum : struct
    {
        value = default;
        if (constant.Value is null)
        {
            return false;
        }

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

    private static ImmutableArray<INamedTypeSymbol> MergeTypes(
        ImmutableArray<INamedTypeSymbol> left,
        ImmutableArray<INamedTypeSymbol> right)
    {
        if (left.IsDefaultOrEmpty && right.IsDefaultOrEmpty)
        {
            return ImmutableArray<INamedTypeSymbol>.Empty;
        }

        var set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var symbol in left)
        {
            set.Add(symbol);
        }

        foreach (var symbol in right)
        {
            set.Add(symbol);
        }

        return [..set];
    }

    private static ImmutableArray<IMethodSymbol> MergeMethods(
        ImmutableArray<IMethodSymbol> left,
        ImmutableArray<IMethodSymbol> right)
    {
        if (left.IsDefaultOrEmpty && right.IsDefaultOrEmpty)
        {
            return ImmutableArray<IMethodSymbol>.Empty;
        }

        var set = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        foreach (var symbol in left)
        {
            set.Add(symbol);
        }

        foreach (var symbol in right)
        {
            set.Add(symbol);
        }

        return [..set];
    }

    private sealed class NamedTypeSymbolArrayComparer : IEqualityComparer<ImmutableArray<INamedTypeSymbol>>
    {
        public static readonly NamedTypeSymbolArrayComparer Instance = new();

        public bool Equals(ImmutableArray<INamedTypeSymbol> x, ImmutableArray<INamedTypeSymbol> y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(ImmutableArray<INamedTypeSymbol> obj)
        {
            var hash = 17;
            foreach (var symbol in obj)
            {
                hash = (hash * 31) + (symbol is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(symbol));
            }

            return hash;
        }
    }

    private sealed class MethodSymbolArrayComparer : IEqualityComparer<ImmutableArray<IMethodSymbol>>
    {
        public static readonly MethodSymbolArrayComparer Instance = new();

        public bool Equals(ImmutableArray<IMethodSymbol> x, ImmutableArray<IMethodSymbol> y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(ImmutableArray<IMethodSymbol> obj)
        {
            var hash = 17;
            foreach (var symbol in obj)
            {
                hash = (hash * 31) + (symbol is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(symbol));
            }

            return hash;
        }
    }
}
