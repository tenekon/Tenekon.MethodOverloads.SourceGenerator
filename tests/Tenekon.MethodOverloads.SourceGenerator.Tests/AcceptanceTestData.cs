using System.Collections.Immutable;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

internal static class AcceptanceTestData
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    private static readonly SymbolDisplayFormat TypeDisplayFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static ImmutableArray<SourceFile> LoadReferenceSources()
    {
        var refFolder = Path.Combine(RepoRoot, "ref", "Tenekon.MethodOverloads.AcceptanceCriterias");
        var files = Directory.GetFiles(refFolder, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(path => !path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        var builder = ImmutableArray.CreateBuilder<SourceFile>();
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            builder.Add(new SourceFile(file, text));
        }

        return builder.ToImmutable();
    }

    internal static ImmutableArray<ExpectedDiagnostic> ExtractExpectedDiagnostics(ImmutableArray<SourceFile> sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var trees = sources.Select(source => CSharpSyntaxTree.ParseText(source.Content, parseOptions, source.Path)).ToArray();
        var expected = new List<ExpectedDiagnostic>();

        foreach (var tree in trees)
        {
            var root = tree.GetRoot();
            foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
            {
                if (!IsAttributeNameMatch(attribute.Name.ToString(), "SuppressMessage"))
                {
                    continue;
                }

                if (attribute.ArgumentList is null)
                {
                    continue;
                }

                if (!TryGetSuppressMessageId(attribute, out var diagnosticId))
                {
                    continue;
                }

                if (!diagnosticId.StartsWith("MOG", StringComparison.Ordinal))
                {
                    continue;
                }

                var classDecl = attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDecl is null)
                {
                    continue;
                }

                expected.Add(new ExpectedDiagnostic(classDecl.Identifier.ValueText, diagnosticId));
            }
        }

        return expected.ToImmutableArray();
    }

    internal static ImmutableArray<ExpectedSignature> ExtractExpectedSignatures(ImmutableArray<SourceFile> sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var trees = sources.Select(source => CSharpSyntaxTree.ParseText(source.Content, parseOptions, source.Path)).ToArray();
        var compilation = CreateCompilation(trees);

        var expected = new List<ExpectedSignature>();

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
            var root = tree.GetRoot();

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var name = classDecl.Identifier.ValueText;
                if (!name.EndsWith("AcceptanceCriterias", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    var symbol = model.GetDeclaredSymbol(method) as IMethodSymbol;
                    if (symbol is null || symbol.Parameters.Length == 0)
                    {
                        continue;
                    }

                    var receiver = symbol.Parameters[0].Type.ToDisplayString(TypeDisplayFormat);
                    var parameters = symbol.Parameters.Skip(1).Select(ToParameterSignature).ToImmutableArray();
                    expected.Add(new ExpectedSignature(
                        KeyFrom(symbol.Name, receiver, symbol.TypeParameters.Length, parameters, symbol.DeclaredAccessibility, "classic"),
                        symbol.Name,
                        receiver,
                        symbol.TypeParameters.Length,
                        symbol.DeclaredAccessibility,
                        parameters,
                        "classic"));
                }
            }

            expected.AddRange(ExtractExtensionBlockSignatures(tree.ToString()));
        }

        return expected.ToImmutableArray();
    }

    public static ImmutableArray<SourceFile> PrepareGeneratorInputs(ImmutableArray<SourceFile> sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var filtered = new List<SourceFile>();

        foreach (var source in sources)
        {
            var fileName = Path.GetFileName(source.Path);
            if (string.Equals(fileName, "GenerateOverloadsAttribute.cs", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, "GenerateMethodOverloadsAttribute.cs", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, "OverloadGenerationOptions.cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var tree = CSharpSyntaxTree.ParseText(source.Content, parseOptions, source.Path);
            var root = tree.GetRoot();

            var toRemove = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(cls => cls.Identifier.ValueText.EndsWith("AcceptanceCriterias", StringComparison.Ordinal))
                .ToArray();

            if (toRemove.Length > 0)
            {
                root = root.RemoveNodes(toRemove, SyntaxRemoveOptions.KeepNoTrivia)!;
            }

            filtered.Add(new SourceFile(source.Path, root.NormalizeWhitespace().ToFullString()));
        }

        return filtered.ToImmutableArray();
    }

    internal static ImmutableArray<ExpectedSignature> ExtractActualSignatures(Compilation compilation, SyntaxTree[] generatedTrees)
    {
        var results = new List<ExpectedSignature>();

        foreach (var tree in generatedTrees)
        {
            var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
            var root = tree.GetRoot();

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(method) as IMethodSymbol;
                if (symbol is null)
                {
                    continue;
                }

                if (method.ParameterList.Parameters.Count > 0 && method.ParameterList.Parameters[0].Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)))
                {
                    var receiver = symbol.Parameters[0].Type.ToDisplayString(TypeDisplayFormat);
                    var parameters = symbol.Parameters.Skip(1).Select(ToParameterSignature).ToImmutableArray();
                    results.Add(new ExpectedSignature(
                        KeyFrom(symbol.Name, receiver, symbol.TypeParameters.Length, parameters, symbol.DeclaredAccessibility, "classic"),
                        symbol.Name,
                        receiver,
                        symbol.TypeParameters.Length,
                        symbol.DeclaredAccessibility,
                        parameters,
                        "classic"));
                }
            }

            results.AddRange(ExtractExtensionBlockSignatures(tree.ToString()));
        }

        return results.ToImmutableArray();
    }

    internal static IReadOnlyList<CaseResult> BuildCaseResults(
        ImmutableArray<ExpectedSignature> expected,
        ImmutableArray<ExpectedSignature> actual)
    {
        var expectedByClass = expected
            .GroupBy(entry => GetReceiverName(entry.Receiver), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(e => e.Key).ToHashSet(StringComparer.Ordinal), StringComparer.Ordinal);

        var actualByClass = actual
            .GroupBy(entry => GetReceiverName(entry.Receiver), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(e => e.Key).ToHashSet(StringComparer.Ordinal), StringComparer.Ordinal);

        var classNames = expectedByClass.Keys.Union(actualByClass.Keys, StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var cases = new List<CaseResult>();
        foreach (var className in classNames)
        {
            expectedByClass.TryGetValue(className, out var expectedKeys);
            actualByClass.TryGetValue(className, out var actualKeys);

            cases.Add(new CaseResult(
                className,
                expectedKeys ?? new HashSet<string>(StringComparer.Ordinal),
                actualKeys ?? new HashSet<string>(StringComparer.Ordinal)));
        }

        return cases;
    }

    internal static IReadOnlyList<DiagnosticCaseResult> BuildDiagnosticResults(
        ImmutableArray<ExpectedDiagnostic> expected,
        IEnumerable<Diagnostic> actualDiagnostics)
    {
        var expectedByClass = expected
            .GroupBy(entry => entry.ClassName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(e => e.Id).ToHashSet(StringComparer.Ordinal), StringComparer.Ordinal);

        var actualByClass = actualDiagnostics
            .Where(diag => diag.Id.StartsWith("MOG", StringComparison.Ordinal))
            .Select(diag => new { diag.Id, ClassName = GetClassNameFromDiagnostic(diag) })
            .Where(entry => !string.IsNullOrEmpty(entry.ClassName))
            .GroupBy(entry => entry.ClassName!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(e => e.Id).ToHashSet(StringComparer.Ordinal), StringComparer.Ordinal);

        var classNames = expectedByClass.Keys.Union(actualByClass.Keys, StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var cases = new List<DiagnosticCaseResult>();
        foreach (var className in classNames)
        {
            expectedByClass.TryGetValue(className, out var expectedIds);
            actualByClass.TryGetValue(className, out var actualIds);

            cases.Add(new DiagnosticCaseResult(
                className,
                expectedIds ?? new HashSet<string>(StringComparer.Ordinal),
                actualIds ?? new HashSet<string>(StringComparer.Ordinal)));
        }

        return cases;
    }


    public static CSharpCompilation CreateCompilation(IEnumerable<SourceFile> sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var trees = sources.Select(source => CSharpSyntaxTree.ParseText(source.Content, parseOptions, source.Path)).ToList();
        trees.Add(CSharpSyntaxTree.ParseText("global using System;", parseOptions, path: "ImplicitUsings.g.cs"));
        return CreateCompilation(trees);
    }

    public static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> trees)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CancellationToken).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.ExtensionAttribute).Assembly.Location)
        };

        var runtimeAssembly = Assembly.Load("System.Runtime");
        references.Add(MetadataReference.CreateFromFile(runtimeAssembly.Location));

        return CSharpCompilation.Create(
            "AcceptanceCriteria",
            trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    private static ParameterSignature ToParameterSignature(IParameterSymbol parameter)
    {
        return new ParameterSignature(
            parameter.Type.ToDisplayString(TypeDisplayFormat),
            parameter.RefKind,
            parameter.IsParams);
    }

    private static string KeyFrom(string name, string receiver, int arity, ImmutableArray<ParameterSignature> parameters, Accessibility accessibility, string kind)
    {
        var parts = new List<string>
        {
            kind,
            name,
            receiver,
            arity.ToString(),
            accessibility.ToString()
        };

        foreach (var parameter in parameters)
        {
            parts.Add(parameter.Type);
            parts.Add(parameter.RefKind.ToString());
            parts.Add(parameter.IsParams ? "params" : "noparams");
        }

        return string.Join("|", parts);
    }

    private static IEnumerable<ExpectedSignature> ExtractExtensionBlockSignatures(string text)
    {
        var results = new List<ExpectedSignature>();
        var extensionRegex = new Regex(@"extension\s*\(([^)]+)\)\s*\{(.*?)\}", RegexOptions.Singleline | RegexOptions.Compiled);
        var methodRegex = new Regex(@"(public|internal|private|protected)\s+static\s+([^\s]+)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)", RegexOptions.Compiled);

        foreach (Match extensionMatch in extensionRegex.Matches(text))
        {
            var receiverType = NormalizeType(extensionMatch.Groups[1].Value);
            var body = extensionMatch.Groups[2].Value;

            foreach (Match methodMatch in methodRegex.Matches(body))
            {
                var accessibilityText = methodMatch.Groups[1].Value;
                var methodName = methodMatch.Groups[3].Value;
                var parametersText = methodMatch.Groups[4].Value;

                var parameters = ParseParameterSignatures(parametersText);
                var accessibility = ParseAccessibility(accessibilityText);

                results.Add(new ExpectedSignature(
                    KeyFrom(methodName, receiverType, 0, parameters, accessibility, "extension"),
                    methodName,
                    receiverType,
                    0,
                    accessibility,
                    parameters,
                    "extension"));
            }
        }

        return results;
    }

    private static ImmutableArray<ParameterSignature> ParseParameterSignatures(string parametersText)
    {
        if (string.IsNullOrWhiteSpace(parametersText))
        {
            return ImmutableArray<ParameterSignature>.Empty;
        }

        var parameters = parametersText.Split(',')
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .Select(ParseParameterSignature)
            .ToImmutableArray();

        return parameters;
    }

    private static ParameterSignature ParseParameterSignature(string text)
    {
        var tokens = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var index = 0;
        var isParams = false;
        var refKind = RefKind.None;

        if (tokens[index] == "params")
        {
            isParams = true;
            index++;
        }

        if (tokens[index] == "ref" || tokens[index] == "out" || tokens[index] == "in")
        {
            refKind = tokens[index] switch
            {
                "ref" => RefKind.Ref,
                "out" => RefKind.Out,
                "in" => RefKind.In,
                _ => RefKind.None
            };
            index++;
        }

        var type = NormalizeType(tokens[index]);
        return new ParameterSignature(type, refKind, isParams);
    }

    private static string NormalizeType(string typeName)
    {
        var trimmed = typeName.Trim();
        var isNullable = trimmed.EndsWith("?", StringComparison.Ordinal);
        if (isNullable)
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 1);
        }

        if (trimmed.StartsWith("global::", StringComparison.Ordinal))
        {
            trimmed = trimmed.Substring("global::".Length);
        }

        if (trimmed.Contains('.'))
        {
            trimmed = trimmed.Split('.').Last();
        }

        var normalized = trimmed switch
        {
            "int" => "Int32",
            "string" => "String",
            "bool" => "Boolean",
            "object" => "Object",
            "double" => "Double",
            "float" => "Single",
            "long" => "Int64",
            "short" => "Int16",
            "byte" => "Byte",
            "char" => "Char",
            _ => trimmed
        };

        return isNullable ? normalized + "?" : normalized;
    }

    private static Accessibility ParseAccessibility(string text)
    {
        return text switch
        {
            "public" => Accessibility.Public,
            "internal" => Accessibility.Internal,
            "private" => Accessibility.Private,
            "protected" => Accessibility.Protected,
            _ => Accessibility.Public
        };
    }

    private static string GetReceiverName(string receiver)
    {
        var name = receiver.Trim();
        if (name.StartsWith("global::", StringComparison.Ordinal))
        {
            name = name.Substring("global::".Length);
        }

        if (name.Contains("::"))
        {
            var parts = name.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            name = parts.Length > 0 ? parts[parts.Length - 1] : name;
        }

        if (name.Contains('.'))
        {
            name = name.Split('.').Last();
        }

        return name;
    }

    private static bool TryGetSuppressMessageId(AttributeSyntax attribute, out string id)
    {
        id = string.Empty;
        string? checkId = null;
        var positionalIndex = 0;

        foreach (var argument in attribute.ArgumentList!.Arguments)
        {
            var value = argument.Expression as LiteralExpressionSyntax;
            if (value is null || !value.IsKind(SyntaxKind.StringLiteralExpression))
            {
                continue;
            }

            if (argument.NameEquals is null)
            {
                if (positionalIndex == 1)
                {
                    checkId ??= value.Token.ValueText;
                }

                positionalIndex++;
            }
            else
            {
                var name = argument.NameEquals.Name.Identifier.ValueText;
                if (string.Equals(name, "CheckId", StringComparison.Ordinal))
                {
                    checkId = value.Token.ValueText;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(checkId))
        {
            return false;
        }

        var index = checkId.IndexOf(':');
        if (index >= 0)
        {
            checkId = checkId.Substring(0, index);
        }

        id = checkId;
        return true;
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

    private static string? GetClassNameFromDiagnostic(Diagnostic diagnostic)
    {
        var location = diagnostic.Location;
        if (location == Location.None || location.SourceTree is null)
        {
            return null;
        }

        var root = location.SourceTree.GetRoot();
        var node = root.FindNode(location.SourceSpan, getInnermostNodeForTie: true);
        var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        return classDecl?.Identifier.ValueText;
    }

    public sealed record SourceFile(string Path, string Content);

    internal sealed record ParameterSignature(string Type, RefKind RefKind, bool IsParams);

    internal sealed record ExpectedSignature(
        string Key,
        string Name,
        string Receiver,
        int Arity,
        Accessibility Accessibility,
        ImmutableArray<ParameterSignature> Parameters,
        string Kind);

    internal sealed record ExpectedDiagnostic(string ClassName, string Id);
}
