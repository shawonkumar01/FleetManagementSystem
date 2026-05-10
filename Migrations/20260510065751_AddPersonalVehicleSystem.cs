using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalVehicleSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalVehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VIN = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    CurrentOdometer = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalVehicles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalVehicleId = table.Column<int>(type: "int", nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OdometerReading = table.Column<int>(type: "int", nullable: true),
                    Liters = table.Column<double>(type: "float", nullable: true),
                    PricePerLiter = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalExpenses_PersonalVehicles_PersonalVehicleId",
                        column: x => x.PersonalVehicleId,
                        principalTable: "PersonalVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalVehicleDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalVehicleId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalVehicleDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalVehicleDocuments_PersonalVehicles_PersonalVehicleId",
                        column: x => x.PersonalVehicleId,
                        principalTable: "PersonalVehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalExpenses_PersonalVehicleId",
                table: "PersonalExpenses",
                column: "PersonalVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalVehicleDocuments_PersonalVehicleId",
                table: "PersonalVehicleDocuments",
                column: "PersonalVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalVehicles_UserId",
                table: "PersonalVehicles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalExpenses");

            migrationBuilder.DropTable(
                name: "PersonalVehicleDocuments");

            migrationBuilder.DropTable(
                name: "PersonalVehicles");
        }
    }
}
