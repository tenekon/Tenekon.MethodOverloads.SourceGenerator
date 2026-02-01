using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Tenekon.MethodOverloads.SourceGenerator;

internal sealed partial class MethodOverloadsGeneratorCore
{
    private const string GenerateOverloadsAttributeName = "GenerateOverloadsAttribute";
    private const string GenerateMethodOverloadsAttributeName = "GenerateMethodOverloadsAttribute";
    private const string OverloadGenerationOptionsAttributeName = "OverloadGenerationOptionsAttribute";

    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    private readonly Dictionary<INamedTypeSymbol, TypeContext> _typeContexts;
    private readonly Dictionary<string, List<GeneratedMethod>> _methodsByNamespace;
    private readonly HashSet<INamedTypeSymbol> _matcherTypes;

    public MethodOverloadsGeneratorCore(Compilation compilation, SourceProductionContext context)
    {
        _compilation = compilation;
        _context = context;
        _typeContexts = new Dictionary<INamedTypeSymbol, TypeContext>(SymbolEqualityComparer.Default);
        _methodsByNamespace = new Dictionary<string, List<GeneratedMethod>>(StringComparer.Ordinal);
        _matcherTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
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
