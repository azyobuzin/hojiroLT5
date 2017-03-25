using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace RoslynGraph
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();
            CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

            if ((options.SourceFiles?.Count ?? 0) == 0)
            {
                Console.WriteLine("No source files");
                return;
            }

            var compilation = CreateCompilation(options.SourceFiles);

            PrintDiagnostics(compilation);

            void saveGraph(Graph graph, string name)
            {
                // 強制的にレイアウトをかける（アホなやり方）
                var viewer = new GViewer();
                viewer.Graph = graph;

                switch (options.OutputFormat)
                {
                    case OutputFormat.Msagl:
                        graph.Write(FileNameGenerator.CreateFilePath(
                            options.OutputDirectory,
                            name, "msagl"
                        ));
                        break;
                    case OutputFormat.Svg:
                        SvgGraphWriter.Write(
                            graph,
                            FileNameGenerator.CreateFilePath(
                                options.OutputDirectory,
                                name, "svg"
                            )
                        );
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var graph = CreateSyntaxGraph(syntaxTree.GetRoot());
                saveGraph(graph, Path.GetFileName(syntaxTree.FilePath));
            }

            {
                var graph = new Graph();
                new SymbolGraphGenerator(graph).Visit(compilation.Assembly);
                saveGraph(graph, "symbols");
            }

            foreach (var (symbol, graph) in new OperationGraphGenerator(compilation).Visit(compilation.Assembly))
            {
                saveGraph(graph, symbol.ToDisplayString());
            }
        }

        private static Compilation CreateCompilation(IEnumerable<string> sourceFiles)
        {
            var syntaxTrees = sourceFiles
                .Select(x =>
                {
                    var sourceFile = new FileInfo(x);

                    SourceText sourceText;
                    using (var stream = sourceFile.OpenRead())
                    {
                        sourceText = SourceText.From(stream);
                    }

                    return CSharpSyntaxTree.ParseText(
                        sourceText,
                        CSharpParseOptions.Default
                            .WithFeatures(new[] { new KeyValuePair<string, string>("IOperation", "") }),
                        sourceFile.FullName
                    );
                })
                .ToArray();

            return CSharpCompilation.Create(
                "RoslynGraph.Generated",
                syntaxTrees,
                EnumerateReferenceAssemblies().Select(x => MetadataReference.CreateFromFile(x)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
            );
        }

        private static IEnumerable<string> EnumerateReferenceAssemblies()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(programFiles))
                programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            var frameworkDir = Path.Combine(programFiles, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6");

            var defaultAssemblies = new[]
            {
                "mscorlib.dll",
                "Microsoft.CSharp.dll",
                "System.dll",
                "System.Core.dll",
                "System.Xml.dll",
                "System.Xml.Linq.dll"
            };

            return defaultAssemblies.Select(x => Path.Combine(frameworkDir, x));
        }

        private static void PrintDiagnostics(Compilation compilation)
        {
            foreach (var diagnostic in compilation.GetDiagnostics())
            {
                Console.WriteLine(diagnostic);
            }
        }

        private static Graph CreateSyntaxGraph(SyntaxNode syntaxNode)
        {
            string getId(SyntaxNodeOrToken x) => x.GetHashCode().ToString("x", CultureInfo.InvariantCulture);

            var graph = new Graph();
            var queue = new Queue<SyntaxNodeOrToken>();
            queue.Enqueue(syntaxNode);

            while (queue.Count > 0)
            {
                var nodeOrToken = queue.Dequeue();

                var graphNode = graph.AddNode(getId(nodeOrToken));
                graphNode.LabelText = nodeOrToken.Kind().ToString();
                graphNode.Label.FontColor = Color.White;
                graphNode.Attr.FillColor = nodeOrToken.IsNode ? Color.Blue : Color.Green;

                if (nodeOrToken.AsNode() != syntaxNode)
                    graph.AddEdge(getId(nodeOrToken.Parent), graphNode.Id);

                Node prevNode = null;
                foreach (var child in nodeOrToken.ChildNodesAndTokens())
                {
                    queue.Enqueue(child);

                    var childNode = graph.AddNode(getId(child));
                    graph.LayerConstraints.AddUpDownConstraint(graphNode, childNode);

                    if (prevNode != null)
                        graph.LayerConstraints.AddLeftRightConstraint(prevNode, childNode);

                    prevNode = childNode;
                }
            }

            return graph;
        }
    }
}
