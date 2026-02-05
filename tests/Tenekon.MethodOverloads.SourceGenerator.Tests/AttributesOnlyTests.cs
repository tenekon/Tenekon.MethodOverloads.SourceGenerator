using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class AttributesOnlyTests
{
    [Fact]
    public void Attributes_only_mode_emits_no_overloads()
    {
        var source = """
            using Tenekon.MethodOverloads.SourceGenerator;
            namespace Demo;

            public sealed class Sample
            {
                [GenerateOverloads]
                public void Test(int value, string? text) { }
            }
            """;

        var compilation = AcceptanceTestData.CreateCompilation(
            new[]
            {
                CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), "Sample.cs")
            });

        var generator = new MethodOverloadsGenerator();
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(
            new Dictionary<string, string>
            {
                ["build_property.TenekonMethodOverloadsSourceGeneratorAttributesOnly"] = "true"
            });

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var generatedTrees = outputCompilation.SyntaxTrees
            .Where(tree => tree.FilePath.Contains(".g.cs", StringComparison.OrdinalIgnoreCase))
            .Select(tree => tree.ToString())
            .ToArray();

        Assert.Contains(
            generatedTrees,
            tree => tree.Contains("class GenerateOverloadsAttribute", StringComparison.Ordinal));
        Assert.Contains(
            generatedTrees,
            tree => tree.Contains("class GenerateMethodOverloadsAttribute", StringComparison.Ordinal));
        Assert.Contains(
            generatedTrees,
            tree => tree.Contains("class OverloadGenerationOptionsAttribute", StringComparison.Ordinal));
        Assert.Contains(generatedTrees, tree => tree.Contains("class MatcherUsageAttribute", StringComparison.Ordinal));
        Assert.DoesNotContain(
            generatedTrees,
            tree => tree.Contains("public static class MethodOverloads", StringComparison.Ordinal));
    }

    private sealed class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> values)
        : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(values);

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return _globalOptions;
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return _globalOptions;
        }
    }

    private sealed class TestAnalyzerConfigOptions(Dictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            return values.TryGetValue(key, out value!);
        }
    }
}
