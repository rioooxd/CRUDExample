using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilter> _logger;
        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger) 
        { 
            _logger = logger;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //After logic
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //Before logic
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");

            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);
                if (!string.IsNullOrEmpty(searchBy))
                {
                    var searchByOptions = new List<string>()
                    {
                         nameof(PersonResponse.PersonName), 
                         nameof(PersonResponse.Email),
                         nameof(PersonResponse.DateOfBirth), 
                         nameof(PersonResponse.Age), 
                         nameof(PersonResponse.Gender),
                         nameof(PersonResponse.CountryID),
                         nameof(PersonResponse.Address)
                    };
                    //reset the searchBy parameter value
                    if (!searchByOptions.Any(temp => temp == searchBy))
                    {
                        _logger.LogInformation("searchBy actual value {searchBy}", searchBy);
                        context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                        _logger.LogInformation("searchBy updated value {searchBy}", context.ActionArguments["searchBy"]);
                    }
                }
            }
        }
    }
}
