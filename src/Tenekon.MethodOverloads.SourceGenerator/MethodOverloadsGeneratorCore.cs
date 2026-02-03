using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    private static readonly SymbolDisplayFormat TypeDisplayFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    private static readonly SymbolDisplayFormat SignatureDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat;
    private const string GenerateOverloadsAttributeName = "GenerateOverloadsAttribute";
    private const string GenerateMethodOverloadsAttributeName = "GenerateMethodOverloadsAttribute";
    private const string OverloadGenerationOptionsAttributeName = "OverloadGenerationOptionsAttribute";

    private readonly GeneratorInputModel _input;
    private readonly SourceProductionContext _context;
    private readonly CancellationToken _cancellationToken;
    private readonly IReadOnlyDictionary<string, SyntaxTree> _syntaxTreesByPath;
    private readonly Dictionary<string, TypeModel> _typesByDisplay;
    private readonly Dictionary<string, TypeTargetModel> _typeTargetsByDisplay;
    private readonly Dictionary<string, MatcherTypeModel> _matcherTypesByDisplay;
    private readonly HashSet<string> _matcherTypeDisplays;
    private readonly Dictionary<string, List<GeneratedMethod>> _methodsByNamespace;
    private readonly Dictionary<string, HashSet<MatcherMethodReference>> _matchedMatchersByNamespace;

    public MethodOverloadsGeneratorCore(
        GeneratorInputModel input,
        SourceProductionContext context,
        IReadOnlyDictionary<string, SyntaxTree> syntaxTreesByPath)
    {
        _context = context;
        _input = input;
        _cancellationToken = context.CancellationToken;
        _syntaxTreesByPath = syntaxTreesByPath;
        _typesByDisplay = input.Types.Items.ToDictionary(type => type.DisplayName, type => type, StringComparer.Ordinal);
        _typeTargetsByDisplay = input.TypeTargets.Items.ToDictionary(target => target.Type.DisplayName, target => target, StringComparer.Ordinal);
        _matcherTypesByDisplay = input.MatcherTypes.Items.ToDictionary(target => target.Type.DisplayName, target => target, StringComparer.Ordinal);
        _matcherTypeDisplays = new HashSet<string>(_matcherTypesByDisplay.Keys, StringComparer.Ordinal);
        _methodsByNamespace = new Dictionary<string, List<GeneratedMethod>>(StringComparer.Ordinal);
        _matchedMatchersByNamespace = new Dictionary<string, HashSet<MatcherMethodReference>>(StringComparer.Ordinal);
    }

    public void Execute()
    {
        GenerateMethods();
        EmitMethods();
    }

    private void Report(DiagnosticDescriptor descriptor, SourceLocationModel? location, params object[] messageArgs)
    {
        var diagnostic = Diagnostic.Create(
            descriptor,
            location.HasValue ? location.Value.ToLocation(_syntaxTreesByPath) : null,
            messageArgs);
        _context.ReportDiagnostic(diagnostic);
    }
}
