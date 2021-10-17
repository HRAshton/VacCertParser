using System;

namespace VacCertParser.Parser.Models
{
    /// <summary>
    /// Данные верситиката о прививках.
    /// </summary>
    public class VacCertDataModel
    {
        /// <summary>
        /// Имя.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Отчество.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Пол.
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// Дата рождения.
        /// </summary>
        public string Birthday { get; set; }

        /// <summary>
        /// Паспорт.
        /// </summary>
        public string Passport { get; set; }

        /// <summary>
        /// Дата последней вакцинации.
        /// </summary>
        public string LastVaccinationDate { get; set; }

        /// <summary>
        /// Имя последней вакцины.
        /// </summary>
        public string LastVaccineName { get; set; }
    }
}