
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    public partial class AlignEquipmentIdentifierNormalization
        : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Equipment"
                ALTER COLUMN "NormalizedEquipmentIdentifier"
                SET EXPRESSION AS (
                    lower(regexp_replace(
                        "EquipmentIdentifier",
                        '^[[:space:]]+|[[:space:]]+$',
                        '',
                        'g'
                    ))
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Equipment"
                ALTER COLUMN "NormalizedEquipmentIdentifier"
                SET EXPRESSION AS (
                    lower(btrim("EquipmentIdentifier"))
                );
                """);
        }
    }
}
