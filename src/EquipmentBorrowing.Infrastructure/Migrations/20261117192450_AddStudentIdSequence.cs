using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentIdSequence : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "StudentIdSequence");

            migrationBuilder.Sql(
                """
        SELECT setval(
            'public."StudentIdSequence"'::regclass,
            GREATEST(
                COALESCE(
                    (SELECT MAX("StudentId") FROM "Students"),
                    0
                ),
                1
            ),
            EXISTS(SELECT 1 FROM "Students")
        );
        """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "StudentIdSequence");
        }

    }
}
