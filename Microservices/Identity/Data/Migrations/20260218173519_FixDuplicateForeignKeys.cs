using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Identity.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDuplicateForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_countries_country_id1",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "fk_users_roles_role_id1",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_country_id1",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_role_id1",
                table: "users");

            migrationBuilder.DropColumn(
                name: "country_id1",
                table: "users");

            migrationBuilder.DropColumn(
                name: "role_id1",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "country_id1",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "role_id1",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_country_id1",
                table: "users",
                column: "country_id1");

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id1",
                table: "users",
                column: "role_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_users_countries_country_id1",
                table: "users",
                column: "country_id1",
                principalTable: "countries",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_roles_role_id1",
                table: "users",
                column: "role_id1",
                principalTable: "roles",
                principalColumn: "id");
        }
    }
}
