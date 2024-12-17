using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO;
using ServiceContracts.DTO.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Business logic for person entity
    /// </summary>
    public interface IPersonsService
    {
        /// <summary>
        /// Adds a new person into the list of persons
        /// </summary>
        /// <param name="personAddRequest"></param>
        /// <returns></returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);

        /// <summary>
        /// Gets all persons from the persons list
        /// </summary>
        /// <returns></returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns the person object by given id
        /// </summary>
        /// <param name="personID"></param>
        /// <returns></returns>
        Task<PersonResponse?> GetPersonByPersonID(Guid? personID);

        /// <summary>
        /// Returns all person objs that match with the given search field
        /// </summary>
        /// <param name="searchBy">Search field</param>
        /// <param name="searchString">Search string</param>
        /// <returns>Returns all matching persons</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

        /// <summary>
        /// Returns sorted list of persons
        /// </summary>
        /// <param name="persons">List of persons to sort</param>
        /// <param name="sortBy">Sort parameter</param>
        /// <param name="sortOrder">Sort order</param>
        /// <returns></returns>
        Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> persons, string sortBy, SortOrderOptions sortOrder);

        /// <summary>
        /// Updates the specified person based on the given personID
        /// </summary>
        /// <param name="personUpdateRequest"></param>
        /// <returns>Person response</returns>
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

        /// <summary>
        /// Deletes person by given PersonID
        /// </summary>
        /// <param name="personID"></param>
        /// <returns>true if deletion is succesful, otherwise false</returns>
        Task<bool> DeletePerson(Guid? personID);

        Task<MemoryStream> GetPersonsCSV();

        Task<MemoryStream> GetPersonsExcel();

    }
}
