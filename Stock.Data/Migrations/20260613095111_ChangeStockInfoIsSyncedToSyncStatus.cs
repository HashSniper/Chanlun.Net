using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStockInfoIsSyncedToSyncStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SyncStatus",
                table: "StockInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 保留数据映射：IsSynced=true -> SyncStatus=Synced(2)
            migrationBuilder.Sql(@"
                UPDATE [StockInfo]
                SET [SyncStatus] = 2
                WHERE [IsSynced] = 1;
            ");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "StockInfo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "StockInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // 回滚时保留映射：SyncStatus=Synced(2) -> IsSynced=true
            migrationBuilder.Sql(@"
                UPDATE [StockInfo]
                SET [IsSynced] = 1
                WHERE [SyncStatus] = 2;
            ");

            migrationBuilder.DropColumn(
                name: "SyncStatus",
                table: "StockInfo");
        }
    }
}
