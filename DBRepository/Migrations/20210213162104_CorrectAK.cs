using Microsoft.EntityFrameworkCore.Migrations;

namespace Context.Migrations
{
    public partial class CorrectAK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Password",
                table: "Users");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Login",
                table: "Users",
                column: "Login");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Login",
                table: "Users");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Password",
                table: "Users",
                column: "Password");
        }
    }
}
