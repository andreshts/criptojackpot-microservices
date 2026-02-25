using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoJackpot.Wallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletTransactionsAndBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wallet_balances",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    total_earned = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    total_withdrawn = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    total_purchased = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    row_version = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_balances", x => x.id);
                    table.UniqueConstraint("ak_wallet_balances_user_guid", x => x.user_guid);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transaction_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    direction = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    balance_after = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_transactions_wallet_balances_user_guid",
                        column: x => x.user_guid,
                        principalTable: "wallet_balances",
                        principalColumn: "user_guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wallet_balances_user_guid",
                table: "wallet_balances",
                column: "user_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_reference_id",
                table: "wallet_transactions",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_status",
                table: "wallet_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_transaction_guid",
                table: "wallet_transactions",
                column: "transaction_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_user_guid",
                table: "wallet_transactions",
                column: "user_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "wallet_balances");
        }
    }
}
