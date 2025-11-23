using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class deletedInstututeUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstitutionUsers");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_CompanyId",
                table: "CompanyUsers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CompanyUsers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CompanyUsers",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CompanyId_UserId",
                table: "CompanyUsers",
                columns: new[] { "CompanyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CreatedDate",
                table: "CompanyUsers",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_IsActive",
                table: "CompanyUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_IsDeleted",
                table: "CompanyUsers",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_CompanyId_UserId",
                table: "CompanyUsers");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_CreatedDate",
                table: "CompanyUsers");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_IsActive",
                table: "CompanyUsers");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_IsDeleted",
                table: "CompanyUsers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "CompanyUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CompanyUsers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "InstitutionUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsManager = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionUsers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CompanyId",
                table: "CompanyUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionUsers_CompanyId_UserId",
                table: "InstitutionUsers",
                columns: new[] { "CompanyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionUsers_CreatedDate",
                table: "InstitutionUsers",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionUsers_IsActive",
                table: "InstitutionUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionUsers_IsDeleted",
                table: "InstitutionUsers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionUsers_UserId",
                table: "InstitutionUsers",
                column: "UserId");
        }
    }
}
