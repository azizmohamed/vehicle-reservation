using System.Text.Json.Serialization;
using VehicleRental.VehicleRentalApi;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Vehicle Rental Api", Version = "v1" });
});
builder.Services.AddDbContext<VehicleRentalDbContext>(options => options.UseInMemoryDatabase("VehicleRentalDb"));
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddScoped<IReservationService, ReservationService>();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VehicleRentalDbContext>();
    dbContext.Database.EnsureCreated();  // This triggers database creation and invokes OnModelCreating
}

app.UseExceptionHandler(x =>
{
    x.Run(async ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";
        var error = ctx.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var errorMessage = !app.Environment.IsProduction()
                ? new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = error.Error.Message, Detail = error.Error.StackTrace }
                : new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "Something went wrong. We couldn't process your request" };

            await ctx.Response.WriteAsJsonAsync(errorMessage);
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Serve Swagger UI at the root "/"
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // This serves the Swagger UI at the root URL
});

Api.RegisterEndPoints(app);
app.Run();
