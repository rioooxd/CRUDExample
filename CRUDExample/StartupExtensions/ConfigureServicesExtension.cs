

using ClassLibrary;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.ResultFilters;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ResponseHeaderActionFilter>();

            services.AddControllersWithViews(options => {
                //options.Filters.Add<ResponseHeaderActionFilter>();
                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();
                options.Filters.Add(new ResponseHeaderActionFilter(logger) { Key = "MyKeyGlobal", Value = "MyValueGlobal", Order = 2 });
            });

            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();
            services.AddScoped<ICountriesService, CountriesService>();
            services.AddScoped<IPersonsService, PersonsService>();


            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"));
                options.EnableSensitiveDataLogging();
            });

            services.AddTransient<PersonsListResultFilter>();

            return services;
        }
    }
}
