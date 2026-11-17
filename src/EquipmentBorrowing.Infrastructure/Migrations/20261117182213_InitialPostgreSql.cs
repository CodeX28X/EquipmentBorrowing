using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentIdentifier = table.Column<string>(type: "text", nullable: false),
                    EquipmentName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    NormalizedEquipmentIdentifier = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(btrim(\"EquipmentIdentifier\"))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.EquipmentId);
                    table.CheckConstraint("CK_Equipment_Description_NotBlank", "\"Description\" IS NULL OR length(btrim(\"Description\")) > 0");
                    table.CheckConstraint("CK_Equipment_Identifier_NotBlank", "length(btrim(\"EquipmentIdentifier\")) > 0");
                    table.CheckConstraint("CK_Equipment_Name_NotBlank", "length(btrim(\"EquipmentName\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    StudentNumber = table.Column<string>(type: "text", nullable: false),
                    StudentName = table.Column<string>(type: "text", nullable: false),
                    StudentYear = table.Column<int>(type: "integer", nullable: true),
                    ContactInformation = table.Column<string>(type: "text", nullable: true),
                    IsAllowedToBorrow = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    NormalizedStudentNumber = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(regexp_replace(\"StudentNumber\", '^[[:space:]]+|[[:space:]]+$', '', 'g'))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                    table.CheckConstraint("CK_Students_Contact_NotBlank", "\"ContactInformation\" IS NULL OR length(btrim(\"ContactInformation\")) > 0");
                    table.CheckConstraint("CK_Students_StudentName_NotBlank", "length(btrim(\"StudentName\")) > 0");
                    table.CheckConstraint("CK_Students_StudentNumber_NotBlank", "length(btrim(\"StudentNumber\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Borrowings",
                columns: table => new
                {
                    BorrowingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    BorrowedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActualReturnDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrowings", x => x.BorrowingId);
                    table.CheckConstraint("CK_Borrowings_ActualReturnDate", "\"ActualReturnDate\" IS NULL OR \"ActualReturnDate\" >= \"BorrowedAt\"");
                    table.CheckConstraint("CK_Borrowings_ExpectedReturnDate", "\"ExpectedReturnDate\" > \"BorrowedAt\"");
                    table.CheckConstraint("CK_Borrowings_Status", "\"Status\" IN (0, 1)");
                    table.CheckConstraint("CK_Borrowings_Status_ReturnDate", "(\"Status\" = 0 AND \"ActualReturnDate\" IS NULL) OR (\"Status\" = 1 AND \"ActualReturnDate\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Borrowings_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Borrowings_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_BorrowedAt",
                table: "Borrowings",
                column: "BorrowedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_EquipmentId_BorrowedAt",
                table: "Borrowings",
                columns: new[] { "EquipmentId", "BorrowedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_Status_ExpectedReturnDate",
                table: "Borrowings",
                columns: new[] { "Status", "ExpectedReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_StudentId_BorrowedAt",
                table: "Borrowings",
                columns: new[] { "StudentId", "BorrowedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_Borrowings_ActiveEquipment",
                table: "Borrowings",
                column: "EquipmentId",
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentName",
                table: "Equipment",
                column: "EquipmentName");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_IsAvailable",
                table: "Equipment",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "UX_Equipment_NormalizedIdentifier",
                table: "Equipment",
                column: "NormalizedEquipmentIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentName",
                table: "Students",
                column: "StudentName");

            migrationBuilder.CreateIndex(
                name: "UX_Students_NormalizedStudentNumber",
                table: "Students",
                column: "NormalizedStudentNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Borrowings");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
