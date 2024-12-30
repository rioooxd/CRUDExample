using ClassLibrary;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
namespace CRUDExample.Filters.ActionFilters
{
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
    {
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonCreateAndEditPostActionFilter> _logger;
        public PersonCreateAndEditPostActionFilter(ICountriesService countriesService, ILogger<PersonCreateAndEditPostActionFilter> logger)
        { 
            _countriesService = countriesService;
            _logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {       
            if(context.Controller is PersonsController personsController)
            {
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> countries = await _countriesService.GetAllCountries();
                    personsController.ViewBag.Countries = countries.Select(temp => new SelectListItem() { Text = temp.CountryName, Value = temp.CountryId.ToString() });
                    personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    var personRequest = context.ActionArguments["personRequest"];
                    context.Result = personsController.View(personRequest); //short circuit
                }
                else
                {
                    await next();
                }
            }
            else
            {
                await next();
            }
            //after logic tra
            _logger.LogInformation("In after logic of PersonsCreateAndEdit Action Filter");
        }
    }
}
