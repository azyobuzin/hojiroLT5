using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.Msagl.Drawing;

namespace RoslynGraph
{
    internal sealed class SymbolGraphGenerator : SymbolVisitor<Node>
    {
        private readonly Graph _graph;

        public SymbolGraphGenerator(Graph graph)
        {
            this._graph = graph;
        }

        private static string GetId(ISymbol symbol)
        {
            return RuntimeHelpers.GetHashCode(symbol)
                .ToString("x", CultureInfo.InvariantCulture);
        }

        public override Node DefaultVisit(ISymbol symbol)
        {
            throw new NotSupportedException("私は手抜きをしました。");
        }

        public override Node VisitAssembly(IAssemblySymbol symbol)
        {
            var node = this._graph.AddNode(GetId(symbol));
            node.LabelText = symbol.Name;
            node.Label.FontColor = Color.White;
            node.Attr.FillColor = Color.Red;

            foreach(var module in symbol.Modules)
            {
                var child = this.Visit(module);
                this._graph.AddEdge(node.Id, child.Id);
            }

            return node;
        }

        public override Node VisitModule(IModuleSymbol symbol)
        {
            var node = this._graph.AddNode(GetId(symbol));
            node.LabelText = symbol.Name;
            node.Label.FontColor = Color.White;
            node.Attr.FillColor = Color.DarkOrange;

            var child = this.Visit(symbol.GlobalNamespace);
            this._graph.AddEdge(node.Id, child.Id);

            return node;
        }

        public override Node VisitNamespace(INamespaceSymbol symbol)
        {
            var node = this._graph.AddNode(GetId(symbol));
            node.LabelText = symbol.IsGlobalNamespace ? "(Global Namespace)" : symbol.Name;
            node.Label.FontColor = Color.White;
            node.Attr.FillColor = Color.DarkKhaki;

            foreach(var member in symbol.GetMembers())
            {
                var child = this.Visit(member);
                this._graph.AddEdge(node.Id, child.Id);
            }

            return node;
        }

        public override Node VisitNamedType(INamedTypeSymbol symbol)
        {
            var node = this._graph.AddNode(GetId(symbol));
            node.LabelText = symbol.Name;
            node.Label.FontColor = Color.White;
            node.Attr.FillColor = Color.Green;

            foreach (var member in symbol.GetMembers())
            {
                var child = this.Visit(member);
                this._graph.AddEdge(node.Id, child.Id);
            }

            return node;
        }

        public override Node VisitMethod(IMethodSymbol symbol)
        {
            var node = this._graph.AddNode(GetId(symbol));
            node.LabelText = symbol.Name;
            node.Label.FontColor = Color.White;
            node.Attr.FillColor = Color.Blue;

            foreach (var parameter in symbol.Parameters)
            {
                var child = this.Visit(parameter);
                this._graph.AddEdge(node.Id, child.Id);
            }

            return node;
        }

        public override Node VisitParameter(IParameterSymbol symbol)
        {
            var node = this._graph.AddNode(GetId(symbol));
            node.LabelText = symbol.Name;
            node.Label.FontColor = Color.White;
            node.Attr.FillColor = Color.DarkBlue;
            return node;
        }
    }
}
