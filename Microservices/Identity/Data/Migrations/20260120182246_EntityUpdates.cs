using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Identity.Data.Migrations
{
    /// <inheritdoc />
    public partial class EntityUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_guid",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "role_permissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_users_user_guid",
                table: "users",
                column: "user_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_user_guid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "user_guid",
                table: "users");

            migrationBuilder.DropColumn(
                name: "id",
                table: "role_permissions");
        }
    }
}
