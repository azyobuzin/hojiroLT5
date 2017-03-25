using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SampleAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SyntacticAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "HLT0001", // ID
            "たんばほじろう → にわやすじろう", // アナライザーの説明
            "もしかして: にわやすじろう", // エラーメッセージ
            "SampleAnalyzer", // カテゴリ
            DiagnosticSeverity.Warning,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // 並列に走らせても問題ないよ！という印
            context.EnableConcurrentExecution();

            // 文字列リテラルにアクションを設定
            context.RegisterSyntaxNodeAction(SyntaxNodeAction, SyntaxKind.StringLiteralExpression);
        }

        private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context)
        {
            const string targetString = "たんばほじろう";

            // StringLiteralExpression の型は LiteralExpressionSyntax
            var node = (LiteralExpressionSyntax)context.Node;
            var stringLiteralToken = node.Token;

            // 「たんばほじろう」をすべて探してレポート
            var hojiroIndex = -1;
            while ((hojiroIndex = stringLiteralToken.Text.IndexOf(targetString, hojiroIndex + 1)) >= 0)
            {
                // 警告を出す範囲を計算
                var start = stringLiteralToken.SpanStart + hojiroIndex;
                var location = node.SyntaxTree.GetLocation(new TextSpan(start, targetString.Length));

                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    location
                ));
            }
        }
    }
}
