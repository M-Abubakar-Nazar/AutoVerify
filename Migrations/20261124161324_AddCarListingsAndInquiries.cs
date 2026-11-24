using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoVerifyMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCarListingsAndInquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarListings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeedKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Make = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Trim = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelYear = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MileageKm = table.Column<int>(type: "int", nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Transmission = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FuelType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsSeedData = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SellerId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarListings_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarInquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarListingId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarInquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarInquiries_AspNetUsers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarInquiries_CarListings_CarListingId",
                        column: x => x.CarListingId,
                        principalTable: "CarListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarInquiries_BuyerId_CreatedAtUtc",
                table: "CarInquiries",
                columns: new[] { "BuyerId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CarInquiries_CarListingId_Status",
                table: "CarInquiries",
                columns: new[] { "CarListingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_SeedKey",
                table: "CarListings",
                column: "SeedKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_SellerId",
                table: "CarListings",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_Status_ModelYear",
                table: "CarListings",
                columns: new[] { "Status", "ModelYear" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarInquiries");

            migrationBuilder.DropTable(
                name: "CarListings");
        }
    }
}
