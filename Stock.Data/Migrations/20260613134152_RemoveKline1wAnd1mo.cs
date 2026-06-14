using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKline1wAnd1mo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kline_1mo");

            migrationBuilder.DropTable(
                name: "Kline_1w");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kline_1mo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_1mo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kline_1mo_StockInfo_Symbol",
                        column: x => x.Symbol,
                        principalTable: "StockInfo",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kline_1w",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_1w", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kline_1w_StockInfo_Symbol",
                        column: x => x.Symbol,
                        principalTable: "StockInfo",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kline_1mo_Symbol",
                table: "Kline_1mo",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline1mo_Symbol_Time",
                table: "Kline_1mo",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline_1w_Symbol",
                table: "Kline_1w",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline1w_Symbol_Time",
                table: "Kline_1w",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);
        }
    }
}
