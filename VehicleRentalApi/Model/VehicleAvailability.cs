namespace VehicleRental.VehicleRentalApi;

public record VehicleAvailability
{
    public VehicleType Type { get; init; }
    public IEnumerable<Vehicle> AvailableVehicles { get; init; } = Enumerable.Empty<Vehicle>();
}
