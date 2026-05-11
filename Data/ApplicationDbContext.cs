using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FleetManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Maintenance> MaintenanceRecords { get; set; }
        public DbSet<FuelRecord> FuelRecords { get; set; }
        public DbSet<VehicleAssignment> VehicleAssignments { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<GPSTracking> GPSTracking { get; set; }
        public DbSet<Geofence> Geofences { get; set; }
        public DbSet<GeofenceAlert> GeofenceAlerts { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<ExpenseApproval> ExpenseApprovals { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentCategory> DocumentCategories { get; set; }
        public DbSet<DocumentAccessLog> DocumentAccessLogs { get; set; }
        public DbSet<DriverPerformance> DriverPerformances { get; set; }
        public DbSet<DriverIncident> DriverIncidents { get; set; }
        public DbSet<PerformanceThreshold> PerformanceThresholds { get; set; }
        public DbSet<VehicleBooking> VehicleBookings { get; set; }
        public DbSet<BookingConflict> BookingConflicts { get; set; }
        public DbSet<VehicleAvailability> VehicleAvailabilities { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SecurityEvent> SecurityEvents { get; set; }
        public DbSet<DataAccessLog> DataAccessLogs { get; set; }

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

            // Vehicle → FuelRecords (one-to-many)
            modelBuilder.Entity<FuelRecord>()
                .HasOne(f => f.Vehicle)
                .WithMany(v => v.FuelRecords)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // VehicleAssignment relationships
            modelBuilder.Entity<VehicleAssignment>()
                .HasOne(va => va.Vehicle)
                .WithMany()
                .HasForeignKey(va => va.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleAssignment>()
                .HasOne(va => va.Driver)
                .WithMany()
                .HasForeignKey(va => va.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision for FuelRecord
            modelBuilder.Entity<FuelRecord>()
                .Property(f => f.PricePerLiter)
                .HasPrecision(10, 2);

            modelBuilder.Entity<FuelRecord>()
                .Property(f => f.TotalCost)
                .HasPrecision(10, 2);

            
            // GPS Tracking relationships and precision
            modelBuilder.Entity<GPSTracking>()
                .HasOne(gt => gt.Vehicle)
                .WithMany(v => v.GPSTrackings)
                .HasForeignKey(gt => gt.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision for GPS coordinates
            modelBuilder.Entity<GPSTracking>()
                .Property(gt => gt.Latitude)
                .HasPrecision(10, 8);

            modelBuilder.Entity<GPSTracking>()
                .Property(gt => gt.Longitude)
                .HasPrecision(11, 8);

            modelBuilder.Entity<GPSTracking>()
                .Property(gt => gt.Heading)
                .HasPrecision(5, 2);

            modelBuilder.Entity<GPSTracking>()
                .Property(gt => gt.Speed)
                .HasPrecision(5, 2);

            modelBuilder.Entity<GPSTracking>()
                .Property(gt => gt.Altitude)
                .HasPrecision(10, 2);

            modelBuilder.Entity<GeofenceAlert>()
                .HasOne(ga => ga.Geofence)
                .WithMany(g => g.GeofenceAlerts)
                .HasForeignKey(ga => ga.GeofenceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GeofenceAlert>()
                .HasOne(ga => ga.GPSTracking)
                .WithMany(gt => gt.GeofenceAlerts)
                .HasForeignKey(ga => ga.GPSTrackingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision for Geofence coordinates
            modelBuilder.Entity<Geofence>()
                .Property(g => g.CenterLatitude)
                .HasPrecision(10, 8);

            modelBuilder.Entity<Geofence>()
                .Property(g => g.CenterLongitude)
                .HasPrecision(11, 8);

            modelBuilder.Entity<Geofence>()
                .Property(g => g.Radius)
                .HasPrecision(10, 2);

            // Expense relationships and precision
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Budget)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Budget>()
                .Property(b => b.TotalAmount)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Budget>()
                .Property(b => b.SpentAmount)
                .HasPrecision(12, 2);

            modelBuilder.Entity<ExpenseApproval>()
                .HasOne(ea => ea.Expense)
                .WithMany()
                .HasForeignKey(ea => ea.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document relationships
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Vehicle)
                .WithMany()
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Driver)
                .WithMany()
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.ParentDocument)
                .WithMany(d => d.Versions)
                .HasForeignKey(d => d.ParentDocumentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DocumentAccessLog>()
                .HasOne(l => l.Document)
                .WithMany()
                .HasForeignKey(l => l.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Driver Performance relationships
            modelBuilder.Entity<DriverPerformance>()
                .HasOne(p => p.Driver)
                .WithMany()
                .HasForeignKey(p => p.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DriverIncident>()
                .HasOne(i => i.Driver)
                .WithMany()
                .HasForeignKey(i => i.DriverId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vehicle Booking relationships
            modelBuilder.Entity<VehicleBooking>()
                .HasOne(b => b.Vehicle)
                .WithMany()
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleBooking>()
                .HasOne(b => b.Driver)
                .WithMany()
                .HasForeignKey(b => b.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VehicleBooking>()
                .HasOne(b => b.Trip)
                .WithMany()
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VehicleBooking>()
                .Property(b => b.EstimatedCost)
                .HasPrecision(12, 2);

            modelBuilder.Entity<VehicleBooking>()
                .Property(b => b.ActualCost)
                .HasPrecision(12, 2);

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

            // Fuel Records
            modelBuilder.Entity<FuelRecord>().HasData(
                new FuelRecord { Id = 1, VehicleId = 1, FuelDate = new DateTime(2024, 1, 10), LitersFilled = 45.5, PricePerLiter = 107, TotalCost = 4868.50m, OdometerReading = 32000, FilledBy = "Rahim Uddin", StationName = "Padma Filling Station", Notes = "Full tank" },
                new FuelRecord { Id = 2, VehicleId = 2, FuelDate = new DateTime(2024, 1, 15), LitersFilled = 60.0, PricePerLiter = 107, TotalCost = 6420.00m, OdometerReading = 54000, FilledBy = "Karim Hossain", StationName = "Meghna Filling Station", Notes = "" },
                new FuelRecord { Id = 3, VehicleId = 1, FuelDate = new DateTime(2024, 2, 1), LitersFilled = 40.0, PricePerLiter = 109, TotalCost = 4360.00m, OdometerReading = 32264, FilledBy = "Rahim Uddin", StationName = "Jamuna Filling Station", Notes = "After Chittagong trip" }
            );

            // ── Identity Roles ─────────────────────────────────────
            const string ADMIN_ROLE_ID = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
            const string MANAGER_ROLE_ID = "b2c3d4e5-f6a7-8901-bcde-f23456789012";
            const string DRIVER_ROLE_ID = "c3d4e5f6-a7b8-9012-cdef-345678901234";

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = ADMIN_ROLE_ID, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = MANAGER_ROLE_ID, Name = "Manager", NormalizedName = "MANAGER" },
                new IdentityRole { Id = DRIVER_ROLE_ID, Name = "Driver", NormalizedName = "DRIVER" }
            );
        }
    }
}