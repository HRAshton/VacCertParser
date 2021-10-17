using System.IO;
using System.Text;
using VacCertParser.Parser.Models;

namespace VacCertParser.Console
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var inFile = args[0];
            var outFile = args[1];

            var parser = new Parser.VacCertParser();
            var result = parser.Parse(File.ReadAllBytes(inFile));

            WriteResult(result, outFile);
        }

        private static void WriteResult(VacCertDataModel result, string outFile)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
        }
    }
}