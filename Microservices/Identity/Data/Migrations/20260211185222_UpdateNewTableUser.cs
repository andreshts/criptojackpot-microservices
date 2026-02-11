using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Identity.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewTableUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_security_code",
                table: "users");

            migrationBuilder.DropColumn(
                name: "google_access_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "google_refresh_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_code_expiration",
                table: "users");

            migrationBuilder.DropColumn(
                name: "security_code",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "keycloak_id",
                table: "users",
                type: "text",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_keycloak_id",
                table: "users",
                column: "keycloak_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_keycloak_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "keycloak_id",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "google_access_token",
                table: "users",
                type: "text",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "google_refresh_token",
                table: "users",
                type: "text",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "users",
                type: "text",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "password_reset_code_expiration",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_code",
                table: "users",
                type: "text",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_security_code",
                table: "users",
                column: "security_code",
                unique: true);
        }
    }
}
