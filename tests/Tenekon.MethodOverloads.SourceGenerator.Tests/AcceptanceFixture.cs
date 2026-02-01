using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Tenekon.MethodOverloads.SourceGenerator;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

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
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, parseOptions: new CSharpParseOptions(LanguageVersion.Preview));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        var runResult = driver.GetRunResult();
        Diagnostics = runResult.Diagnostics;

        var generatedTrees = outputCompilation.SyntaxTrees
            .Where(tree => tree.FilePath.Contains(".g.cs", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        HasGenerateOverloadsAttribute = generatedTrees.Any(tree => tree.ToString().Contains("class GenerateOverloadsAttribute", StringComparison.Ordinal));
        HasGenerateMethodOverloadsAttribute = generatedTrees.Any(tree => tree.ToString().Contains("class GenerateMethodOverloadsAttribute", StringComparison.Ordinal));

        var actual = AcceptanceTestData.ExtractActualSignatures(outputCompilation, generatedTrees);
        Cases = AcceptanceTestData.BuildCaseResults(expected, actual);
        DiagnosticCases = AcceptanceTestData.BuildDiagnosticResults(ExpectedDiagnostics, Diagnostics);
    }

    public bool HasGenerateOverloadsAttribute { get; }
    public bool HasGenerateMethodOverloadsAttribute { get; }
    public IReadOnlyList<CaseResult> Cases { get; }
    internal ImmutableArray<AcceptanceTestData.ExpectedDiagnostic> ExpectedDiagnostics { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public IReadOnlyList<DiagnosticCaseResult> DiagnosticCases { get; }
}
