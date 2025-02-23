using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityLengths.Analyzer.FluentConfiguration;

internal class FluentApiHandler
{
    private FluentApiHandler() { }

    public static int? GetMaxLengthFromFluentApi(
        SemanticModel semanticModel,
        IPropertySymbol property
    )
    {
        try
        {
            var dbContextType = semanticModel.Compilation.GetTypeByMetadataName(
                Constants.EfDbContextNamespace
            );

            if (dbContextType is null)
            {
                return null;
            }

            foreach (var syntaxTree in semanticModel.Compilation.SyntaxTrees)
            {
                var treeSemanticModel = semanticModel.Compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classDecl in classDeclarations)
                {
                    if (
                        treeSemanticModel.GetDeclaredSymbol(classDecl)
                        is not INamedTypeSymbol classSymbol
                    )
                    {
                        continue;
                    }

                    var baseType = classSymbol.BaseType;
                    while (baseType is not null)
                    {
                        if (SymbolEqualityComparer.Default.Equals(baseType, dbContextType))
                        {
                            var onModelCreating = classDecl
                                .Members.OfType<MethodDeclarationSyntax>()
                                .FirstOrDefault(m =>
                                    string.Equals(
                                        m.Identifier.Text,
                                        Constants.OnModelCreatingMethod,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                );

                            if (onModelCreating is not null)
                            {
                                var walker = new FluentConfigurationWalker(
                                    treeSemanticModel,
                                    property
                                );

                                walker.Visit(onModelCreating);

                                if (walker.MaxLength.HasValue)
                                {
                                    return walker.MaxLength;
                                }
                            }

                            break;
                        }

                        baseType = baseType.BaseType;
                    }
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
