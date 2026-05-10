using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverPerformanceScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    IncidentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TripId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImpactOnScore = table.Column<int>(type: "int", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverIncidents_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverPerformances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    EvaluationPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluationPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SafetyScore = table.Column<int>(type: "int", nullable: false),
                    PunctualityScore = table.Column<int>(type: "int", nullable: false),
                    FuelEfficiencyScore = table.Column<int>(type: "int", nullable: false),
                    VehicleConditionScore = table.Column<int>(type: "int", nullable: false),
                    CustomerServiceScore = table.Column<int>(type: "int", nullable: false),
                    ComplianceScore = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    TotalTrips = table.Column<int>(type: "int", nullable: false),
                    TotalDistanceKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDrivingHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccidentsCount = table.Column<int>(type: "int", nullable: false),
                    TrafficViolationsCount = table.Column<int>(type: "int", nullable: false),
                    ComplaintsCount = table.Column<int>(type: "int", nullable: false),
                    ComplimentsCount = table.Column<int>(type: "int", nullable: false),
                    AverageFuelConsumption = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalFuelCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateArrivalsCount = table.Column<int>(type: "int", nullable: false),
                    EarlyDeparturesCount = table.Column<int>(type: "int", nullable: false),
                    MaintenanceIssuesReported = table.Column<int>(type: "int", nullable: false),
                    EvaluatorNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DriverComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EvaluatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ImprovementGoals = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrainingRecommendations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsTopPerformer = table.Column<bool>(type: "bit", nullable: false),
                    NeedsImprovement = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverPerformances_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceThresholds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetricType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinAcceptableScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TargetScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExcellentScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceThresholds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverIncidents_DriverId",
                table: "DriverIncidents",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverPerformances_DriverId",
                table: "DriverPerformances",
                column: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverIncidents");

            migrationBuilder.DropTable(
                name: "DriverPerformances");

            migrationBuilder.DropTable(
                name: "PerformanceThresholds");
        }
    }
}
