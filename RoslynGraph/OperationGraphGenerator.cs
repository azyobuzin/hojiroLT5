using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.Msagl.Drawing;

namespace RoslynGraph
{
    internal sealed class OperationGraphGenerator : SymbolVisitor<IEnumerable<(ISymbol, Graph)>>
    {
        private readonly Compilation _compilation;

        public OperationGraphGenerator(Compilation compilation)
        {
            this._compilation = compilation;
        }

        public override IEnumerable<(ISymbol, Graph)> DefaultVisit(ISymbol symbol)
        {
            throw new NotSupportedException("私は手抜きをしました。");
        }

        public override IEnumerable<(ISymbol, Graph)> VisitAssembly(IAssemblySymbol symbol)
        {
            return symbol.Modules.SelectMany(this.Visit);
        }

        public override IEnumerable<(ISymbol, Graph)> VisitModule(IModuleSymbol symbol)
        {
            return this.Visit(symbol.GlobalNamespace);
        }

        public override IEnumerable<(ISymbol, Graph)> VisitNamespace(INamespaceSymbol symbol)
        {
            return symbol.GetMembers().SelectMany(this.Visit);
        }

        public override IEnumerable<(ISymbol, Graph)> VisitNamedType(INamedTypeSymbol symbol)
        {
            return symbol.GetMembers().SelectMany(this.Visit);
        }

        public override IEnumerable<(ISymbol, Graph)> VisitMethod(IMethodSymbol symbol)
        {
            var syntaxRefs = symbol.DeclaringSyntaxReferences;
            if (syntaxRefs.IsDefault || syntaxRefs.Length != 1) yield break;

            var syntaxRef = syntaxRefs[0];
            var syntax = (MethodDeclarationSyntax)syntaxRef.GetSyntax();
            var methodBody = (SyntaxNode)syntax.Body ?? syntax.ExpressionBody.Expression;

            var semanticModel = this._compilation.GetSemanticModel(syntaxRef.SyntaxTree);
            var graph = new Graph();
            new CoreVisitor().Visit(semanticModel.GetOperation(methodBody), graph);

            yield return (symbol, graph);
        }

        private sealed class CoreVisitor : OperationVisitor<Graph, Node>
        {
            private static string GetId(IOperation operation)
            {
                return RuntimeHelpers.GetHashCode(operation)
                    .ToString("x", CultureInfo.InvariantCulture);
            }

            public override Node DefaultVisit(IOperation operation, Graph argument)
            {
                throw new NotSupportedException("私は手抜きをしました。");
            }

            public override Node VisitBlockStatement(IBlockStatement operation, Graph graph)
            {
                var node = graph.AddNode(GetId(operation));
                node.LabelText = "BlockStatement";

                Node prev = null;
                foreach (var stmt in operation.Statements)
                {
                    var child = this.Visit(stmt, graph);
                    graph.AddEdge(node.Id, child.Id);

                    if (prev != null)
                        graph.LayerConstraints.AddUpDownConstraint(prev, child);

                    prev = child;
                }

                return node;
            }

            public override Node VisitExpressionStatement(IExpressionStatement operation, Graph graph)
            {
                var node = graph.AddNode(GetId(operation));
                node.LabelText = "ExpressionStatement";

                var child = this.Visit(operation.Expression, graph);
                graph.AddEdge(node.Id, child.Id);

                return node;
            }

            public override Node VisitInvocationExpression(IInvocationExpression operation, Graph graph)
            {
                var node = graph.AddNode(GetId(operation));
                node.LabelText = "InvocationExpression\r\n"
                    + operation.TargetMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                IEnumerable<IOperation> args = operation.ArgumentsInParameterOrder;
                if (operation.Instance is IOperation instance)
                    args = new[] { instance }.Concat(args);

                Node prev = null;
                foreach (var arg in args)
                {
                    var child = this.Visit(arg, graph);
                    graph.AddEdge(node.Id, child.Id);

                    if (prev != null)
                        graph.LayerConstraints.AddUpDownConstraint(prev, child);

                    prev = child;
                }

                return node;
            }

            public override Node VisitArgument(IArgument operation, Graph graph)
            {
                var node = graph.AddNode(GetId(operation));
                node.LabelText = "Argument";

                var child = this.Visit(operation.Value, graph);
                graph.AddEdge(node.Id, child.Id);

                return node;
            }

            public override Node VisitLiteralExpression(ILiteralExpression operation, Graph graph)
            {
                var node = graph.AddNode(GetId(operation));
                node.LabelText = "LiteralExpression\r\n" + operation.Text;
                return node;
            }
        }
    }
}
