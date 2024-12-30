using AutoFixture;
using ClassLibrary;
using Moq;
using ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using CRUDExample.Controllers;
using ServiceContracts.DTO;
using ServiceContracts.DTO.Enums;
using Microsoft.AspNetCore.Mvc;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;


        private readonly Mock<ILogger<PersonsController>> _loggerMock;
        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IPersonsService> _personsServiceMock;


        private readonly Fixture _fixture;

        public PersonsControllerTest()
        {
            _fixture = new Fixture();

            _countriesServiceMock = new Mock<ICountriesService>();
            _personsServiceMock = new Mock<IPersonsService>();
            _loggerMock = new Mock<ILogger<PersonsController>>();

            _logger = _loggerMock.Object;
            _personsService = _personsServiceMock.Object;
            _countriesService = _countriesServiceMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ShoudReturnIndexViewWithPersonsList()
        {
            //Arrange
            List<PersonResponse> personsResponseList = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            _personsServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personsResponseList);

            _personsServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
                .ReturnsAsync(personsResponseList);

            //Act
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(personsResponseList);
        }
        #endregion
        #region Create
        [Fact]
        public async Task Create_NoModelErrors_ShouldReturnRedirectToAction()
        {
            //Arrange 
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();

            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);


            //Act

            IActionResult result = await personsController.Create(personAddRequest);

            //Assert 
            RedirectToActionResult redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            redirectToActionResult.ActionName.Should().Be("Index");


        }
        #endregion
        #region Edit
        [Fact]
        public async Task Edit_NoModelErrors_ShouldReturnRedirectToAction()
        {
            //Arrange
            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            PersonUpdateRequest personUpdateRequest = _fixture.Create<PersonUpdateRequest>();
            
            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryList = _fixture.Create<List<CountryResponse>>();

            _personsServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>()))
                .ReturnsAsync(personResponse);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countryList);
            
            //Act
            IActionResult actionResult = await personsController.Edit(personUpdateRequest, personUpdateRequest.PersonID);

            //Assert
            RedirectToActionResult result = Assert.IsType<RedirectToActionResult>(actionResult);

            result.ActionName.Should().Be("Index");


        }
        /*
        [Fact]
        public async Task Edit_ModelErrors_ShouldReturnViewResult()
        {
            //Arrange
            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            PersonUpdateRequest personUpdateRequest = _fixture.Build<PersonUpdateRequest>()
                .With(p=>p.Gender, GenderOptions.Male)
                .Create();
            
            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryList = _fixture.Create<List<CountryResponse>>();

            _personsServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>()))
                .ReturnsAsync(personResponse);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countryList);

            personsController.ModelState.AddModelError("PersonName", "Person name is required");
            
            //Act
            IActionResult actionResult = await personsController.Edit(personUpdateRequest, personUpdateRequest.PersonID);

            //Assert
            ViewResult result = Assert.IsType<ViewResult>(actionResult);

            result.Model.Should().BeAssignableTo<PersonUpdateRequest>();
            result.Model.Should().Be(personUpdateRequest);
        }
        */
        #endregion
    }
}
