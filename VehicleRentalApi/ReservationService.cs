using Microsoft.EntityFrameworkCore;

namespace VehicleRental.VehicleRentalApi;
public interface IReservationService
{
    Task<List<VehicleAvailability>> GetVehicleAvailabilities(DateOnly pickupDate, DateOnly returnDate, VehicleType[]? vehicleTypes);
    Task<ReservatioinStatus> ReserveVehicle(DateOnly pickupDate, DateOnly returnDate, VehicleType vehicleType);
}

public class ReservationService(VehicleRentalDbContext vehicleRentalDbContext) : IReservationService
{
    public async Task<List<VehicleAvailability>> GetVehicleAvailabilities(DateOnly pickupDate, DateOnly returnDate, VehicleType[]? vehicleTypes)
    {
        var vehicleAvailabilities = await vehicleRentalDbContext.Vehicles
            .Where(v => vehicleTypes == null || vehicleTypes.Count() == 0 || vehicleTypes.Contains(v.Type))
            .GroupBy(v => v.Type)
            .Select(g => new VehicleAvailability
            {
                Type = g.Key,
                AvailableVehicles = g.Where(v => !v.VehicleReservations.Any(r => (pickupDate >= r.PickupDate && pickupDate <= r.ReturnDate)
                    || (returnDate >= r.PickupDate && returnDate <= r.ReturnDate)))
            }).ToListAsync();
        return vehicleAvailabilities;
    }

    public async Task<ReservatioinStatus> ReserveVehicle(DateOnly pickupDate, DateOnly returnDate, VehicleType vehicleType)
    {
        var vehicleAvailabilities = await GetVehicleAvailabilities(pickupDate, returnDate, [vehicleType]);
        if (!vehicleAvailabilities.First().AvailableVehicles.Any())
        {
            return ReservatioinStatus.NotAvailable;
        }

        var availableVehicle = vehicleAvailabilities.First().AvailableVehicles.First();
        //This update should trigger the rowversion update for concurrency check
        availableVehicle.LatestReservationDateTime = DateTimeOffset.UtcNow; 
        vehicleRentalDbContext.VehicleReservations.Add(new VehicleReservation
        {
            Id = Guid.NewGuid(),
            VehicleId = availableVehicle.Id,
            Vehicle = availableVehicle,
            PickupDate = pickupDate,
            ReturnDate = returnDate
        });
        try
        {
            await vehicleRentalDbContext.SaveChangesAsync();
            return ReservatioinStatus.Success;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle the concurrency conflict
            return ReservatioinStatus.Conflict;
        }
    }
}
