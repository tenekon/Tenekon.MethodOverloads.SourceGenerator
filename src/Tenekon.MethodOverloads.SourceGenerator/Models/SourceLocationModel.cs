using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Tenekon.MethodOverloads.SourceGenerator.Models;

internal readonly record struct SourceLocationModel(
    string? SourceTreeFilePath,
    TextSpan SourceSpan,
    FileLinePositionSpan FileLineSpan,
    FileLinePositionSpan MappedFileLineSpan)
{
    public Location ToLocation()
    {
        var lineSpan = new LinePositionSpan(FileLineSpan.StartLinePosition, FileLineSpan.EndLinePosition);

        if (FileLineSpan.HasMappedPath)
        {
            var mappedLineSpan = new LinePositionSpan(
                MappedFileLineSpan.StartLinePosition,
                MappedFileLineSpan.EndLinePosition);

            return Location.Create(
                SourceTreeFilePath ?? string.Empty,
                SourceSpan,
                lineSpan,
                MappedFileLineSpan.Path,
                mappedLineSpan);
        }

        return Location.Create(SourceTreeFilePath ?? string.Empty, SourceSpan, lineSpan);
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
        var fileLineSpan = location.GetLineSpan();
        var mappedFileSpan = fileLineSpan.HasMappedPath ? location.GetMappedLineSpan() : default;

        return new SourceLocationModel(
            location.SourceTree?.FilePath,
            location.SourceSpan,
            fileLineSpan,
            mappedFileSpan);
    }
}