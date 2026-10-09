using System.Diagnostics;
using System.IO.Compression;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

public sealed class PackageLayoutFixture : IDisposable
{
    private readonly string _outputRoot;

    public PackageLayoutFixture()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var projectPath = Path.Combine(
            repoRoot,
            "src",
            "Tenekon.MethodOverloads.SourceGenerator",
            "Tenekon.MethodOverloads.SourceGenerator.csproj");

        _outputRoot = Path.Combine(
            Path.GetTempPath(),
            "Tenekon.MethodOverloads.SourceGenerator.PackTests",
            Guid.NewGuid().ToString("N"));
        var outputPath = Path.Combine(_outputRoot, "pkgs");

        Directory.CreateDirectory(outputPath);

        var result = RunProcess("dotnet", $"pack \"{projectPath}\" -c Release -p:PackageOutputPath=\"{outputPath}\"");
        Assert.True(result.ExitCode == 0, $"dotnet pack failed with exit code {result.ExitCode}\n{result.Output}");

        var nupkg = Directory.GetFiles(outputPath, "*.nupkg").SingleOrDefault();
        Assert.False(string.IsNullOrWhiteSpace(nupkg), "Expected exactly one .nupkg in the package output directory.");

        using var zip = ZipFile.OpenRead(nupkg!);
        Entries = zip.Entries.Select(e => e.FullName).ToArray();
    }

    public string[] Entries { get; }

    public void Dispose()
    {
        if (Directory.Exists(_outputRoot)) Directory.Delete(_outputRoot, recursive: true);
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

        // Reused MSBuild nodes outlive the build and inherit the redirected pipes, so reading
        // stdout/stderr to the end would block until their idle timeout expires.
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        // Drain both streams concurrently; reading them one after another can deadlock once the
        // other pipe's buffer is full.
        var error = process.StandardError.ReadToEndAsync();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, string.Concat(output, error.Result));
    }

    private sealed record ProcessResult(int ExitCode, string Output);
}