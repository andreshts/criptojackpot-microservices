using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Lottery.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPrizeImagesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lottery_draws",
                columns: table => new
                {
                    lottery_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    lottery_no = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    min_number = table.Column<int>(type: "integer", nullable: false),
                    max_number = table.Column<int>(type: "integer", nullable: false),
                    total_series = table.Column<int>(type: "integer", nullable: false),
                    ticket_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    max_tickets = table.Column<int>(type: "integer", nullable: false),
                    sold_tickets = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    terms = table.Column<string>(type: "text", nullable: false),
                    has_age_restriction = table.Column<bool>(type: "boolean", nullable: false),
                    minimum_age = table.Column<int>(type: "integer", nullable: true),
                    restricted_countries = table.Column<List<string>>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lottery_draws", x => x.lottery_guid);
                });

            migrationBuilder.CreateTable(
                name: "lottery_numbers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lottery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    series = table.Column<int>(type: "integer", nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lottery_numbers", x => x.id);
                    table.ForeignKey(
                        name: "fk_lottery_numbers_lottery_draws_lottery_id",
                        column: x => x.lottery_id,
                        principalTable: "lottery_draws",
                        principalColumn: "lottery_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prizes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lottery_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tier = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    estimated_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    main_image_url = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    specifications = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    cash_alternative = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    is_deliverable = table.Column<bool>(type: "boolean", nullable: false),
                    is_digital = table.Column<bool>(type: "boolean", nullable: false),
                    winner_ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    claimed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prizes", x => x.id);
                    table.ForeignKey(
                        name: "fk_prizes_lottery_draws_lottery_id",
                        column: x => x.lottery_id,
                        principalTable: "lottery_draws",
                        principalColumn: "lottery_guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prize_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    caption = table.Column<string>(type: "text", maxLength: 200, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prize_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_prize_images_prizes_prize_id",
                        column: x => x.prize_id,
                        principalTable: "prizes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lottery_draws_lottery_no",
                table: "lottery_draws",
                column: "lottery_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotteryNumbers_LotteryId_Number_Series",
                table: "lottery_numbers",
                columns: new[] { "lottery_id", "number", "series" });

            migrationBuilder.CreateIndex(
                name: "ix_prize_images_prize_id",
                table: "prize_images",
                column: "prize_id");

            migrationBuilder.CreateIndex(
                name: "ix_prizes_lottery_id",
                table: "prizes",
                column: "lottery_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lottery_numbers");

            migrationBuilder.DropTable(
                name: "prize_images");

            migrationBuilder.DropTable(
                name: "prizes");

            migrationBuilder.DropTable(
                name: "lottery_draws");
        }
    }
}
