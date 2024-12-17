using ServiceContracts;
using System;
using System.Collections.Generic;
using Xunit;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts.DTO.Enums;
using Services;
using ClassLibrary;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using FluentAssertions.Execution;
using System.Linq.Expressions;
using Serilog.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;
namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;
        private readonly IPersonsRepository _personsRepository;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly ITestOutputHelper _outputHelper;
        private readonly IFixture _fixture;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();

            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;

            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            var loggerMock = new Mock<ILogger<PersonsService>>();

            _personsService = new PersonsService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);
            _outputHelper = testOutputHelper;

            
        }
        
        #region AddPerson

        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            PersonAddRequest? personAddRequest = null;
            Func<Task> action = async () =>
            {
                await _personsService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, null as string)
                .Create();

            Person person = personAddRequest.ToPerson();

            _personsRepositoryMock
                .Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
            Func<Task> action = async () =>
            {
                await _personsService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();

        }
        [Fact]
        public async Task AddPerson_InvalidEmail_ToBeArgumentException()
        {
            PersonAddRequest? personAddRequest = _fixture.Create<PersonAddRequest>();

            Person person = personAddRequest.ToPerson();

            _personsRepositoryMock
               .Setup(temp => temp.AddPerson(It.IsAny<Person>()))
               .ReturnsAsync(person);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {

                await _personsService.AddPerson(personAddRequest);
            });


        }
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccesful()
        {
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someone@example.com")
                .Create();

            Person person = personAddRequest.ToPerson();
            //If we supply any argument value to the AddPerson method, it should return the same return value
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            PersonResponse personResponseExpected = person.ToPersonResponse();
            PersonResponse personResponseActual = await _personsService.AddPerson(personAddRequest);
            personResponseExpected.PersonID = personResponseActual.PersonID;

            personResponseActual.PersonID.Should().NotBe(Guid.Empty);

            personResponseActual.Should().Be(personResponseExpected);

        }

        #endregion

        #region GetPersonPersonID
        //Null person id
        [Fact]
        public async Task GetPersonByPersonID_InvalidID_ToBeNull()
        {
            Guid? personID = null;

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            personResponse.Should().BeNull();

        }
        [Fact]
        public async Task GetPersonByPersonID_WithPersonID_ToBeSuccessful()
        {
            //Arange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();
            PersonResponse person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock
                .Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);
            //Act
            PersonResponse? personResponse_get = await _personsService.GetPersonByPersonID(person.PersonID);
            //Assert

            personResponse_get.Should().Be(person_response_expected);
        }

        #endregion

        #region GetAllPersons
        [Fact]
        public async Task GetAllPersons_EmptyList_ToBeSuccessful()
        {
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(new List<Person>());

            List<PersonResponse> persons_from_get = await _personsService.GetAllPersons();

            persons_from_get.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllPersons_AddFewPersons_ToBeSuccessful()
        {
            //Arrange
          
            Person person1 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            Person person2 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            List<Person> persons = new List<Person>()
            {
                person1, person2
            };

            List<PersonResponse> personResponseListFromAdd = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);
            //Act

            _outputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response in personResponseListFromAdd)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }

            List<PersonResponse> personGetAll = await _personsService.GetAllPersons();

            _outputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in personGetAll)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }
            personGetAll.Should().BeEquivalentTo(personGetAll);
        }


        #endregion

        #region GetFilteredPersons
        // IF the search text is empty and search by is "PersonName, it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {


            Person person1 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            Person person2 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            List<Person> persons = new List<Person>()
            {
                person1, person2
            };

            List<PersonResponse> personResponseListFromAdd = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp
                .GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            _outputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response in personResponseListFromAdd)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }

            List<PersonResponse> personSearchList = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");

            _outputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in personSearchList)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }
            personSearchList.Should().BeEquivalentTo(personResponseListFromAdd);
        }

        // First we add few persons, and then we search based on person name and search string
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            Person person1 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            Person person2 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            List<Person> persons = new List<Person>()
            {
                person1, person2
            };

            List<PersonResponse> personResponseListFromAdd = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp
              .GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
               .ReturnsAsync(persons);

            _outputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response in personResponseListFromAdd)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }

            List<PersonResponse> personSearchList = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "sa");

            _outputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in personSearchList)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }

            personSearchList.Should().BeEquivalentTo(personResponseListFromAdd);
        }

        #endregion

        #region GetSortedPersons

        // When we sort based on PersonName in DESC, it should return persons list in descending order 
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            Person person1 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            Person person2 = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            List<Person> persons = new List<Person>()
            {
                person1, person2
            };

            List<PersonResponse> personResponseListFromAdd = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            _outputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response in personResponseListFromAdd)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }


            List<PersonResponse> personSortList = await _personsService.GetSortedPersons(await _personsService.GetAllPersons(), nameof(Person.PersonName), SortOrderOptions.DESC);

            _outputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_response in personSortList)
            {
                _outputHelper.WriteLine(person_response.ToString());
            }
            personSortList.Should().BeInDescendingOrder(temp => temp.PersonName);

        }

        #endregion

        #region UpdatePerson
        //null expection when personupdaterequest is null
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            //Act
            Func<Task> action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
        //Invalid person id => throw argument expection
        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            PersonUpdateRequest? personUpdateRequest = _fixture.Create<PersonUpdateRequest>();

            Func<Task> action = async() =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }
        //Null personname=> throw argument expection
        [Fact]
        public async Task UpdatePerson_NullPersonName_ToBeArgumentException()
        {
            //Assert
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "someone@email.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse personResponse = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            personUpdateRequest.PersonName = null;
            Func<Task> action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }
        // Valid update person
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            //Assert
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "someone@email.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse personResponse = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            personUpdateRequest.PersonName = "William";
            personUpdateRequest.Email = "bigchungus@gmail.com";

            //Act
            PersonResponse updatedPersonResponse = await _personsService.UpdatePerson(personUpdateRequest);

            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(updatedPersonResponse.PersonID);

            updatedPersonResponse.Should().Be(personResponseFromGet);
        }
        #endregion

        #region DeletePerson
        // true because valid person id
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "someone@email.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Female")
                .Create();



            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(person);
                

            //Act

            bool isDeleted = await _personsService.DeletePerson(person.PersonID);

            //Assert
            isDeleted.Should().BeTrue();
        }   
        // false because invalid person id
        [Fact]
        public async Task DeletePerson_InvalidPersonID_ToBeSuccessful()
        {
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "someone@email.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Female")
                .Create();


            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(null as Person);

            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(false);

            //Act

            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();
        }   

        #endregion
    }
}
