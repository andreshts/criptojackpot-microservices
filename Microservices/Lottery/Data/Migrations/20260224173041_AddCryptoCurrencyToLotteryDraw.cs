using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoJackpot.Lottery.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCryptoCurrencyToLotteryDraw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "crypto_currency_id",
                table: "lottery_draws",
                type: "text",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "crypto_currency_symbol",
                table: "lottery_draws",
                type: "text",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crypto_currency_id",
                table: "lottery_draws");

            migrationBuilder.DropColumn(
                name: "crypto_currency_symbol",
                table: "lottery_draws");
        }
    }
}
