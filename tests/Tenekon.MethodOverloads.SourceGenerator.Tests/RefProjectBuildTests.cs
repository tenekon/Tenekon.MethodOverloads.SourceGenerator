using System.Diagnostics;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

[Collection("Projects")]
public sealed class RefProjectBuildTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void AcceptanceCriterias_project_builds_with_generator_attributes_only()
    {
        var projectPath = Path.Combine(
            RepoRoot,
            "ref",
            "Tenekon.MethodOverloads.AcceptanceCriterias",
            "Tenekon.MethodOverloads.AcceptanceCriterias.csproj");
        Assert.True(File.Exists(projectPath), "AcceptanceCriterias project file not found.");

        var result = RunProcess(
            "dotnet",
            $"build \"{projectPath}\" -c Release",
            RepoRoot,
            TimeSpan.FromMinutes(value: 2));

        Assert.True(
            result.ExitCode == 0,
            $"dotnet build failed.\nExitCode: {result.ExitCode}\nStdOut:\n{result.StdOut}\nStdErr:\n{result.StdErr}");
    }

    [Fact]
    public void AcceptanceCriterias_project_builds_with_generator_attributes_only_disabled()
    {
        var projectPath = Path.Combine(
            RepoRoot,
            "ref",
            "Tenekon.MethodOverloads.AcceptanceCriterias",
            "Tenekon.MethodOverloads.AcceptanceCriterias.csproj");
        Assert.True(File.Exists(projectPath), "AcceptanceCriterias project file not found.");

        var result = RunProcess(
            "dotnet",
            $"build \"{projectPath}\" -c Release /p:TenekonMethodOverloadsSourceGeneratorAttributesOnly=false",
            RepoRoot,
            TimeSpan.FromMinutes(value: 2));

        Assert.True(
            result.ExitCode == 0,
            $"dotnet build failed.\nExitCode: {result.ExitCode}\nStdOut:\n{result.StdOut}\nStdErr:\n{result.StdErr}");
    }

    private static ProcessResult RunProcess(
        string fileName,
        string arguments,
        string workingDirectory,
        TimeSpan timeout)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var stdOut = process.StandardOutput.ReadToEndAsync();
        var stdErr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignore shutdown errors; we'll report timeout as failure.
            }

            return new ProcessResult(ExitCode: -1, "Process timed out.", string.Empty);
        }

        Task.WaitAll(stdOut, stdErr);
        return new ProcessResult(process.ExitCode, stdOut.Result, stdErr.Result);
    }

    private sealed record ProcessResult(int ExitCode, string StdOut, string StdErr);
}