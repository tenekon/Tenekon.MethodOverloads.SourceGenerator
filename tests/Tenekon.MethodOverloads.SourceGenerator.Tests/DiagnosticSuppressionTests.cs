using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class DiagnosticSuppressionTests
{
    private const string InvalidAnchorMethod = """
        [GenerateOverloads(Begin = "missing")]
        public static void Method(int value) { }
        """;

    [Fact]
    public void Diagnostic_is_reported_unsuppressed_by_default()
    {
        var diagnostic = Assert.Single(GetDiagnostics(CreateTargetSource(InvalidAnchorMethod)), IsMog001);

        Assert.False(diagnostic.IsSuppressed);
    }

    [Fact]
    public void Pragma_warning_disable_suppresses_diagnostic()
    {
        var source = "#pragma warning disable MOG001\n" + CreateTargetSource(InvalidAnchorMethod);

        var diagnostic = Assert.Single(GetDiagnostics(source), IsMog001);

        Assert.True(diagnostic.IsSuppressed);
    }

    [Fact]
    public void SuppressMessage_attribute_on_method_suppresses_diagnostic()
    {
        var source = CreateTargetSource(
            """
            [System.Diagnostics.CodeAnalysis.SuppressMessage("MethodOverloadsGenerator", "MOG001")]
            """ + "\n" + InvalidAnchorMethod);

        var diagnostic = Assert.Single(GetDiagnostics(source), IsMog001);

        Assert.True(diagnostic.IsSuppressed);
    }

    [Fact]
    public void Diagnostic_location_is_in_source_and_spans_the_attribute()
    {
        var diagnostic = Assert.Single(GetDiagnostics(CreateTargetSource(InvalidAnchorMethod)), IsMog001);

        Assert.True(diagnostic.Location.IsInSource);
        Assert.Equal("Target.cs", diagnostic.Location.SourceTree!.FilePath);
        Assert.Equal("GenerateOverloads(Begin = \"missing\")", GetLocationText(diagnostic.Location));
    }

    [Fact]
    public void Matcher_without_match_is_reported_at_typeof_reference_of_target()
    {
        const string source = """
            using Tenekon.MethodOverloads;

            namespace Demo;

            [OverloadMatcher]
            internal interface IMatcher
            {
                [GenerateOverloads(nameof(value))]
                void Match(Guid value);
            }

            [GenerateMethodOverloads(Matchers = [typeof(IMatcher)])]
            public sealed class Target
            {
                public void Method(int value) { }
            }
            """;

        var diagnostic = Assert.Single(GetDiagnostics(source), d => d.Id == "MOG002");

        Assert.True(diagnostic.Location.IsInSource);
        Assert.Equal("typeof(IMatcher)", GetLocationText(diagnostic.Location));
        Assert.Equal(
            "Matcher 'Match' has no subsequence match for any target method in 'Target'",
            diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    private static bool IsMog001(Diagnostic diagnostic)
    {
        return diagnostic.Id == "MOG001";
    }

    private static string CreateTargetSource(string members)
    {
        return $$"""
            using Tenekon.MethodOverloads;

            namespace Demo;

            public static class Target
            {
            {{members}}
            }
            """;
    }

    private static string GetLocationText(Location location)
    {
        return location.SourceTree!.GetText().ToString(location.SourceSpan);
    }

    private static ImmutableArray<Diagnostic> GetDiagnostics(string source)
    {
        var compilation = AcceptanceTestData.CreateCompilation([new AcceptanceTestData.SourceFile("Target.cs", source)]);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new MethodOverloadsGenerator().AsSourceGenerator()],
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var exceptions = new ConcurrentQueue<Exception>();
        var options = new CompilationWithAnalyzersOptions(
            new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty),
            onAnalyzerException: (exception, _, _) => exceptions.Enqueue(exception),
            concurrentAnalysis: true,
            logAnalyzerExecutionTime: false,
            reportSuppressedDiagnostics: true);
        var diagnostics = outputCompilation
            .WithAnalyzers([new MethodOverloadsDiagnosticsAnalyzer()], options)
            .GetAnalyzerDiagnosticsAsync()
            .GetAwaiter()
            .GetResult();

        Assert.Empty(exceptions);
        return diagnostics;
    }
}
