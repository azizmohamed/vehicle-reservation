using System.ComponentModel.DataAnnotations;

namespace VehicleRental.VehicleRentalApi;

public record Vehicle
{
    public required Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset LatestReservationDateTime { get; set; }
    public required VehicleType Type { get; init; }
    
    // Concurrency token (RowVersion)
    [ConcurrencyCheck]
    public byte[] RowVersion { get; init; } = Array.Empty<byte>();
    
    // Navigation property for the one-to-many relationship
    public List<VehicleReservation> VehicleReservations { get; set; } = new();
}
