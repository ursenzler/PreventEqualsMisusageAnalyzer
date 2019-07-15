using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PreventEqualsMisusage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PreventEqualsMisusageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PreventEqualsMisusage";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Equality";

        private static readonly Dictionary<string, int> ForbiddenMethodNames = new Dictionary<string, int>
            {
                { "Union", 1 },
                { "Distinct", 0 },
                { "Except", 1 },
                { "Intersect", 1 }
            };

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var access = (MemberAccessExpressionSyntax)context.Node;

            var identifierText = access.Name.Identifier.Text;

            if (ForbiddenMethodNames.ContainsKey(identifierText))
            {
                var forbiddenArgumentCount = ForbiddenMethodNames[identifierText];

                if (access.Parent is InvocationExpressionSyntax p)
                {
                    if (p.ArgumentList.ChildNodes().Count() <= forbiddenArgumentCount)
                    {
                        var diagnostic = Diagnostic.Create(Rule, access.GetLocation(), $"{identifierText} without equality comparer");

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
            else
            {
                switch (identifierText)
                {
                    case "Contains":
                    {
                        var symbol = context.SemanticModel.GetSymbolInfo(access).Symbol ??
                                     context.SemanticModel.GetDeclaredSymbol(access);

                        if (symbol is IMethodSymbol method)
                        {
                            var constructedFrom = method.ConstructedFrom.ContainingType.Name;
                            if (constructedFrom != "String"
                                && constructedFrom != "HashSet")
                            {
                                if (access.Parent is InvocationExpressionSyntax p)
                                {
                                    if (p.ArgumentList.ChildNodes().Count() <= 1)
                                    {
                                        var diagnostic = Diagnostic.Create(Rule, p.GetLocation(),
                                            "Contains without equality comparer");

                                        context.ReportDiagnostic(diagnostic);
                                    }
                                }

                                if (access.Parent is ArgumentSyntax a)
                                {
                                    var diagnostic = Diagnostic.Create(Rule, a.GetLocation(),
                                        "Contains without equality comparer");

                                    context.ReportDiagnostic(diagnostic);
                                }
                            }
                        }

                        break;
                    }

                    case "GroupBy":
                    {
                        if (access.Parent is InvocationExpressionSyntax p)
                        {
                            var symbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(p).Symbol;

                            if (symbol.ConstructedFrom.Parameters.Last().Type.Name != "IEqualityComparer")
                            {
                                var diagnostic = Diagnostic.Create(Rule, access.GetLocation(),
                                    "GroupBy without equality comparer");

                                context.ReportDiagnostic(diagnostic);
                            }
                        }

                        break;
                    }

                    case "Equals":
                    {
                        var symbol = context.SemanticModel.GetSymbolInfo(access).Symbol ??
                                     context.SemanticModel.GetDeclaredSymbol(access);

                        if (symbol is IMethodSymbol method)
                        {
                            if (method.ConstructedFrom.ContainingType.Name == "Object"
                                || method.ConstructedFrom.ContainingType.Name == "ValueType")
                            {
                                var diagnostic = Diagnostic.Create(Rule, access.GetLocation(),
                                    "Equals on types not implementing IEquatable");

                                context.ReportDiagnostic(diagnostic);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}
