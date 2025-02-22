using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace EntityLengths.Analyzer.AttributeHandlers;

internal class ColumnAttributeHandler : IAttributeHandler
{
    private static readonly Regex VarCharPattern = new(
        @"(?:var)?char\s*\((\d+)\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public bool CanHandle(ISymbol symbol) =>
        symbol.GetAttributes()
            .Any(attr => string.Equals(attr.AttributeClass?.Name, Constants.ColumnAttribute,
                StringComparison.OrdinalIgnoreCase));

    public int? GetMaxLength(ISymbol symbol)
    {
        var attr = symbol.GetAttributes()
            .FirstOrDefault(a =>
                string.Equals(a.AttributeClass?.Name, Constants.ColumnAttribute, StringComparison.OrdinalIgnoreCase));

        if (attr is null)
        {
            return null;
        }

        var typeNameArg = attr.NamedArguments
            .FirstOrDefault(arg => string.Equals(arg.Key, Constants.TypeName, StringComparison.OrdinalIgnoreCase))
            .Value;

        if (typeNameArg.IsNull)
        {
            return null;
        }

        var typeName = typeNameArg.Value?.ToString();
        if (string.IsNullOrEmpty(typeName))
        {
            return null;
        }

        var match = VarCharPattern.Match(typeName);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var length))
        {
            return length;
        }

        return null;
    }
}