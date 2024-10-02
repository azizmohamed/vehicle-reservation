using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VehicleRental.VehicleRentalApi;

public static class Api
{
    public static void RegisterEndPoints(this WebApplication webApplication)
    {
        var group = webApplication
            .MapGroup("api/v1/reservations")
            .WithOpenApi();

        group.MapGet("/", async ([FromQuery] DateOnly pickupDate, [FromQuery] DateOnly returnDate, [FromQuery] VehicleType[]? vehicleTypes, [FromServices] IReservationService reservationService) =>
        {
            var VehicleAvailabilities = await reservationService.GetVehicleAvailabilities(pickupDate, returnDate, vehicleTypes);
            return Results.Ok(VehicleAvailabilities.Select(v => new VehicleAvailability
            {
                Type = v.Type,
                AvailableVehicles = v.AvailableVehicles
            }));
        })
        .WithName("GetVehicleAvailabilities")
        .Produces<IEnumerable<VehicleAvailability>>(StatusCodes.Status200OK);


        group.MapPost("/", async ([FromQuery] DateOnly pickupDate, [FromQuery] DateOnly returnDate, [FromQuery] VehicleType vehicleType, [FromServices] IReservationService reservationService) =>
        {
            var reservationStatus = await reservationService.ReserveVehicle(pickupDate, returnDate, vehicleType);
            return reservationStatus switch
            {
                ReservatioinStatus.Success => Results.Created(),
                ReservatioinStatus.NotAvailable => Results.BadRequest("No vehicles available for the selected type and dates"),
                ReservatioinStatus.Conflict => Results.Conflict("Another user has updated the vehicle. Please refresh and try again."),
                _ => Results.BadRequest("Invalid reservation status")
            };
        })
        .WithName("ReserveVehicle")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
    }


}
