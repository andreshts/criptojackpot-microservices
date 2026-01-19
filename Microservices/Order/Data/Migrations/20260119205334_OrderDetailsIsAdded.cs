using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoJackpot.Order.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderDetailsIsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_tickets_ticket_id",
                table: "orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tickets",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_order_id",
                table: "tickets");

            migrationBuilder.DropPrimaryKey(
                name: "pk_orders",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_ticket_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "lottery_number_ids",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "order_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "selected_numbers",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "gift_recipient_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "is_gift",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "lottery_number_ids",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "selected_numbers",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "series",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ticket_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "total_amount",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "gift_recipient_id",
                table: "tickets",
                newName: "gift_sender_id");

            migrationBuilder.AlterColumn<string>(
                name: "transaction_id",
                table: "tickets",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "is_gift",
                table: "tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "tickets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "lottery_number_id",
                table: "tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "number",
                table: "tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "order_detail_id",
                table: "tickets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "pk_tickets",
                table: "tickets",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_orders",
                table: "orders",
                column: "id");

            migrationBuilder.CreateTable(
                name: "order_details",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    number = table.Column<int>(type: "integer", nullable: false),
                    series = table.Column<int>(type: "integer", nullable: false),
                    lottery_number_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_gift = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    gift_recipient_id = table.Column<long>(type: "bigint", nullable: true),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_details_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_order_detail_id",
                table: "tickets",
                column: "order_detail_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tickets_ticket_guid",
                table: "tickets",
                column: "ticket_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_guid",
                table: "orders",
                column: "order_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_details_number_series",
                table: "order_details",
                columns: new[] { "number", "series" });

            migrationBuilder.CreateIndex(
                name: "ix_order_details_order_id",
                table: "order_details",
                column: "order_id");

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_order_details_order_detail_id",
                table: "tickets",
                column: "order_detail_id",
                principalTable: "order_details",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tickets_order_details_order_detail_id",
                table: "tickets");

            migrationBuilder.DropTable(
                name: "order_details");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tickets",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_order_detail_id",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_ticket_guid",
                table: "tickets");

            migrationBuilder.DropPrimaryKey(
                name: "pk_orders",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_order_guid",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "lottery_number_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "number",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "order_detail_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "id",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "gift_sender_id",
                table: "tickets",
                newName: "gift_recipient_id");

            migrationBuilder.AlterColumn<string>(
                name: "transaction_id",
                table: "tickets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<bool>(
                name: "is_gift",
                table: "tickets",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "lottery_number_ids",
                table: "tickets",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "order_id",
                table: "tickets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int[]>(
                name: "selected_numbers",
                table: "tickets",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<long>(
                name: "gift_recipient_id",
                table: "orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_gift",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "lottery_number_ids",
                table: "orders",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.AddColumn<int[]>(
                name: "selected_numbers",
                table: "orders",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int>(
                name: "series",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ticket_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                table: "orders",
                type: "numeric(18,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "pk_tickets",
                table: "tickets",
                column: "ticket_guid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_orders",
                table: "orders",
                column: "order_guid");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_order_id",
                table: "tickets",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_ticket_id",
                table: "orders",
                column: "ticket_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_tickets_ticket_id",
                table: "orders",
                column: "ticket_id",
                principalTable: "tickets",
                principalColumn: "ticket_guid");
        }
    }
}
