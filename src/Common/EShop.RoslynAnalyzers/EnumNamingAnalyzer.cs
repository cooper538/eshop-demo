using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EShop.RoslynAnalyzers;

/// <summary>
/// Roslyn analyzer that enforces enum naming convention: prefix 'E'.
/// Example: EOrderStatus, EServiceProtocol
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumNamingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ESHOP001";
    private const string Category = "Naming";

    private static readonly LocalizableString Title = "Enum naming convention violation";

    private static readonly LocalizableString MessageFormat =
        "Enum '{0}' should have prefix 'E' (e.g., 'E{0}')";

    private static readonly LocalizableString Description =
        "All enums in the EShop codebase should follow the naming convention: prefix 'E'. Example: EOrderStatus, EServiceProtocol.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.TypeKind != TypeKind.Enum)
        {
            return;
        }

        var enumName = namedTypeSymbol.Name;

        // Check if enum starts with 'E' followed by uppercase letter
        var hasValidPrefix =
            enumName.StartsWith("E", StringComparison.Ordinal)
            && enumName.Length > 1
            && char.IsUpper(enumName[1]);

        if (hasValidPrefix)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], enumName);

        context.ReportDiagnostic(diagnostic);
    }
}
