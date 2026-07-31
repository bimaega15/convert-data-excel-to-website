using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sifp_Vue.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SheetCount = table.Column<int>(type: "int", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    EditCount = table.Column<int>(type: "int", nullable: false),
                    EditsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CanAccessAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Zona = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CcvcLibraryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowNo = table.Column<int>(type: "int", nullable: true),
                    ProtocolGroup = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PsecId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PsecName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExposureType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CcvcId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuestionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QuestionSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VerificationPurpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CcvcLibraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CcvcLibraryItems_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClsrHealthMapRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClsrId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClsrDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Zona11Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Zona11Score = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Zona12Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Zona12Score = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Zona13Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Zona13Score = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Zona14Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Zona14Score = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Regional4Score = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    HealthStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClsrHealthMapRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClsrHealthMapRows_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DashboardTexts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Section = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardTexts_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExecutiveMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Numerator = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Denominator = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    ScorePercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    TargetPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutiveMeasures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecutiveMeasures_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImprovementInitiatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImprovementCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Initiative = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RelatedClsr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    ExpectedImpact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImprovementInitiatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImprovementInitiatives_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Observations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObsCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProtocolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProtocolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ObservationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Zona = table.Column<int>(type: "int", nullable: true),
                    Site = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AreaEquipment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activity = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Observer1 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Observer2 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Observer3 = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    YesCount = table.Column<int>(type: "int", nullable: false),
                    NoCount = table.Column<int>(type: "int", nullable: false),
                    NaCount = table.Column<int>(type: "int", nullable: false),
                    PerformancePercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    ObservationSequence = table.Column<int>(type: "int", nullable: true),
                    PsieEligible = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observations_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuickFacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FactValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickFacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuickFacts_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TopFiveItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Item = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Denominator = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopFiveItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopFiveItems_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrendPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    MonthLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ActualPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    PlannedPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    ObservationCount = table.Column<int>(type: "int", nullable: false),
                    IsProjection = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrendPoints_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Worksheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SheetIndex = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsCurated = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ColCount = table.Column<int>(type: "int", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worksheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Worksheets_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZonaScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Zona = table.Column<int>(type: "int", nullable: false),
                    ZonaLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScorePercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    ObservationCount = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonaScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZonaScores_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriftConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObservationId = table.Column<int>(type: "int", nullable: false),
                    ProtocolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProtocolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Situation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriftConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriftConditions_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriftConditions_Observations_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "Observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ErrorTraps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObservationId = table.Column<int>(type: "int", nullable: false),
                    ProtocolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProtocolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrapName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorTraps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorTraps_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ErrorTraps_Observations_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "Observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HpTools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObservationId = table.Column<int>(type: "int", nullable: false),
                    ProtocolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProtocolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tujuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KapanDigunakan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaraPakai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectivenessNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HpTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HpTools_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HpTools_Observations_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "Observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LatentConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObservationId = table.Column<int>(type: "int", nullable: false),
                    ProtocolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProtocolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ObservationText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Level2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatentConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LatentConditions_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LatentConditions_Observations_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "Observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SifQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObservationId = table.Column<int>(type: "int", nullable: false),
                    ProtocolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProtocolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QuestionRef = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CcvcId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Answer = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SifExposure = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CriticalSafeguard = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ObservationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Zona = table.Column<int>(type: "int", nullable: true),
                    Site = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activity = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SifQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SifQuestions_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SifQuestions_Observations_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "Observations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorksheetRows",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorksheetId = table.Column<int>(type: "int", nullable: false),
                    ExcelRow = table.Column<int>(type: "int", nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    CellsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorksheetRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorksheetRows_Worksheets_WorksheetId",
                        column: x => x.WorksheetId,
                        principalTable: "Worksheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CcvcLibraryItems_CcvcId",
                table: "CcvcLibraryItems",
                column: "CcvcId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CcvcLibraryItems_ImportBatchId",
                table: "CcvcLibraryItems",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CcvcLibraryItems_PsecId",
                table: "CcvcLibraryItems",
                column: "PsecId");

            migrationBuilder.CreateIndex(
                name: "IX_ClsrHealthMapRows_ClsrId",
                table: "ClsrHealthMapRows",
                column: "ClsrId");

            migrationBuilder.CreateIndex(
                name: "IX_ClsrHealthMapRows_ImportBatchId",
                table: "ClsrHealthMapRows",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardTexts_ImportBatchId",
                table: "DashboardTexts",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardTexts_Section",
                table: "DashboardTexts",
                column: "Section",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriftConditions_Code",
                table: "DriftConditions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DriftConditions_ImportBatchId",
                table: "DriftConditions",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DriftConditions_ObservationId",
                table: "DriftConditions",
                column: "ObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorTraps_Category",
                table: "ErrorTraps",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorTraps_ImportBatchId",
                table: "ErrorTraps",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorTraps_ObservationId",
                table: "ErrorTraps",
                column: "ObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutiveMeasures_ImportBatchId",
                table: "ExecutiveMeasures",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutiveMeasures_MetricCode",
                table: "ExecutiveMeasures",
                column: "MetricCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HpTools_ImportBatchId",
                table: "HpTools",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_HpTools_ObservationId",
                table: "HpTools",
                column: "ObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_HpTools_ToolName",
                table: "HpTools",
                column: "ToolName");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_CreatedAt",
                table: "ImportBatches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_FileHash",
                table: "ImportBatches",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_ImprovementInitiatives_ImportBatchId",
                table: "ImprovementInitiatives",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ImprovementInitiatives_ImprovementCode",
                table: "ImprovementInitiatives",
                column: "ImprovementCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImprovementInitiatives_Status",
                table: "ImprovementInitiatives",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LatentConditions_Code",
                table: "LatentConditions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_LatentConditions_ImportBatchId",
                table: "LatentConditions",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_LatentConditions_ObservationId",
                table: "LatentConditions",
                column: "ObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_ImportBatchId",
                table: "Observations",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_ObsCode",
                table: "Observations",
                column: "ObsCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Observations_ObservationDate",
                table: "Observations",
                column: "ObservationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_ProtocolCode",
                table: "Observations",
                column: "ProtocolCode");

            migrationBuilder.CreateIndex(
                name: "IX_Observations_Zona",
                table: "Observations",
                column: "Zona");

            migrationBuilder.CreateIndex(
                name: "IX_QuickFacts_DisplayOrder",
                table: "QuickFacts",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_QuickFacts_ImportBatchId",
                table: "QuickFacts",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SifQuestions_Answer",
                table: "SifQuestions",
                column: "Answer");

            migrationBuilder.CreateIndex(
                name: "IX_SifQuestions_CcvcId",
                table: "SifQuestions",
                column: "CcvcId");

            migrationBuilder.CreateIndex(
                name: "IX_SifQuestions_ImportBatchId",
                table: "SifQuestions",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SifQuestions_ObservationId",
                table: "SifQuestions",
                column: "ObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TopFiveItems_Category_DisplayOrder",
                table: "TopFiveItems",
                columns: new[] { "Category", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TopFiveItems_ImportBatchId",
                table: "TopFiveItems",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrendPoints_ImportBatchId",
                table: "TrendPoints",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrendPoints_PeriodMonth",
                table: "TrendPoints",
                column: "PeriodMonth",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorksheetRows_WorksheetId_RowIndex",
                table: "WorksheetRows",
                columns: new[] { "WorksheetId", "RowIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Worksheets_ImportBatchId_Slug",
                table: "Worksheets",
                columns: new[] { "ImportBatchId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZonaScores_ImportBatchId",
                table: "ZonaScores",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ZonaScores_Zona",
                table: "ZonaScores",
                column: "Zona",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CcvcLibraryItems");

            migrationBuilder.DropTable(
                name: "ClsrHealthMapRows");

            migrationBuilder.DropTable(
                name: "DashboardTexts");

            migrationBuilder.DropTable(
                name: "DriftConditions");

            migrationBuilder.DropTable(
                name: "ErrorTraps");

            migrationBuilder.DropTable(
                name: "ExecutiveMeasures");

            migrationBuilder.DropTable(
                name: "HpTools");

            migrationBuilder.DropTable(
                name: "ImprovementInitiatives");

            migrationBuilder.DropTable(
                name: "LatentConditions");

            migrationBuilder.DropTable(
                name: "QuickFacts");

            migrationBuilder.DropTable(
                name: "SifQuestions");

            migrationBuilder.DropTable(
                name: "TopFiveItems");

            migrationBuilder.DropTable(
                name: "TrendPoints");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "WorksheetRows");

            migrationBuilder.DropTable(
                name: "ZonaScores");

            migrationBuilder.DropTable(
                name: "Observations");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Worksheets");

            migrationBuilder.DropTable(
                name: "ImportBatches");
        }
    }
}
