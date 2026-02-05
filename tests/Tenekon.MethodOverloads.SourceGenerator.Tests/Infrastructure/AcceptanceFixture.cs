using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

public sealed class AcceptanceFixture
{
    public AcceptanceFixture()
    {
        var referenceSources = AcceptanceTestData.LoadReferenceSources();
        var expected = AcceptanceTestData.ExtractExpectedSignatures(referenceSources);
        ExpectedDiagnostics = AcceptanceTestData.ExtractExpectedDiagnostics(referenceSources);

        var generatorSources = AcceptanceTestData.PrepareGeneratorInputs(referenceSources);
        var compilation = AcceptanceTestData.CreateCompilation(generatorSources);
        var generator = new MethodOverloadsGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        var generatedTrees = outputCompilation.SyntaxTrees
            .Where(tree => tree.FilePath.Contains(".g.cs", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        GeneratedTrees = generatedTrees;

        HasGenerateOverloadsAttribute = generatedTrees.Any(tree => tree.ToString()
            .Contains("class GenerateOverloadsAttribute", StringComparison.Ordinal));
        HasGenerateMethodOverloadsAttribute = generatedTrees.Any(tree => tree.ToString()
            .Contains("class GenerateMethodOverloadsAttribute", StringComparison.Ordinal));

        var actual = AcceptanceTestData.ExtractActualSignatures(outputCompilation, generatedTrees);
        Cases = AcceptanceTestData.BuildCaseResults(expected, actual);
        Diagnostics = GetAnalyzerDiagnostics(outputCompilation);
        DiagnosticCases = AcceptanceTestData.BuildDiagnosticResults(ExpectedDiagnostics, Diagnostics);
    }

    public bool HasGenerateOverloadsAttribute { get; }
    public bool HasGenerateMethodOverloadsAttribute { get; }
    public IReadOnlyList<SyntaxTree> GeneratedTrees { get; }
    public IReadOnlyList<CaseResult> Cases { get; }
    internal ImmutableArray<AcceptanceTestData.ExpectedDiagnostic> ExpectedDiagnostics { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public IReadOnlyList<DiagnosticCaseResult> DiagnosticCases { get; }

    private static ImmutableArray<Diagnostic> GetAnalyzerDiagnostics(Compilation compilation)
    {
        var analyzer = new MethodOverloadsDiagnosticsAnalyzer();
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);
        var diagnostics = compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        return diagnostics;
    }
}