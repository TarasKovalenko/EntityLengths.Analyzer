using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EntityLengths.Analyzer.AttributeHandlers;

internal class MaxLengthAttributeHandler : IAttributeHandler
{
    public bool CanHandle(ISymbol symbol)
    {
        return symbol.GetAttributes()
            .Any(attr =>
                string.Equals(attr.AttributeClass?.Name, Constants.MaxLengthAttribute,
                    StringComparison.OrdinalIgnoreCase));
    }

    public int? GetMaxLength(ISymbol symbol)
    {
        var attr = symbol.GetAttributes()
            .FirstOrDefault(a =>
                string.Equals(a.AttributeClass?.Name, Constants.MaxLengthAttribute,
                    StringComparison.OrdinalIgnoreCase));

        if (attr?.ConstructorArguments.Length > 0)
        {
            return (int?)attr.ConstructorArguments[0].Value;
        }

        return null;
    }
}