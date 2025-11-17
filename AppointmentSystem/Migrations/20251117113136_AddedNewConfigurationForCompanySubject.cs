using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewConfigurationForCompanySubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanySubjects_CompanyId",
                table: "CompanySubjects");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CompanySubjects",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CompanySubjects",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubjects_CompanyId_SubjectId",
                table: "CompanySubjects",
                columns: new[] { "CompanyId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubjects_CreatedDate",
                table: "CompanySubjects",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubjects_IsActive",
                table: "CompanySubjects",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubjects_IsDeleted",
                table: "CompanySubjects",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanySubjects_CompanyId_SubjectId",
                table: "CompanySubjects");

            migrationBuilder.DropIndex(
                name: "IX_CompanySubjects_CreatedDate",
                table: "CompanySubjects");

            migrationBuilder.DropIndex(
                name: "IX_CompanySubjects_IsActive",
                table: "CompanySubjects");

            migrationBuilder.DropIndex(
                name: "IX_CompanySubjects_IsDeleted",
                table: "CompanySubjects");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CompanySubjects",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CompanySubjects",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySubjects_CompanyId",
                table: "CompanySubjects",
                column: "CompanyId");
        }
    }
}
