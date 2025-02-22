using System.Collections.Immutable;
using System.Linq;
using EntityLengths.Analyzer.AttributeHandlers;
using EntityLengths.Analyzer.FluentConfiguration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EntityLengths.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MaxLengthPropertyAnalyzer : DiagnosticAnalyzer
{
    private const string InfoDiagnosticId = "ML001";
    private const string ErrorDiagnosticId = "ML002";

    private const string InfoTitle = "String length validation needed";
    private const string ErrorTitle = "String length exceeds maximum";

    private const string InfoMessageFormat =
        "Property '{0}' has a maximum length of {1}. Consider adding length validation.";

    private const string ErrorMessageFormat = "String length ({0}) exceeds maximum length of {1} for property '{2}'";

    private const string Description =
        "Properties with length constraints should validate their values before assignment.";

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor WarningRule = new(
        InfoDiagnosticId,
        InfoTitle,
        InfoMessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly DiagnosticDescriptor ErrorRule = new(
        ErrorDiagnosticId,
        ErrorTitle,
        ErrorMessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    private readonly IAttributeHandler[] _handlers =
    [
        new MaxLengthAttributeHandler(),
        new StringLengthAttributeHandler(),
        new ColumnAttributeHandler()
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(WarningRule, ErrorRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            AnalyzeAssignment,
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression);
    }

    private void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (assignment.Left is not (IdentifierNameSyntax or MemberAccessExpressionSyntax))
        {
            return;
        }

        var symbolInfo = context.SemanticModel.GetSymbolInfo(assignment.Left);
        if (symbolInfo.Symbol is not IPropertySymbol propertySymbol)
        {
            return;
        }

        // Skip if not a string property
        if (propertySymbol.Type.SpecialType != SpecialType.System_String)
        {
            return;
        }

        // Check all attribute handlers
        int? maxLength = null;
        foreach (var handler in _handlers.Where(h => h.CanHandle(propertySymbol)))
        {
            maxLength = handler.GetMaxLength(propertySymbol);
            if (maxLength.HasValue)
            {
                break;
            }
        }

        maxLength ??= FluentApiHandler.GetMaxLengthFromFluentApi(
            context.SemanticModel,
            propertySymbol
        );

        if (!maxLength.HasValue)
        {
            return;
        }

        // Check if the assigned value is a string literal
        if (assignment.Right is LiteralExpressionSyntax literalExpression &&
            literalExpression.Kind() == SyntaxKind.StringLiteralExpression)
        {
            var stringValue = literalExpression.Token.ValueText;
            if (stringValue.Length > maxLength.Value)
            {
                // Report error for string literals that exceed max length
                var diagnostic = Diagnostic.Create(
                    ErrorRule,
                    assignment.GetLocation(),
                    stringValue.Length,
                    maxLength.Value,
                    propertySymbol.Name);

                context.ReportDiagnostic(diagnostic);
                return;
            }
        }

        // Report info for non-literal assignments or valid literals
        var infoDiagnostic = Diagnostic.Create(
            WarningRule,
            assignment.GetLocation(),
            propertySymbol.Name,
            maxLength.Value);

        context.ReportDiagnostic(infoDiagnostic);
    }
}