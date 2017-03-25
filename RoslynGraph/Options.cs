using System.Collections.Generic;
using CommandLine;

namespace RoslynGraph
{
    internal class Options
    {
        [ValueList(typeof(List<string>))]
        public IList<string> SourceFiles { get; set; }

        [Option('o', "output-directory")]
        public string OutputDirectory { get; set; }

        [Option('f', "output-format", DefaultValue = OutputFormat.Msagl)]
        public OutputFormat OutputFormat { get; set; }
    }
}
