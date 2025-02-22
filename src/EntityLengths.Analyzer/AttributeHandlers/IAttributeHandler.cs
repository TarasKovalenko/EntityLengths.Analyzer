using Microsoft.CodeAnalysis;

namespace EntityLengths.Analyzer.AttributeHandlers;

/// <summary>
///  Interface for attribute handlers.
/// </summary>
internal interface IAttributeHandler
{
    /// <summary>
    ///  Determines if the attribute handler can handle the given symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    bool CanHandle(ISymbol symbol);

    /// <summary>
    ///  Gets the maximum length of the given symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    int? GetMaxLength(ISymbol symbol);
}