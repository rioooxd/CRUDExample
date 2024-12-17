using ClassLibrary;
using CRUDExample.Filters.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.DTO.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class PersonsController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;
        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _personsService = personsService;
            _countriesService = countriesService;
            _logger = logger;
        }
        [Route("[action]")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter))]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("Index action method of PersonsController");

            _logger.LogDebug("sortBy: " + sortBy, "searchtBy: " + searchBy, "searchString: "  + searchString, "sortOrder: " + sortOrder);

            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                { nameof(PersonResponse.PersonName), "Person Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                { nameof(PersonResponse.Age), "Age" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.CountryID), "Country" },
                { nameof(PersonResponse.Address), "Address" }
            };
            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;


            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();
            return View(sortedPersons);
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp => new SelectListItem(){ 
                Text = temp.CountryName, Value = temp.CountryId.ToString()
            });  
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                var countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(personAddRequest);
            }
            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);
            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Edit(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);
            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp => new SelectListItem()
            {
                Text = temp.CountryName,
                Value = temp.CountryId.ToString()
            });
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            return View(personUpdateRequest);
        }
        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest, Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                PersonResponse updatedPerson = await _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }
            else
            {
                var countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(personResponse.ToPersonUpdateRequest());
            }
        }
        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);
            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }
        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if(personResponse == null)
            {
                return RedirectToAction("Index");
            }
            
            await _personsService.DeletePerson(personUpdateRequest.PersonID);
            return RedirectToAction("Index");

        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPDF()
        {
            List<PersonResponse> personResponses = await _personsService.GetAllPersons();

            return new ViewAsPdf("PersonsPDF", personResponses, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins()
                {
                    Top = 20, Right = 20, Bottom = 20, Left = 20,
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }
        [Route("[action]")]
        public async Task<IActionResult> PersonsCSV()
        {
            MemoryStream stream = await _personsService.GetPersonsCSV();

            return File(stream, "application/octet-stream");
        }
        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream stream = await _personsService.GetPersonsExcel();

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }
    }
}
