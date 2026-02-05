namespace Tenekon.MethodOverloads.SourceGenerator.Tests.Infrastructure;

public sealed record CaseResult(string ClassName, HashSet<string> ExpectedKeys, HashSet<string> ActualKeys);

public sealed record DiagnosticCaseResult(string ClassName, HashSet<string> ExpectedIds, HashSet<string> ActualIds);