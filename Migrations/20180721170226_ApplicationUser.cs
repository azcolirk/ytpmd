using Microsoft.EntityFrameworkCore.Migrations;

namespace ytpmd.Migrations
{
    public partial class ApplicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "YTLogin",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YTPassword",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YTLogin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "YTPassword",
                table: "AspNetUsers");
        }
    }
}
