using System;

namespace AnalyzerTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // SyntacticAnalyzer のテスト
            Console.WriteLine("たんばほじろうは偉大なり。たんばほじろうは偉大なり。");

            // SemanticAnalyzer のテスト
            const string tamba = "丹羽";
            var firstName1 = tamba;
            var firstName2 = "保次郎";
            firstName2 = "丹羽";

            Console.WriteLine(firstName1);
            Console.WriteLine(firstName2);
        }
    }
}
