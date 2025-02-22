using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityLengths.Analyzer.FluentConfiguration;

public class FluentConfigurationWalker(
    SemanticModel semanticModel,
    IPropertySymbol propertySymbol
) : CSharpSyntaxWalker
{
    public int? MaxLength { get; private set; }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // Only look in OnModelCreating methods
        if (!string.Equals(node.Identifier.Text, Constants.OnModelCreatingMethod, StringComparison.OrdinalIgnoreCase) ||
            !IsDbContextMethod(node))
        {
            return;
        }

        base.VisitMethodDeclaration(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Check if this is a HasMaxLength() call
        if (node.Expression is MemberAccessExpressionSyntax memberAccess &&
            string.Equals(memberAccess.Name.Identifier.Text, Constants.HasMaxLengthMethod,
                StringComparison.OrdinalIgnoreCase) &&
            // Check if this HasMaxLength is part of a property configuration chain
            IsPropertyConfiguration(node, propertySymbol))
        {
            // Get the length argument
            var arg = node.ArgumentList.Arguments.FirstOrDefault();
            if (arg?.Expression is LiteralExpressionSyntax { Token.Value: int length })
            {
                MaxLength = length;
            }
        }

        base.VisitInvocationExpression(node);
    }

    private bool IsDbContextMethod(MethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.Parent is not ClassDeclarationSyntax classDecl)
        {
            return false;
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol is null)
        {
            return false;
        }

        var dbContextType =
            semanticModel.Compilation.GetTypeByMetadataName(Constants.EfDbContextNamespace);
        return dbContextType is not null && InheritsFrom(classSymbol, dbContextType);
    }

    private static bool InheritsFrom(ITypeSymbol? type, ITypeSymbol baseType)
    {
        while (type is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private bool IsPropertyConfiguration(InvocationExpressionSyntax hasMaxLength, IPropertySymbol targetProperty)
    {
        var current = hasMaxLength.Expression;
        while (current is MemberAccessExpressionSyntax mae)
        {
            // Look for Property() call in the chain
            if (mae.Expression is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: Constants.Property }
                } invocation)
            {
                // Check if lambda references our target property
                var lambda = invocation.ArgumentList.Arguments
                    .FirstOrDefault()?.Expression as LambdaExpressionSyntax;

                if (lambda?.Body is MemberAccessExpressionSyntax propAccess)
                {
                    var symbol = semanticModel.GetSymbolInfo(propAccess).Symbol as IPropertySymbol;
                    return SymbolEqualityComparer.Default.Equals(symbol, targetProperty);
                }

                // Also check for string-based configuration
                if (invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression
                    is LiteralExpressionSyntax literal)
                {
                    return literal.Token.ValueText == targetProperty.Name;
                }
            }

            current = mae.Expression;
        }

        return false;
    }
}