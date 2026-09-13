using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MUnique.OpenMU.Persistence.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class CombatMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CombatMetric",
                schema: "data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntervalStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GameServerId = table.Column<byte>(type: "smallint", nullable: false),
                    CombatType = table.Column<byte>(type: "smallint", nullable: false),
                    MapNumber = table.Column<short>(type: "smallint", nullable: false),
                    SkillNumber = table.Column<short>(type: "smallint", nullable: false),
                    HitCount = table.Column<long>(type: "bigint", nullable: false),
                    HealthDamage = table.Column<long>(type: "bigint", nullable: false),
                    ShieldDamage = table.Column<long>(type: "bigint", nullable: false),
                    CriticalHitCount = table.Column<long>(type: "bigint", nullable: false),
                    ExcellentHitCount = table.Column<long>(type: "bigint", nullable: false),
                    DamageBelow200HitCount = table.Column<long>(type: "bigint", nullable: false),
                    Damage200To399HitCount = table.Column<long>(type: "bigint", nullable: false),
                    Damage400To599HitCount = table.Column<long>(type: "bigint", nullable: false),
                    Damage600To799HitCount = table.Column<long>(type: "bigint", nullable: false),
                    Damage800OrMoreHitCount = table.Column<long>(type: "bigint", nullable: false),
                    AttackerCharacterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatMetric", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CombatMetric",
                schema: "data");
        }
    }
}
