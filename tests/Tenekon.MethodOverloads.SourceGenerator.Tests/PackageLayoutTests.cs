using System.Diagnostics;
using System.IO.Compression;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class PackageLayoutTests
{
    [Fact]
    public void Package_PlacesAnalyzerUnderAnalyzersFolder_AndUsesLibPlaceholder()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var projectPath = Path.Combine(repoRoot, "src", "Tenekon.MethodOverloads.SourceGenerator", "Tenekon.MethodOverloads.SourceGenerator.csproj");
        var outputRoot = Path.Combine(Path.GetTempPath(), "Tenekon.MethodOverloads.SourceGenerator.PackTests", Guid.NewGuid().ToString("N"));
        var outputPath = Path.Combine(outputRoot, "pkgs");

        Directory.CreateDirectory(outputPath);

        try
        {
            var result = RunProcess("dotnet", $"pack \"{projectPath}\" -c Release -p:PackageOutputPath=\"{outputPath}\"");
            Assert.True(result.ExitCode == 0, $"dotnet pack failed with exit code {result.ExitCode}\n{result.Output}");

            var nupkg = Directory.GetFiles(outputPath, "*.nupkg").SingleOrDefault();
            Assert.False(string.IsNullOrWhiteSpace(nupkg), "Expected exactly one .nupkg in the package output directory.");

            using var zip = ZipFile.OpenRead(nupkg!);
            var entries = zip.Entries.Select(e => e.FullName).ToArray();

            Assert.Contains("analyzers/dotnet/cs/Tenekon.MethodOverloads.SourceGenerator.dll", entries);
            Assert.DoesNotContain(entries, e => e.StartsWith("analyzers/dotnet/cs/netstandard", StringComparison.OrdinalIgnoreCase));

            var libEntries = entries.Where(e => e.StartsWith("lib/", StringComparison.OrdinalIgnoreCase)).ToArray();
            Assert.Equal(new[] { "lib/netstandard2.0/_._" }, libEntries);
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, recursive: true);
            }
        }
    }

    private static ProcessResult RunProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, string.Concat(output, error));
    }

    private sealed record ProcessResult(int ExitCode, string Output);
}
