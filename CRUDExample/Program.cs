using ClassLibrary;
using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {
    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
        //read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services);
        //read out current apps services and make them available to serilog
});
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();

builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    options.EnableSensitiveDataLogging();
});
var app = builder.Build();

app.UseSerilogRequestLogging();

//Connection string
//Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonsDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("information-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");
if (!builder.Environment.IsEnvironment("Test"))
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();

public partial class Program { } // make the aut-generated Program accessible
