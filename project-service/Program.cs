using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using project_service.Controllers;
using project_service.Data;
using project_service.Models.Entities;
using static project_service.Controllers.ProjectController;
using project_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(options =>
//{
//    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
//});

builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Project Service API",
        Version = "v1"
    });
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



//builder.Services.AddHttpClient<UserServiceClient>(client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5218/user-service/api/");

//});
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}

//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "user-service v1");
//    c.RoutePrefix = string.Empty; // Accéder à Swagger via l'URL racine
//});

////app.UseExceptionHandler(new ExceptionHandlerOptions
////{
////    ExceptionHandlingPath = "/error",
////    AllowStatusCode404Response = true
////});
//app.UseExceptionHandler(errorApp =>
//{
//    errorApp.Run(async context =>
//    {
//        context.Response.StatusCode = context.Response.StatusCode == 404 ? 404 : 500;
//        context.Response.ContentType = "application/json";

//        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

//        await context.Response.WriteAsync(new
//        {
//            error = exception?.Message ?? "An unknown error occurred."
//        }.ToString());
//    });
//});
var userServiceUrl = builder.Configuration["MicroserviceUrls:UserService"]
    ?? throw new InvalidOperationException("Configuration 'MicroserviceUrls:UserService' is required.");

builder.Services.AddHttpClient<UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(userServiceUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Project Service API v1"));
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(new
    {
        title = "An unexpected error occurred.",
        status = StatusCodes.Status500InternalServerError,
        traceId = context.TraceIdentifier
    });
}));


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();
