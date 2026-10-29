using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(descriptions => descriptions.First());
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Project Service API",
        Version = "v1"
    });
});

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
