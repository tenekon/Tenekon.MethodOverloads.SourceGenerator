using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Tenekon.MethodOverloads.SourceGenerator.Helpers;

internal readonly record struct SourceFileModel(string Path, string Text, LanguageVersion LanguageVersion)
{
    public SyntaxTree CreateSyntaxTree()
    {
        var options = new CSharpParseOptions(LanguageVersion);
        return CSharpSyntaxTree.ParseText(Text, options, Path);
    }
}

internal readonly record struct SourceLocationModel(
    string Path,
    int SpanStart,
    int SpanLength,
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter)
{
    public Location ToLocation()
    {
        var span = new TextSpan(SpanStart, SpanLength);
        if (string.IsNullOrEmpty(Path)) return Location.None;

        var lineSpan = new LinePositionSpan(
            new LinePosition(StartLine, StartCharacter),
            new LinePosition(EndLine, EndCharacter));
        return Location.Create(Path, span, lineSpan);
    }

    public static SourceLocationModel FromSyntaxNode(SyntaxNode node)
    {
        return FromLocation(node.GetLocation());
    }

    public static SourceLocationModel FromSyntaxToken(SyntaxToken token)
    {
        return FromLocation(token.GetLocation());
    }

    private static SourceLocationModel FromLocation(Location location)
    {
        var lineSpan = location.GetLineSpan();
        return new SourceLocationModel(
            location.SourceTree?.FilePath ?? string.Empty,
            location.SourceSpan.Start,
            location.SourceSpan.Length,
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line,
            lineSpan.EndLinePosition.Character);
    }
}