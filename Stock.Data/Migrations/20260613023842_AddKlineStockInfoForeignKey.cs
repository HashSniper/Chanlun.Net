using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKlineStockInfoForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockInfo_Symbol",
                table: "StockInfo");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_StockInfo_Symbol",
                table: "StockInfo",
                column: "Symbol");

            // 清理 Kline 表中的脏数据：删除 StockInfo 中不存在的 Symbol 记录
            migrationBuilder.Sql(@"
                DELETE FROM [Kline_1m] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
                DELETE FROM [Kline_5m] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
                DELETE FROM [Kline_15m] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
                DELETE FROM [Kline_30m] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
                DELETE FROM [Kline_60m] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
                DELETE FROM [Kline_1d] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
                DELETE FROM [Kline_1w] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
                DELETE FROM [Kline_1mo] WHERE [Symbol] NOT IN (SELECT [Symbol] FROM [StockInfo]);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_60m_Symbol",
                table: "Kline_60m",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_5m_Symbol",
                table: "Kline_5m",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_30m_Symbol",
                table: "Kline_30m",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_1w_Symbol",
                table: "Kline_1w",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_1mo_Symbol",
                table: "Kline_1mo",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_1m_Symbol",
                table: "Kline_1m",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_1d_Symbol",
                table: "Kline_1d",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_Kline_15m_Symbol",
                table: "Kline_15m",
                column: "Symbol");

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_15m_StockInfo_Symbol",
                table: "Kline_15m",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_1d_StockInfo_Symbol",
                table: "Kline_1d",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_1m_StockInfo_Symbol",
                table: "Kline_1m",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_1mo_StockInfo_Symbol",
                table: "Kline_1mo",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_1w_StockInfo_Symbol",
                table: "Kline_1w",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_30m_StockInfo_Symbol",
                table: "Kline_30m",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_5m_StockInfo_Symbol",
                table: "Kline_5m",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kline_60m_StockInfo_Symbol",
                table: "Kline_60m",
                column: "Symbol",
                principalTable: "StockInfo",
                principalColumn: "Symbol",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kline_15m_StockInfo_Symbol",
                table: "Kline_15m");

            migrationBuilder.DropForeignKey(
                name: "FK_Kline_1d_StockInfo_Symbol",
                table: "Kline_1d");

            migrationBuilder.DropForeignKey(
                name: "FK_Kline_1m_StockInfo_Symbol",
                table: "Kline_1m");

            migrationBuilder.DropForeignKey(
                name: "FK_Kline_1mo_StockInfo_Symbol",
                table: "Kline_1mo");

            migrationBuilder.DropForeignKey(
                name: "FK_Kline_1w_StockInfo_Symbol",
                table: "Kline_1w");

            migrationBuilder.DropForeignKey(
                name: "FK_Kline_30m_StockInfo_Symbol",
                table: "Kline_30m");

            migrationBuilder.DropForeignKey(
                name: "FK_Kline_5m_StockInfo_Symbol",
                table: "Kline_5m");

            migrationBuilder.DropForeignKey(
                name: "FK_Kline_60m_StockInfo_Symbol",
                table: "Kline_60m");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_StockInfo_Symbol",
                table: "StockInfo");

            migrationBuilder.DropIndex(
                name: "IX_Kline_60m_Symbol",
                table: "Kline_60m");

            migrationBuilder.DropIndex(
                name: "IX_Kline_5m_Symbol",
                table: "Kline_5m");

            migrationBuilder.DropIndex(
                name: "IX_Kline_30m_Symbol",
                table: "Kline_30m");

            migrationBuilder.DropIndex(
                name: "IX_Kline_1w_Symbol",
                table: "Kline_1w");

            migrationBuilder.DropIndex(
                name: "IX_Kline_1mo_Symbol",
                table: "Kline_1mo");

            migrationBuilder.DropIndex(
                name: "IX_Kline_1m_Symbol",
                table: "Kline_1m");

            migrationBuilder.DropIndex(
                name: "IX_Kline_1d_Symbol",
                table: "Kline_1d");

            migrationBuilder.DropIndex(
                name: "IX_Kline_15m_Symbol",
                table: "Kline_15m");

            migrationBuilder.CreateIndex(
                name: "IX_StockInfo_Symbol",
                table: "StockInfo",
                column: "Symbol",
                unique: true);
        }
    }
}
