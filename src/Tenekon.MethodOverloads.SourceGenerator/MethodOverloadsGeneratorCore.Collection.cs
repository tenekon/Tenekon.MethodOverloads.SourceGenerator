using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    /// <summary>
    /// Collects type/method symbols and matcher metadata from the compilation.
    /// </summary>
    private void CollectTypeContexts()
    {
        foreach (var tree in _compilation.SyntaxTrees)
        {
            var semanticModel = _compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            foreach (var typeDecl in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol typeSymbol)
                {
                    continue;
                }

                if (!_typeContexts.ContainsKey(typeSymbol))
                {
                    _typeContexts[typeSymbol] = new TypeContext(typeSymbol);
                }

                var typeContext = _typeContexts[typeSymbol];

                var generateMethodOverloads = GetAttribute(typeSymbol, GenerateMethodOverloadsAttributeName);
                var hasGenerateMethodOverloadsSyntax = HasAttribute(typeDecl, "GenerateMethodOverloads");
                if (generateMethodOverloads is not null || hasGenerateMethodOverloadsSyntax)
                {
                    typeContext.GenerateMethodOverloadsAttribute = generateMethodOverloads;
                    typeContext.HasGenerateMethodOverloads = true;
                    typeContext.MatcherTypes = ExtractMatchers(generateMethodOverloads, typeDecl, semanticModel);

                    foreach (var matcher in typeContext.MatcherTypes)
                    {
                        _matcherTypes.Add(matcher);
                    }
                }

                if (typeDecl is TypeDeclarationSyntax typeDeclaration)
                {
                    foreach (var methodDecl in typeDeclaration.Members.OfType<MethodDeclarationSyntax>())
                    {
                        if (semanticModel.GetDeclaredSymbol(methodDecl) is not IMethodSymbol methodSymbol)
                        {
                            continue;
                        }

                        typeContext.Methods.Add(methodSymbol);
                        typeContext.MethodSyntax[methodSymbol] = methodDecl;

                        var methodMatchers = ExtractMatchersFromSyntax(methodDecl, semanticModel, "GenerateOverloads");
                        foreach (var matcher in methodMatchers)
                        {
                            _matcherTypes.Add(matcher);
                        }
                    }
                }
            }
        }
    }
    private AttributeData? GetAttribute(ISymbol symbol, string attributeName)
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
    private static ImmutableArray<INamedTypeSymbol> ExtractMatchers(AttributeData? attribute, BaseTypeDeclarationSyntax typeDecl, SemanticModel semanticModel)
    {
        var fromAttribute = ExtractMatchers(attribute);
        if (!fromAttribute.IsEmpty)
        {
            return fromAttribute;
        }

        return ExtractMatchersFromSyntax(typeDecl, semanticModel, "GenerateMethodOverloads");
    }
    private ImmutableArray<INamedTypeSymbol> ExtractMatchers(AttributeData? attribute, MethodDeclarationSyntax methodSyntax)
    {
        var fromAttribute = ExtractMatchers(attribute);
        if (!fromAttribute.IsEmpty)
        {
            return fromAttribute;
        }

        var semanticModel = _compilation.GetSemanticModel(methodSyntax.SyntaxTree);
        return ExtractMatchersFromSyntax(methodSyntax, semanticModel, "GenerateOverloads");
    }
    private static ImmutableArray<INamedTypeSymbol> ExtractMatchers(AttributeData? attribute)
    {
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

            return builder.ToImmutable();
        }

        return ImmutableArray<INamedTypeSymbol>.Empty;
    }
    private static ImmutableArray<INamedTypeSymbol> ExtractMatchersFromSyntax(
        MemberDeclarationSyntax memberSyntax,
        SemanticModel semanticModel,
        string attributeName)
    {
        foreach (var attributeList in memberSyntax.AttributeLists)
        {
            foreach (var attr in attributeList.Attributes)
            {
                if (!IsAttributeNameMatch(attr.Name.ToString(), attributeName))
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
    private static bool HasAttribute(MemberDeclarationSyntax memberSyntax, string attributeName)
    {
        foreach (var attributeList in memberSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (IsAttributeNameMatch(attribute.Name.ToString(), attributeName))
                {
                    return true;
                }
            }
        }

        return false;
    }
}

