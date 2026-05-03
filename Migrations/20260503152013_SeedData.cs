using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FleetManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "Id", "FirstName", "LastName", "LicenseExpiry", "LicenseNumber", "Phone", "Status" },
                values: new object[,]
                {
                    { 1, "Rahim", "Uddin", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DL-001-2021", "01711-000001", "Active" },
                    { 2, "Karim", "Hossain", new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "DL-002-2020", "01711-000002", "Active" },
                    { 3, "Jamal", "Sheikh", new DateTime(2024, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "DL-003-2019", "01711-000003", "Inactive" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "LicensePlate", "Make", "Mileage", "Model", "Status", "Year" },
                values: new object[,]
                {
                    { 1, "DHK-1234", "Toyota", 32000, "Hilux", "Active", 2021 },
                    { 2, "DHK-5678", "Ford", 54000, "Transit", "Active", 2020 },
                    { 3, "DHK-9999", "Mitsubishi", 87000, "Canter", "InMaintenance", 2019 }
                });

            migrationBuilder.InsertData(
                table: "MaintenanceRecords",
                columns: new[] { "Id", "Cost", "NextServiceDate", "Notes", "ServiceDate", "ServiceType", "Status", "VehicleId" },
                values: new object[,]
                {
                    { 1, 15000m, new DateTime(2024, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Full engine check and oil change", new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engine Overhaul", "Completed", 3 },
                    { 2, 8000m, new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "All 4 tires replaced", new DateTime(2024, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tire Replacement", "Completed", 1 },
                    { 3, 0m, null, "Brake pads inspection due", new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Brake Service", "Scheduled", 2 }
                });

            migrationBuilder.InsertData(
                table: "Trips",
                columns: new[] { "Id", "Destination", "DistanceKm", "DriverId", "EndTime", "Origin", "StartTime", "Status", "VehicleId" },
                values: new object[,]
                {
                    { 1, "Chittagong", 264.0, 1, new DateTime(2024, 1, 10, 14, 0, 0, 0, DateTimeKind.Unspecified), "Dhaka", new DateTime(2024, 1, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 1 },
                    { 2, "Sylhet", 240.0, 2, new DateTime(2024, 1, 15, 15, 30, 0, 0, DateTimeKind.Unspecified), "Dhaka", new DateTime(2024, 1, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 2 },
                    { 3, "Khulna", 0.0, 1, null, "Dhaka", new DateTime(2024, 2, 1, 7, 0, 0, 0, DateTimeKind.Unspecified), "Planned", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MaintenanceRecords",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaintenanceRecords",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaintenanceRecords",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Trips",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
