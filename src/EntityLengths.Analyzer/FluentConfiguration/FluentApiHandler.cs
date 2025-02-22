using Microsoft.CodeAnalysis;

namespace EntityLengths.Analyzer.FluentConfiguration;

internal class FluentApiHandler
{
    private FluentApiHandler()
    {
    }

    public static int? GetMaxLengthFromFluentApi(SemanticModel semanticModel, IPropertySymbol property)
    {
        var walker = new FluentConfigurationWalker(semanticModel, property);

        // Walk through all syntax trees in the compilation
        foreach (var tree in semanticModel.Compilation.SyntaxTrees)
        {
            var root = tree.GetRoot();
            walker.Visit(root);

            // If we found a max length, return it
            if (walker.MaxLength.HasValue)
            {
                return walker.MaxLength;
            }
        }

        return null;
    }
}