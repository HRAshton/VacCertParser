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
            sb.AppendLine(result.LastName);
            sb.AppendLine(result.FirstName);
            sb.AppendLine(result.MiddleName);
            sb.AppendLine(result.Birthday);
            sb.AppendLine(result.Sex);
            sb.AppendLine(result.Passport);
            sb.AppendLine(result.LastVaccinationDate);
            sb.AppendLine(result.LastVaccineName);

            var textToWrite = sb.ToString();
            File.WriteAllText(outFile, textToWrite, Encoding.GetEncoding("windows-1251"));
        }
    }
}