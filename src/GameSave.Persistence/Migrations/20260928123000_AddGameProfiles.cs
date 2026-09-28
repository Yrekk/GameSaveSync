using GameSave.Persistence.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSave.Persistence.Migrations;

[DbContext(typeof(GameSaveDbContext))]
[Migration("20260928123000_AddGameProfiles")]
public partial class AddGameProfiles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GameProfiles",
            columns: table => new
            {
                ProfileId = table.Column<string>(
                    type: "TEXT",
                    maxLength: 128,
                    nullable: false),
                DisplayName = table.Column<string>(
                    type: "TEXT",
                    maxLength: 256,
                    nullable: false),
                PayloadJson = table.Column<string>(
                    type: "TEXT",
                    nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameProfiles", x => x.ProfileId);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "GameProfiles");
    }
}
