
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    public partial class AddEquipmentIdSequence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "EquipmentIdSequence");

            migrationBuilder.Sql(
                """
                SELECT setval(
                    'public."EquipmentIdSequence"'::regclass,
                    GREATEST(
                        COALESCE(
                            (SELECT MAX("EquipmentId") FROM "Equipment"),
                            0
                        ),
                        1
                    ),
                    EXISTS(
                        SELECT 1 FROM "Equipment"
                    )
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "EquipmentIdSequence");
        }
    }
}
