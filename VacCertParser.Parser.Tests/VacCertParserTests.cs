using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace VacCertParser.Parser.Tests
{
    [TestFixture]
    public class VacCertParserTests
    {
        [Test]
        public void ExpressIntegrationTest()
        {
            var certPdfContent = File.ReadAllBytes("kr1.pdf");

            var parser = new VacCertParser();
            var result = parser.Parse(certPdfContent);

            Assert.AreEqual("Сергей", result.FirstName);
            Assert.AreEqual("Ковалев", result.LastName);
            Assert.AreEqual("Юрьевич", result.MiddleName);
            Assert.AreEqual("28-10-1965", result.Birthday);
            Assert.AreEqual("Мужской", result.Sex);
            Assert.AreEqual("5009 №715855", result.Passport);
            Assert.AreEqual("03.08.2021", result.LastVaccinationDate);
            Assert.AreEqual(
                "Гам-КОВИД-Вак Комбинированная векторная вакцина для профилактики коронавирусной инфекции," +
                " вызываемой вирусом SARS-CoV-2",
                result.LastVaccineName);
        }
    }
}