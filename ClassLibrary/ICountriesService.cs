using Entities;
using ServiceContracts.DTO;
using Microsoft.AspNetCore.Http;
namespace ClassLibrary
{
    /// <summary>
    /// Business logic
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        /// Adds a country object to the list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to add</param>
        /// <returns>Country Response</returns>
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

        /// <summary>
        /// Returns all countries from the list of countries
        /// </summary>
        /// <returns>All countries from the list as list of CountryResponse</returns>
        Task<List<CountryResponse>> GetAllCountries();

        /// <summary>
        /// Returns country object by given CountryID
        /// </summary>
        /// <param name="countryID"></param>
        /// <returns>Countryresponse</returns>
        Task<CountryResponse?> GetCountryByCountryID(Guid? countryID);

        Task<int> UploadCountriesFromExcelFile(IFormFile formFile);

    }
}
