using ClassLibrary;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTO;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFrameworkCoreMock;
using Moq;
using FluentAssertions;
using RepositoryContracts;
using AutoFixture;
namespace CRUDTests
{
    public class CountriesServiceTest
    {

        private readonly ICountriesService _countriesService;
        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;

        private Fixture _fixture;

        public CountriesServiceTest()
        {
            //seed data
            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;

            _countriesService = new CountriesService(_countriesRepository);

            _fixture = new Fixture();
        }
        #region AddCountry
        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentNullException()
        {
            //Arrange
            CountryAddRequest request = null;
            Country country = _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>).Create();

            _countriesRepositoryMock
                .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);

            //Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(null);

            };
            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() 
            { 
                CountryName = null
            };

            Country country = _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>).Create();

            _countriesRepositoryMock
                .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);

            //Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(request);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest? request1 = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "Example")
                .Create();
            CountryAddRequest? request2 = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "Example")
                .Create();

            Country country1 = request1.ToCountry();
            Country country2 = request2.ToCountry();

            _countriesRepositoryMock
                .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country1);

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync(null as Country);

            CountryResponse countryResponse1 = await _countriesService.AddCountry(request1);

            //Act
            Func<Task> action = async () =>
            {
                _countriesRepositoryMock
                .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country2);

                _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                    .ReturnsAsync(country1);

                await _countriesService.AddCountry(request2);
            };
            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }
        [Fact]
        public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
        {
            //Arrange
            CountryAddRequest request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "Japan")
                .Create();

            Country country = request.ToCountry();
            CountryResponse countryResponse = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
                .ReturnsAsync(null as Country);

            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);

            CountryResponse responseActual = await _countriesService.AddCountry(request);

            country.CountryID = responseActual.CountryId;
            countryResponse.CountryId = responseActual.CountryId;
            //Assert

            responseActual.CountryId.Should().NotBe(Guid.Empty);
            responseActual.Should().Be(countryResponse);
        }
        #endregion
        #region GetAllCountries        
        [Fact]
        public async Task GetAllCountries_EmptyList_ToBeEmptyList()
        {
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(new List<Country>());

            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();

            actual_country_response_list.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllCountries_ShouldHaveFewCountries()
        {
            //Arrange
            List<Country> countries = new List<Country>()
            {
                _fixture.Build<Country>()
                    .With(temp=>temp.Persons, null as List<Person>)
                    .Create(),         
                _fixture.Build<Country>()
                    .With(temp=>temp.Persons, null as List<Person>)
                    .Create()
            };
            //Act

            List<CountryResponse> countriesResponses = countries.Select(temp => temp.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            actualCountryResponseList.Should().BeEquivalentTo(countriesResponses);

        }

        #endregion

        #region GetCountryByCountryID

        [Fact]
        public async Task GetCountryByCountryID_ToBeArgumentNullException()
        {
            Guid? countryID = null;

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
                .ReturnsAsync(null as Country);

            CountryResponse? countryResponse = await _countriesService.GetCountryByCountryID(countryID);

            countryResponse.Should().BeNull();
        }
        [Fact]
        public async Task GetCountryByCountryID_ToBeSuccessful()
        {
            Country country = _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create();

            CountryResponse countryResponseExpected = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
                .ReturnsAsync(country);

            CountryResponse? countryResponseActual = await _countriesService.GetCountryByCountryID(countryResponseExpected.CountryId);


            countryResponseActual.Should().Be(countryResponseExpected);
        }
        #endregion
    }
}
