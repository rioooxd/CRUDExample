using ClassLibrary;
using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using Repositories;
using Serilog;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.ResultFilters;
using CRUDExample;
using CRUDExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {
    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
        //read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services);
        //read out current apps services and make them available to serilog
});

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();



app.UseSerilogRequestLogging();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
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
