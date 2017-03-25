using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SampleAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class HojiroFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(SyntacticAnalyzer.Rule.Id);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "「にわやすじろう」に置き換える",
                    async cancellationToken =>
                    {
                        var sourceText = await context.Document.GetTextAsync(cancellationToken).ConfigureAwait(false);
                        sourceText = sourceText.Replace(context.Span, "にわやすじろう");

                        // 置き換えた sourceText で Document を作り直す
                        return context.Document.WithText(sourceText);
                    },
                    nameof(HojiroFixProvider) // 同じ操作であることを表せれば何でもいい
                ),
                context.Diagnostics
            );

            return Task.CompletedTask;
        }

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;
    }
}
