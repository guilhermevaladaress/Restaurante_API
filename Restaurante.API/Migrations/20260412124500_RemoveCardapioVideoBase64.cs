using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurante.API.Migrations
{
    [DbContext(typeof(Restaurante.API.Data.AppDbContext))]
    [Migration("20260412124500_RemoveCardapioVideoBase64")]
    public partial class RemoveCardapioVideoBase64 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoBase64",
                table: "ItensCardapio");

            migrationBuilder.DropColumn(
                name: "VideoMimeType",
                table: "ItensCardapio");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoBase64",
                table: "ItensCardapio",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoMimeType",
                table: "ItensCardapio",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);
        }
    }
}
