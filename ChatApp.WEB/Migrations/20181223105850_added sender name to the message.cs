using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatApp.WEB.Migrations
{
    public partial class addedsendernametothemessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "Messages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Messages");
        }
    }
}
