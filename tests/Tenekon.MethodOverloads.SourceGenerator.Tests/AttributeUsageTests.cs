using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator.Tests;

public sealed class AttributeUsageTests
{
    [Fact]
    public void Attribute_usage_matches_acceptance_targets()
    {
        var sources = AcceptanceTestData.LoadReferenceSources();
        var targetsByAttribute = AcceptanceTestData.ExtractAttributeTargets(sources);

        var attributeSources = typeof(GeneratorAttributesSource).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(obj: null)!)
            .ToArray();

        var usageByAttribute =
            new Dictionary<string, HashSet<AcceptanceTestData.AttributeTargetKind>>(StringComparer.Ordinal);
        foreach (var source in attributeSources)
        foreach (var usage in ExtractAttributeUsages(source))
            usageByAttribute[usage.Key] = usage.Value;

        var targetAttributes = new[]
        {
            "GenerateMethodOverloads",
            "GenerateOverloads",
            "OverloadGenerationOptions",
            "MatcherUsage"
        };

        foreach (var attributeName in targetAttributes)
        {
            Assert.True(
                targetsByAttribute.TryGetValue(attributeName, out var expectedTargets),
                $"Expected acceptance criteria targets for {attributeName}.");
            Assert.True(
                usageByAttribute.TryGetValue(attributeName, out var actualTargets),
                $"Expected AttributeUsage for {attributeName}.");

            Assert.Equal(expectedTargets.OrderBy(x => x), actualTargets.OrderBy(x => x));
        }
    }

    private static Dictionary<string, HashSet<AcceptanceTestData.AttributeTargetKind>> ExtractAttributeUsages(
        string source)
    {
        var results = new Dictionary<string, HashSet<AcceptanceTestData.AttributeTargetKind>>(StringComparer.Ordinal);
        var nameMatch = Regex.Match(source, @"\bsealed\s+class\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)Attribute\b");
        if (!nameMatch.Success) return results;

        var attributeName = nameMatch.Groups["name"].Value;
        var usageMatch = Regex.Match(source, @"AttributeUsage\((?<args>[^)]*)\)");
        if (!usageMatch.Success) return results;

        var args = usageMatch.Groups["args"].Value;
        var targets = new HashSet<AcceptanceTestData.AttributeTargetKind>();

        if (args.Contains("AttributeTargets.Class", StringComparison.Ordinal))
            targets.Add(AcceptanceTestData.AttributeTargetKind.Class);
        if (args.Contains("AttributeTargets.Struct", StringComparison.Ordinal))
            targets.Add(AcceptanceTestData.AttributeTargetKind.Struct);
        if (args.Contains("AttributeTargets.Interface", StringComparison.Ordinal))
            targets.Add(AcceptanceTestData.AttributeTargetKind.Interface);
        if (args.Contains("AttributeTargets.Method", StringComparison.Ordinal))
            targets.Add(AcceptanceTestData.AttributeTargetKind.Method);

        results[attributeName] = targets;
        return results;
    }
}