using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyIdFromSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Companies_CompanyId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_CompanyId_Code",
                table: "Subjects");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                table: "Subjects",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code",
                table: "Subjects",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CompanyId",
                table: "Subjects",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Companies_CompanyId",
                table: "Subjects",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Companies_CompanyId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_Code",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_CompanyId",
                table: "Subjects");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                table: "Subjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CompanyId_Code",
                table: "Subjects",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Companies_CompanyId",
                table: "Subjects",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
