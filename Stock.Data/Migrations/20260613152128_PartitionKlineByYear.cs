using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Data.Migrations
{
    /// <inheritdoc />
    public partial class PartitionKlineByYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 删除原聚集主键
            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_1m",
                table: "Kline_1m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_5m",
                table: "Kline_5m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_15m",
                table: "Kline_15m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_30m",
                table: "Kline_30m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_60m",
                table: "Kline_60m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_1d",
                table: "Kline_1d");

            // 2. 删除原 (Symbol, TradeTime) 唯一索引
            migrationBuilder.DropIndex(
                name: "IX_Kline1m_Symbol_Time",
                table: "Kline_1m");

            migrationBuilder.DropIndex(
                name: "IX_Kline5m_Symbol_Time",
                table: "Kline_5m");

            migrationBuilder.DropIndex(
                name: "IX_Kline15m_Symbol_Time",
                table: "Kline_15m");

            migrationBuilder.DropIndex(
                name: "IX_Kline30m_Symbol_Time",
                table: "Kline_30m");

            migrationBuilder.DropIndex(
                name: "IX_Kline60m_Symbol_Time",
                table: "Kline_60m");

            migrationBuilder.DropIndex(
                name: "IX_Kline1d_Symbol_Time",
                table: "Kline_1d");

            // 3. 重新添加非聚集主键 (Id)
            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_1m",
                table: "Kline_1m",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_5m",
                table: "Kline_5m",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_15m",
                table: "Kline_15m",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_30m",
                table: "Kline_30m",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_60m",
                table: "Kline_60m",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_1d",
                table: "Kline_1d",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            // 4. 创建分区函数与分区方案（按 TradeTime 年份划分，RANGE RIGHT）
            migrationBuilder.Sql(@"
                CREATE PARTITION FUNCTION pf_Kline_TradeTime(datetime2(0))
                AS RANGE RIGHT FOR VALUES (
                    '2021-01-01', '2022-01-01', '2023-01-01',
                    '2024-01-01', '2025-01-01', '2026-01-01'
                );

                CREATE PARTITION SCHEME ps_Kline_TradeTime
                AS PARTITION pf_Kline_TradeTime
                ALL TO ([PRIMARY]);
            ");

            // 5. 创建按年份分区的聚集索引 (Symbol, TradeTime)
            migrationBuilder.Sql(@"
                CREATE UNIQUE CLUSTERED INDEX [IX_Kline1m_Symbol_Time]
                ON [Kline_1m] ([Symbol], [TradeTime])
                ON ps_Kline_TradeTime([TradeTime]);

                CREATE UNIQUE CLUSTERED INDEX [IX_Kline5m_Symbol_Time]
                ON [Kline_5m] ([Symbol], [TradeTime])
                ON ps_Kline_TradeTime([TradeTime]);

                CREATE UNIQUE CLUSTERED INDEX [IX_Kline15m_Symbol_Time]
                ON [Kline_15m] ([Symbol], [TradeTime])
                ON ps_Kline_TradeTime([TradeTime]);

                CREATE UNIQUE CLUSTERED INDEX [IX_Kline30m_Symbol_Time]
                ON [Kline_30m] ([Symbol], [TradeTime])
                ON ps_Kline_TradeTime([TradeTime]);

                CREATE UNIQUE CLUSTERED INDEX [IX_Kline60m_Symbol_Time]
                ON [Kline_60m] ([Symbol], [TradeTime])
                ON ps_Kline_TradeTime([TradeTime]);

                CREATE UNIQUE CLUSTERED INDEX [IX_Kline1d_Symbol_Time]
                ON [Kline_1d] ([Symbol], [TradeTime])
                ON ps_Kline_TradeTime([TradeTime]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. 删除非聚集主键
            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_1m",
                table: "Kline_1m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_5m",
                table: "Kline_5m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_15m",
                table: "Kline_15m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_30m",
                table: "Kline_30m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_60m",
                table: "Kline_60m");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Kline_1d",
                table: "Kline_1d");

            // 2. 删除聚集分区索引
            migrationBuilder.DropIndex(
                name: "IX_Kline1m_Symbol_Time",
                table: "Kline_1m");

            migrationBuilder.DropIndex(
                name: "IX_Kline5m_Symbol_Time",
                table: "Kline_5m");

            migrationBuilder.DropIndex(
                name: "IX_Kline15m_Symbol_Time",
                table: "Kline_15m");

            migrationBuilder.DropIndex(
                name: "IX_Kline30m_Symbol_Time",
                table: "Kline_30m");

            migrationBuilder.DropIndex(
                name: "IX_Kline60m_Symbol_Time",
                table: "Kline_60m");

            migrationBuilder.DropIndex(
                name: "IX_Kline1d_Symbol_Time",
                table: "Kline_1d");

            // 3. 删除分区方案和分区函数
            migrationBuilder.Sql(@"
                DROP PARTITION SCHEME ps_Kline_TradeTime;
                DROP PARTITION FUNCTION pf_Kline_TradeTime;
            ");

            // 4. 恢复原来的聚集主键 (Id)
            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_1m",
                table: "Kline_1m",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_5m",
                table: "Kline_5m",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_15m",
                table: "Kline_15m",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_30m",
                table: "Kline_30m",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_60m",
                table: "Kline_60m",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Kline_1d",
                table: "Kline_1d",
                column: "Id");

            // 5. 恢复原来的非聚集唯一索引 (Symbol, TradeTime)
            migrationBuilder.CreateIndex(
                name: "IX_Kline1m_Symbol_Time",
                table: "Kline_1m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline5m_Symbol_Time",
                table: "Kline_5m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline15m_Symbol_Time",
                table: "Kline_15m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline30m_Symbol_Time",
                table: "Kline_30m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline60m_Symbol_Time",
                table: "Kline_60m",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kline1d_Symbol_Time",
                table: "Kline_1d",
                columns: new[] { "Symbol", "TradeTime" },
                unique: true);
        }
    }
}
