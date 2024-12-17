using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts.DTO.Enums;
using Entities;
using System.ComponentModel.DataAnnotations;
namespace ServiceContracts.DTO
{
    /// <summary>
    ///     DTO
    /// </summary>
    public class PersonAddRequest
    {
        [Required(ErrorMessage = "Person Name can't be blank")]
        public string? PersonName { get; set; }
        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage = "Email value should be a valid email")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Date of Birth can't be blank")]
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage = "Choose your gender")]
        public GenderOptions? Gender { get; set; }
        [Required(ErrorMessage = "Choose your country")]
        public Guid? CountryID { get; set; }
        [Required(ErrorMessage = "Address can't be blank")]
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        public Person ToPerson()
        {
            return new Person()
            {
                PersonName = PersonName, Email = Email, DateOfBirth = DateOfBirth, Gender = Gender.ToString(), CountryID = CountryID, Address = Address, ReceiveNewsLetters = ReceiveNewsLetters
            };
        }
    }
}
