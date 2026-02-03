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

    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    private readonly Dictionary<INamedTypeSymbol, TypeContext> _typeContexts;
    private readonly Dictionary<string, List<GeneratedMethod>> _methodsByNamespace;
    private readonly HashSet<INamedTypeSymbol> _matcherTypes;
    private readonly Dictionary<string, HashSet<IMethodSymbol>> _matchedMatchersByNamespace;

    public MethodOverloadsGeneratorCore(Compilation compilation, SourceProductionContext context)
    {
        _compilation = compilation;
        _context = context;
        _typeContexts = new Dictionary<INamedTypeSymbol, TypeContext>(SymbolEqualityComparer.Default);
        _methodsByNamespace = new Dictionary<string, List<GeneratedMethod>>(StringComparer.Ordinal);
        _matcherTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        _matchedMatchersByNamespace = new Dictionary<string, HashSet<IMethodSymbol>>(StringComparer.Ordinal);
    }

    public void Execute()
    {
        CollectTypeContexts();
        GenerateMethods();
        EmitMethods();
    }

    private void Report(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
    {
        var diagnostic = Diagnostic.Create(descriptor, location, messageArgs);
        _context.ReportDiagnostic(diagnostic);
    }
}
