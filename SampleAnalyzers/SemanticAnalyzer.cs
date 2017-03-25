using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SampleAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SemanticAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "HLT0002", // ID
            "「丹羽」は偉大な苗字です", // アナライザーの説明
            "「丹羽」はファーストネームではありません。", // エラーメッセージ
            "SampleAnalyzer", // カテゴリ
            DiagnosticSeverity.Warning,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxNodeAction(
                SyntaxNodeAction,
                SyntaxKind.SimpleAssignmentExpression, // 代入
                SyntaxKind.VariableDeclarator // 変数宣言
            );
        }

        private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context)
        {
            switch (context.Node.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    SimpleAssignmentExpressionAction(context);
                    break;
                case SyntaxKind.VariableDeclarator:
                    VariableDeclaratorAction(context);
                    break;
            }
        }

        private static void SimpleAssignmentExpressionAction(SyntaxNodeAnalysisContext context)
        {
            var node = (AssignmentExpressionSyntax)context.Node;

            // 左辺のシンボルを SemanticModel を使って取得
            var leftSymbol = context.SemanticModel.GetSymbolInfo(node.Left).Symbol;

            if (IsFirstNameSymbol(leftSymbol) && IsTambaExpression(node.Right, context.SemanticModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    node.Right.GetLocation() // 右辺を警告場所にする
                ));
            }
        }

        private static void VariableDeclaratorAction(SyntaxNodeAnalysisContext context)
        {
            var node = (VariableDeclaratorSyntax)context.Node;

            // 初期値が設定されていないなら、この後のチェックをする必要ないので return
            var initialValueExpression = node.Initializer?.Value;
            if (initialValueExpression == null) return;

            // 宣言しているシンボルを取得するには GetDeclaredSymbol
            var varSymbol = context.SemanticModel.GetDeclaredSymbol(node);

            if (IsFirstNameSymbol(varSymbol) && IsTambaExpression(initialValueExpression, context.SemanticModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                   Rule,
                   initialValueExpression.GetLocation()
               ));
            }
        }

        /// <summary>
        /// 与えられたシンボルの名前に「firstName」が含まれているかをチェック
        /// </summary>
        private static bool IsFirstNameSymbol(ISymbol symbol)
        {
            return symbol?.Name.IndexOf("firstname", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// <paramref name="node"/> の定数値が「丹羽」かどうかをチェック
        /// </summary>
        private static bool IsTambaExpression(ExpressionSyntax node, SemanticModel semanticModel)
        {
            var optionalValue = semanticModel.GetConstantValue(node);
            return optionalValue.HasValue
                && (optionalValue.Value as string) == "丹羽";
        }
    }
}
