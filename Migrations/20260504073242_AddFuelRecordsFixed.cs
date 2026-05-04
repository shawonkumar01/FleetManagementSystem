using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FleetManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelRecordsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuelRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    FuelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LitersFilled = table.Column<double>(type: "float", nullable: false),
                    PricePerLiter = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    OdometerReading = table.Column<int>(type: "int", nullable: false),
                    FilledBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelRecords_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FuelRecords",
                columns: new[] { "Id", "FilledBy", "FuelDate", "LitersFilled", "Notes", "OdometerReading", "PricePerLiter", "StationName", "TotalCost", "VehicleId" },
                values: new object[,]
                {
                    { 1, "Rahim Uddin", new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 45.5, "Full tank", 32000, 107m, "Padma Filling Station", 4868.50m, 1 },
                    { 2, "Karim Hossain", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 60.0, "", 54000, 107m, "Meghna Filling Station", 6420.00m, 2 },
                    { 3, "Rahim Uddin", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 40.0, "After Chittagong trip", 32264, 109m, "Jamuna Filling Station", 4360.00m, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelRecords_VehicleId",
                table: "FuelRecords",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelRecords");
        }
    }
}
