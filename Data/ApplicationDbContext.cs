using FleetManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Maintenance> MaintenanceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vehicle → Trips (one-to-many)
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.Trips)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Driver → Trips (one-to-many)
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany(d => d.Trips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle → Maintenance (one-to-many)
            modelBuilder.Entity<Maintenance>()
                .HasOne(m => m.Vehicle)
                .WithMany(v => v.MaintenanceRecords)
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision for Cost
            modelBuilder.Entity<Maintenance>()
                .Property(m => m.Cost)
                .HasPrecision(10, 2);

            // ── Seed Data ──────────────────────────────────────────

            // Vehicles
            modelBuilder.Entity<Vehicle>().HasData(
                new Vehicle { Id = 1, Make = "Toyota", Model = "Hilux", Year = 2021, LicensePlate = "DHK-1234", Status = "Active", Mileage = 32000 },
                new Vehicle { Id = 2, Make = "Ford", Model = "Transit", Year = 2020, LicensePlate = "DHK-5678", Status = "Active", Mileage = 54000 },
                new Vehicle { Id = 3, Make = "Mitsubishi", Model = "Canter", Year = 2019, LicensePlate = "DHK-9999", Status = "InMaintenance", Mileage = 87000 }
            );

            // Drivers
            modelBuilder.Entity<Driver>().HasData(
                new Driver { Id = 1, FirstName = "Rahim", LastName = "Uddin", LicenseNumber = "DL-001-2021", LicenseExpiry = new DateTime(2026, 6, 1), Phone = "01711-000001", Status = "Active" },
                new Driver { Id = 2, FirstName = "Karim", LastName = "Hossain", LicenseNumber = "DL-002-2020", LicenseExpiry = new DateTime(2025, 12, 31), Phone = "01711-000002", Status = "Active" },
                new Driver { Id = 3, FirstName = "Jamal", LastName = "Sheikh", LicenseNumber = "DL-003-2019", LicenseExpiry = new DateTime(2024, 8, 15), Phone = "01711-000003", Status = "Inactive" }
            );

            // Trips
            modelBuilder.Entity<Trip>().HasData(
                new Trip { Id = 1, VehicleId = 1, DriverId = 1, Origin = "Dhaka", Destination = "Chittagong", StartTime = new DateTime(2024, 1, 10, 8, 0, 0), EndTime = new DateTime(2024, 1, 10, 14, 0, 0), DistanceKm = 264, Status = "Completed" },
                new Trip { Id = 2, VehicleId = 2, DriverId = 2, Origin = "Dhaka", Destination = "Sylhet", StartTime = new DateTime(2024, 1, 15, 9, 0, 0), EndTime = new DateTime(2024, 1, 15, 15, 30, 0), DistanceKm = 240, Status = "Completed" },
                new Trip { Id = 3, VehicleId = 1, DriverId = 1, Origin = "Dhaka", Destination = "Khulna", StartTime = new DateTime(2024, 2, 1, 7, 0, 0), EndTime = null, DistanceKm = 0, Status = "Planned" }
            );

            // Maintenance
            modelBuilder.Entity<Maintenance>().HasData(
                new Maintenance { Id = 1, VehicleId = 3, ServiceType = "Engine Overhaul", ServiceDate = new DateTime(2024, 1, 20), NextServiceDate = new DateTime(2024, 7, 20), Notes = "Full engine check and oil change", Cost = 15000, Status = "Completed" },
                new Maintenance { Id = 2, VehicleId = 1, ServiceType = "Tire Replacement", ServiceDate = new DateTime(2024, 2, 5), NextServiceDate = new DateTime(2025, 2, 5), Notes = "All 4 tires replaced", Cost = 8000, Status = "Completed" },
                new Maintenance { Id = 3, VehicleId = 2, ServiceType = "Brake Service", ServiceDate = new DateTime(2024, 3, 1), NextServiceDate = null, Notes = "Brake pads inspection due", Cost = 0, Status = "Scheduled" }
            );
        }
    }
}