using System;

namespace AnalyzerTestApp
{
    internal static class SemanticAnalyzerTest
    {
        public static void Main()
        {
            const string tamba = "丹羽";
            var firstName1 = tamba;
            var firstName2 = "保次郎";
            firstName2 = "丹羽";

            Console.WriteLine(firstName1);
            Console.WriteLine(firstName2);
        }
    }
}
