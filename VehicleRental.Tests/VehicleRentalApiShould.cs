using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VehicleRental.VehicleRentalApi;

namespace VehicleRental.Tests;

public class VehicleRentalApiShould
{
    WebApplicationFactory<Program> InitializeWebApplicationFactory(Func<VehicleRentalDbContext, VehicleRentalDbContext>? prepareDbContext = null)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                services.AddDbContext<VehicleRentalDbContext>(options => options.UseInMemoryDatabase("VehicleRentalDb" + Guid.NewGuid()));
                var dbContext = scope.ServiceProvider.GetRequiredService<VehicleRentalDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                if (prepareDbContext != null)
                {
                    prepareDbContext(dbContext).SaveChanges();
                }
            });
        });
    }
    [Fact]
    public async Task Return_AllVehicles_When_No_Reservations_Created()
    {
        var client = InitializeWebApplicationFactory((dbContext) =>
        {
            dbContext.Vehicles.RemoveRange(dbContext.Vehicles);
            dbContext.Vehicles.AddRange(
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 1", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 2", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 3", Type = VehicleType.Compact });
            return dbContext;
        }).CreateClient();
        var response = await client.GetAsync("/api/v1/reservations?pickupDate=1/1/2020&returnDate=1/2/2022");
        var responseMessage = response.EnsureSuccessStatusCode();
        var vehicleAvailabilities = await responseMessage.Content.ReadFromJsonAsync<IEnumerable<VehicleAvailability>>();
        vehicleAvailabilities?.SelectMany(v => v.AvailableVehicles).Count().Should().Be(3);
    }

    [Fact]
    public async Task Return_Expected_Reservations_When_A_Reservation_Created()
    {
        var pickupDate = "1/1/2020";
        var returnDate = "1/2/2022";
        var client = InitializeWebApplicationFactory((dbContext) =>
        {
            dbContext.Vehicles.RemoveRange(dbContext.Vehicles);
            dbContext.Vehicles.AddRange(
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 1", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 2", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 3", Type = VehicleType.Compact });
            return dbContext;
        }).CreateClient();
        var createReservationResponse = await client.PostAsJsonAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleType=Compact", new { });
        var reservationsResponse = await client.GetAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleTypes=Compact");
        var reservationsResponseMessage = reservationsResponse.EnsureSuccessStatusCode();
        var vehicleAvailabilities = await reservationsResponseMessage.Content.ReadFromJsonAsync<IEnumerable<VehicleAvailability>>();
        vehicleAvailabilities?.SelectMany(v => v.AvailableVehicles).Count().Should().Be(2);
    }

    [Fact]
    public async Task Return_Expected_Reservations_When_A_Reservation_For_Different_Vehicle_Type_Created()
    {
        var pickupDate = "1/1/2020";
        var returnDate = "1/2/2022";
        var client = InitializeWebApplicationFactory((dbContext) =>
        {
            dbContext.Vehicles.RemoveRange(dbContext.Vehicles);
            dbContext.Vehicles.AddRange(
                new Vehicle { Id = Guid.NewGuid(), Description = "SUV Vehicle 1", Type = VehicleType.SUV },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 1", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 2", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 3", Type = VehicleType.Compact });
            return dbContext;
        }).CreateClient();
        var createReservationResponse = await client.PostAsJsonAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleType=SUV", new { });
        var reservationsResponse = await client.GetAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleTypes=Compact");
        var reservationsResponseMessage = reservationsResponse.EnsureSuccessStatusCode();
        var vehicleAvailabilities = await reservationsResponseMessage.Content.ReadFromJsonAsync<IEnumerable<VehicleAvailability>>();
        vehicleAvailabilities?.SelectMany(v => v.AvailableVehicles).Count().Should().Be(3);
    }

    [Fact]
    public async Task Return_Expected_Reservations_When_A_Reservation_For_Intersected_Dates_Created()
    {
        var pickupDate = "1/1/2020";
        var returnDate = "1/1/2024";
        var client = InitializeWebApplicationFactory((dbContext) =>
        {
            dbContext.Vehicles.RemoveRange(dbContext.Vehicles);
            dbContext.Vehicles.AddRange(
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 1", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 2", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 3", Type = VehicleType.Compact });
            return dbContext;
        }).CreateClient();
        var createReservationResponse = await client.PostAsJsonAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleType=Compact", new { });
        var reservationsResponse = await client.GetAsync($"/api/v1/reservations?pickupDate=1/1/2021&returnDate=1/1/2022&vehicleTypes=Compact");
        var reservationsResponseMessage = reservationsResponse.EnsureSuccessStatusCode();
        var vehicleAvailabilities = await reservationsResponseMessage.Content.ReadFromJsonAsync<IEnumerable<VehicleAvailability>>();
        vehicleAvailabilities?.SelectMany(v => v.AvailableVehicles).Count().Should().Be(2);
    }

    [Fact]
    public async Task Return_Expected_Reservations_When_A_Reservation_For_NonIntersected_Dates_Created()
    {
        var pickupDate = "1/1/2020";
        var returnDate = "1/1/2021";
        var client = InitializeWebApplicationFactory((dbContext) =>
        {
            dbContext.Vehicles.RemoveRange(dbContext.Vehicles);
            dbContext.Vehicles.AddRange(
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 1", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 2", Type = VehicleType.Compact },
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 3", Type = VehicleType.Compact });
            return dbContext;
        }).CreateClient();
        var createReservationResponse = await client.PostAsJsonAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleType=Compact", new { });
        var reservationsResponse = await client.GetAsync($"/api/v1/reservations?pickupDate=1/1/2022&returnDate=1/1/2023&vehicleTypes=Compact");
        var reservationsResponseMessage = reservationsResponse.EnsureSuccessStatusCode();
        var vehicleAvailabilities = await reservationsResponseMessage.Content.ReadFromJsonAsync<IEnumerable<VehicleAvailability>>();
        vehicleAvailabilities?.SelectMany(v => v.AvailableVehicles).Count().Should().Be(3);
    }

    [Fact]
    public async Task Return_Error_When_Reserving_Unavailable_Vehicle_Type()
    {
        var pickupDate = "1/1/2020";
        var returnDate = "1/1/2021";
        var client = InitializeWebApplicationFactory((dbContext) =>
        {
            dbContext.Vehicles.RemoveRange(dbContext.Vehicles);
            dbContext.Vehicles.AddRange(
                new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 1", Type = VehicleType.Compact });
            return dbContext;
        }).CreateClient();
        var firstReservation = await client.PostAsJsonAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleType=Compact", new { });
        var secondReservation = await client.PostAsJsonAsync($"/api/v1/reservations?pickupDate={pickupDate}&returnDate={returnDate}&vehicleType=Compact", new { });
        secondReservation.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}