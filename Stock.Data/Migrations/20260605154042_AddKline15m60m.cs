using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKline15m60m : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kline_15m",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_15m", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kline_1d",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_1d", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kline_1m",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_1m", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kline_1mo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_1mo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kline_1w",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_1w", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kline_30m",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_30m", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kline_5m",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_5m", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kline_60m",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TradeTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kline_60m", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockInfo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kline15m_Symbol_Time",
                table: "Kline_15m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline1d_Symbol_Time",
                table: "Kline_1d",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline1m_Symbol_Time",
                table: "Kline_1m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline1mo_Symbol_Time",
                table: "Kline_1mo",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline1w_Symbol_Time",
                table: "Kline_1w",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline30m_Symbol_Time",
                table: "Kline_30m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline5m_Symbol_Time",
                table: "Kline_5m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline60m_Symbol_Time",
                table: "Kline_60m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockInfo_Symbol",
                table: "StockInfo",
                column: "Symbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kline_15m");

            migrationBuilder.DropTable(
                name: "Kline_1d");

            migrationBuilder.DropTable(
                name: "Kline_1m");

            migrationBuilder.DropTable(
                name: "Kline_1mo");

            migrationBuilder.DropTable(
                name: "Kline_1w");

            migrationBuilder.DropTable(
                name: "Kline_30m");

            migrationBuilder.DropTable(
                name: "Kline_5m");

            migrationBuilder.DropTable(
                name: "Kline_60m");

            migrationBuilder.DropTable(
                name: "StockInfo");
        }
    }
}
