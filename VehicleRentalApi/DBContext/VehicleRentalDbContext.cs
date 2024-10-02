using VehicleRental.VehicleRentalApi;
using Microsoft.EntityFrameworkCore;

public class VehicleRentalDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleReservation> VehicleReservations { get; set; }

    public VehicleRentalDbContext(DbContextOptions<VehicleRentalDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 1", Type = VehicleType.Compact },
            new Vehicle { Id = Guid.NewGuid(), Description = "Sedan Vehicle 1", Type = VehicleType.Sedan },
            new Vehicle { Id = Guid.NewGuid(), Description = "SUV Vehicle 1", Type = VehicleType.SUV },
            new Vehicle { Id = Guid.NewGuid(), Description = "Van Vehicle 1", Type = VehicleType.Van },
            new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 2", Type = VehicleType.Compact },
            new Vehicle { Id = Guid.NewGuid(), Description = "Sedan Vehicle 2", Type = VehicleType.Sedan },
            new Vehicle { Id = Guid.NewGuid(), Description = "SUV Vehicle 2", Type = VehicleType.SUV },
            new Vehicle { Id = Guid.NewGuid(), Description = "Van Vehicle 2", Type = VehicleType.Van },
            new Vehicle { Id = Guid.NewGuid(), Description = "Compact Vehicle 3", Type = VehicleType.Compact },
            new Vehicle { Id = Guid.NewGuid(), Description = "Sedan Vehicle 3", Type = VehicleType.Sedan },
            new Vehicle { Id = Guid.NewGuid(), Description = "SUV Vehicle 3", Type = VehicleType.SUV },
            new Vehicle { Id = Guid.NewGuid(), Description = "Van Vehicle 3", Type = VehicleType.Van }
        );

        // Configure one-to-many relationship
        modelBuilder.Entity<VehicleReservation>()
            .HasOne(r => r.Vehicle)
            .WithMany(v => v.VehicleReservations)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .Property(v => v.RowVersion)
            .IsRowVersion();  // Configures the RowVersion property as a concurrency token
    }
}
