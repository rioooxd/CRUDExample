using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
namespace RepositoryContracts
{
    /// <summary>
    /// Represents data access logic for managing country entity
    /// </summary>
    public interface ICountriesRepository
    {
        Task<Country> AddCountry(Country country);
        Task<List<Country>> GetAllCountries();
        Task<Country?> GetCountryByCountryID(Guid? countryID);

        Task<Country?> GetCountryByCountryName(string countryName);
    }
}
