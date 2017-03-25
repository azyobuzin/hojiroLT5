using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.CodeAnalysis.Text;

namespace RoslynGraph
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();
            CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

            if (options.SourceFiles == null || options.SourceFiles.Count == 0)
            {
                Console.WriteLine("No source files");
                return;
            }

            var compilation = CreateCompilation(options.SourceFiles);

            PrintDiagnostics(compilation);


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

            return Directory.EnumerateFiles(
                Path.Combine(programFiles, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6"),
                "*.dll"
            );
        }

        private static void PrintDiagnostics(Compilation compilation)
        {
            foreach (var diagnostic in compilation.GetDiagnostics())
            {
                Console.WriteLine(diagnostic);
            }
        }


    }
}
