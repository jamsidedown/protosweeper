using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Protosweeper.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Seed = table.Column<Guid>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<byte>(type: "INTEGER", nullable: false),
                    InitialX = table.Column<int>(type: "INTEGER", nullable: false),
                    InitialY = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
