using System;
using System.Globalization;
using System.Linq;
using VacCertParser.Parser.Models;

namespace VacCertParser.Parser
{
    public class VacCertParser
    {
        public VacCertParser()
        {
            PdfParser = new PdfParser();
        }

        private PdfParser PdfParser { get; }

        public VacCertDataModel Parse(byte[] fileContent)
        {
            var pdfParserResult = PdfParser.Parse(fileContent);

            var data = new VacCertDataModel();
            ParsePersonalData(pdfParserResult, data);
            ParseDocumentData(pdfParserResult, data);
            ParseVaccineData(pdfParserResult, data);

            return data;
        }

        private static void ParsePersonalData(string[] pdfParserResult, VacCertDataModel vacCertDataModel)
        {
            var personalDataLine = Array.IndexOf(pdfParserResult, "Персональные данные");

            SplitName(pdfParserResult[personalDataLine + 1], vacCertDataModel);
            vacCertDataModel.Birthday = pdfParserResult[personalDataLine + 2];
            vacCertDataModel.Sex = pdfParserResult[personalDataLine + 3];
        }

        private static void ParseDocumentData(string[] pdfParserResult, VacCertDataModel vacCertDataModel)
        {
            var documentLine = Array.IndexOf(pdfParserResult, "Документ удостоверяющий личность");
            vacCertDataModel.Passport = pdfParserResult
                .Skip(documentLine)
                .FirstOrDefault(line => line.Any(char.IsNumber));
        }

        private static void ParseVaccineData(string[] pdfParserResult, VacCertDataModel vacCertDataModel)
        {
            var currentLineIndex = Math.Max(
                Array.LastIndexOf(pdfParserResult, "Первая вакцинация"),
                Array.LastIndexOf(pdfParserResult, "Вторая вакцинация"));
            var lastVaccinationLineIndex = Array.LastIndexOf(pdfParserResult, "Дата введения вакцины:");

            var lastVaccinationDate = FindVaccinationDate(
                pdfParserResult,
                lastVaccinationLineIndex,
                ref currentLineIndex);
            var vaccineName = ParseVaccineName(pdfParserResult, currentLineIndex, lastVaccinationLineIndex);

            vacCertDataModel.LastVaccineName = vaccineName;
            vacCertDataModel.LastVaccinationDate = lastVaccinationDate;
        }

        private static string ParseVaccineName(string[] pdfParserResult, int currentLineIndex, int lastVaccinationLineIndex)
        {
            return string.Join(" ", pdfParserResult
                .Skip(currentLineIndex)
                .Take(lastVaccinationLineIndex - currentLineIndex));
        }

        private static string FindVaccinationDate(
            string[] pdfParserResult,
            int lastVaccinationLineIndex,
            ref int currentLineIndex)
        {
            string lastVaccinationDate = null;
            while (++currentLineIndex <= lastVaccinationLineIndex && lastVaccinationDate == null)
            {
                if (DateTime.TryParseExact(pdfParserResult[currentLineIndex], "dd.MM.yyyy",
                    CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out _))
                {
                    lastVaccinationDate = pdfParserResult[currentLineIndex];
                }
            }

            return lastVaccinationDate;
        }

        private static void SplitName(string name, VacCertDataModel vacCertDataModel)
        {
            var parts = name.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            vacCertDataModel.LastName = parts[0];
            vacCertDataModel.FirstName = parts[1];

            if (parts.Length > 2)
            {
                vacCertDataModel.MiddleName = string.Join(" ", parts.Skip(2));
            }
        }
    }
}