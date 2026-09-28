using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSave.Persistence.Migrations;

/// <summary>
/// Establishes EF migration history before GameSaveSync has business tables.
/// </summary>
public partial class InitialMetadataDatabase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
