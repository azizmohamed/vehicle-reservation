namespace VehicleRental.VehicleRentalApi;

public record VehicleReservation
{
    public required Guid Id { get; init; }
    public required Guid VehicleId { get; init; }
    public required Vehicle Vehicle { get; init; }
    public required DateOnly PickupDate { get; init; }
    public required DateOnly ReturnDate { get; init; }
}
