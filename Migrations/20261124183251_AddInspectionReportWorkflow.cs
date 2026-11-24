using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoVerifyMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionReportWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyCondition",
                table: "CarInquiries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrakesCondition",
                table: "CarInquiries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "CarInquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineCondition",
                table: "CarInquiries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectionNotes",
                table: "CarInquiries",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectorId",
                table: "CarInquiries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransmissionCondition",
                table: "CarInquiries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarInquiries_InspectorId_Status",
                table: "CarInquiries",
                columns: new[] { "InspectorId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_CarInquiries_AspNetUsers_InspectorId",
                table: "CarInquiries",
                column: "InspectorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarInquiries_AspNetUsers_InspectorId",
                table: "CarInquiries");

            migrationBuilder.DropIndex(
                name: "IX_CarInquiries_InspectorId_Status",
                table: "CarInquiries");

            migrationBuilder.DropColumn(
                name: "BodyCondition",
                table: "CarInquiries");

            migrationBuilder.DropColumn(
                name: "BrakesCondition",
                table: "CarInquiries");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "CarInquiries");

            migrationBuilder.DropColumn(
                name: "EngineCondition",
                table: "CarInquiries");

            migrationBuilder.DropColumn(
                name: "InspectionNotes",
                table: "CarInquiries");

            migrationBuilder.DropColumn(
                name: "InspectorId",
                table: "CarInquiries");

            migrationBuilder.DropColumn(
                name: "TransmissionCondition",
                table: "CarInquiries");
        }
    }
}
