using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContracts
{
    /// <summary>
    /// represents data access logic for managing person entity
    /// </summary>
    public interface IPersonsRepository
    {
        /// <summary>
        /// Adds a person to the table
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        Task<Person> AddPerson(Person person);
        /// <summary>
        /// Returns all persons from the table
        /// </summary>
        /// <returns></returns>
        Task<List<Person>> GetAllPersons();

        /// <summary>
        /// Returns a person with matching personID
        /// </summary>
        /// <param name="personID"></param>
        /// <returns></returns>
        Task<Person?> GetPersonByPersonID(Guid personID);

        /// <summary>
        /// Returns all person objects based on the given expression
        /// </summary>
        /// <param name="predicate">Lambda expressions</param>
        /// <returns></returns>
        Task<List<Person?>> GetFilteredPersons(Expression<Func<Person, bool>> predicate);
        /// <summary>
        /// Deletes person with matching personID
        /// </summary>
        /// <param name="personID"></param>
        /// <returns></returns>
        /// 
        Task<bool> DeletePersonByPersonID(Guid personID);

        /// <summary>
        /// Updates a person object with a matching personID
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        Task<Person> UpdatePerson(Person person);

    }
}
