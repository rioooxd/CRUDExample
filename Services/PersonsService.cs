using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using ClassLibrary;
using Services.Helpers;
using ServiceContracts.DTO.Enums;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using OfficeOpenXml;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTimings;
using Exceptions;
namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;
        public PersonsService(IPersonsRepository personsRepository, ILogger<PersonsService> logger, IDiagnosticContext diagnosticContext ,bool initialize = true)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }
        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            if(personAddRequest == null) throw new ArgumentNullException(nameof(personAddRequest));

            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();

            person.PersonID = Guid.NewGuid();

            await _personsRepository.AddPerson(person);
            //_db.sp_InsertPerson(person);

            return person.ToPersonResponse();

        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonsService");

            // SELECT * FROM Persons 
            var persons = await _personsRepository.GetAllPersons();
            return persons.Select(p => p.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            if(personID == null)
                return null;

            Person? p = await _personsRepository.GetPersonByPersonID(personID.Value);
            return p == null ? null : p.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {

            _logger.LogInformation("GetFilteredPersons of PersonsService");
            List<Person> persons;

            using (Operation.Time("Time for Filtered Persons from Database"))
            {
                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) =>
                        await _personsRepository.GetFilteredPersons(temp => temp.PersonName.Contains(searchString)),
                    nameof(PersonResponse.Email) =>
                        await _personsRepository.GetFilteredPersons(temp => temp.Email.Contains(searchString)),
                    nameof(PersonResponse.DateOfBirth) =>
                        await _personsRepository.GetFilteredPersons(temp => temp.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString)),
                    nameof(PersonResponse.Gender) =>
                        await _personsRepository.GetFilteredPersons(temp => temp.Gender.Contains(searchString)),
                    nameof(PersonResponse.CountryID) =>
                        await _personsRepository.GetFilteredPersons(temp => temp.Country.CountryName.Contains(searchString)),
                    nameof(PersonResponse.Address) =>
                        await _personsRepository.GetFilteredPersons(temp => temp.Address.Contains(searchString)),

                    _ => await _personsRepository.GetAllPersons()
                };
            }
            _diagnosticContext.Set("Persons", persons);
            return persons.Select(temp => temp.ToPersonResponse()).ToList();
        }

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> persons, string sortBy, SortOrderOptions sortOrder)
        {
            _logger.LogInformation("GetSortedPersons of PersonsService");
            if (string.IsNullOrEmpty(sortBy)) return persons;

            List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Email), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Email), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.DateOfBirth).ToList(),
                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.DateOfBirth).ToList(),
                (nameof(PersonResponse.Age), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.Age).ToList(),
                (nameof(PersonResponse.Age), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.Age).ToList(),
                (nameof(PersonResponse.Gender), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.CountryName), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.CountryName, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.CountryName), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.CountryName, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Address), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Address), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) => persons.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),
                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) => persons.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),
                _ => persons


            };
            

            return sortedPersons;

        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if(personUpdateRequest == null) throw new ArgumentNullException(nameof(personUpdateRequest));

            ValidationHelper.ModelValidation(personUpdateRequest);

            Person? matchingPerson = await _personsRepository.GetPersonByPersonID(personUpdateRequest.PersonID);
            if (matchingPerson == null) throw new InvalidPersonIDException("Given PersonID doesnt exist");

            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _personsRepository.UpdatePerson(matchingPerson);
            return matchingPerson.ToPersonResponse();
        }
        public async Task<bool> DeletePerson(Guid? personID)
        {
            if(personID == null) throw new ArgumentNullException(nameof(personID));

            Person? person = await _personsRepository.GetPersonByPersonID(personID.Value);
            if (person == null) return false;

            await _personsRepository.DeletePersonByPersonID(person.PersonID);
            return true;
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);

            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvWriter csvWriter = new CsvWriter(writer, csvConfig, leaveOpen: true);
            csvWriter.WriteField(nameof(PersonResponse.PersonID));
            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.CountryName));
            csvWriter.NextRecord();
            csvWriter.Flush();
            List<PersonResponse> persons = await GetAllPersons();
            foreach (PersonResponse person in persons)
            {
                csvWriter.WriteField(person.PersonID);
                csvWriter.WriteField(person.PersonName);
                if(person.DateOfBirth.HasValue)
                    csvWriter.WriteField(person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
                csvWriter.WriteField(person.Email);
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.CountryName);
                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add("Persons");
                workSheet.Cells["A1"].Value = "Person ID";
                workSheet.Cells["B1"].Value = "Person Name";
                workSheet.Cells["C1"].Value = "Email";
                workSheet.Cells["D1"].Value = "Date of Birth";
                workSheet.Cells["E1"].Value = "Age";
                workSheet.Cells["F1"].Value = "Gender";
                workSheet.Cells["G1"].Value = "Country";
                workSheet.Cells["H1"].Value = "Address";
                workSheet.Cells["I1"].Value = "Receive News Letters";

                using (ExcelRange headerCells = workSheet.Cells["A1:I1"])
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> persons = await GetAllPersons();
                foreach (PersonResponse person in persons)
                {
                    workSheet.Cells[row, 1].Value = person.PersonID;
                    workSheet.Cells[row, 2].Value = person.PersonName;
                    workSheet.Cells[row, 3].Value = person.Email;
                    if (person.DateOfBirth.HasValue)
                        workSheet.Cells[row, 4].Value = person.DateOfBirth.Value.ToString("yyyy-MM-dd");
                    else
                        workSheet.Cells[row, 4].Value = null;
                    workSheet.Cells[row, 5].Value = person.Age;
                    workSheet.Cells[row, 6].Value = person.Gender;
                    workSheet.Cells[row, 7].Value = person.CountryName;
                    workSheet.Cells[row, 8].Value = person.Address;
                    workSheet.Cells[row, 9].Value = person.ReceiveNewsLetters;

                    row++;
                }

                workSheet.Cells[$"A1:I{row}"].AutoFitColumns();

                await package.SaveAsync();
                memoryStream.Position = 0;
                return memoryStream;

            }
        }
    }
}
