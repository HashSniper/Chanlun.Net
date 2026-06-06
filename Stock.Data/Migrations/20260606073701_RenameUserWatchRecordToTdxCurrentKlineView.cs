using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserWatchRecordToTdxCurrentKlineView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserWatchRecord",
                table: "UserWatchRecord");

            migrationBuilder.RenameTable(
                name: "UserWatchRecord",
                newName: "TdxCurrentKlineView");

            migrationBuilder.RenameIndex(
                name: "IX_UserWatchRecord_Symbol_Resolution",
                table: "TdxCurrentKlineView",
                newName: "IX_TdxCurrentKlineView_Symbol_Resolution");

            migrationBuilder.AlterColumn<int>(
                name: "Resolution",
                table: "TdxCurrentKlineView",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TdxCurrentKlineView",
                table: "TdxCurrentKlineView",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TdxCurrentKlineView",
                table: "TdxCurrentKlineView");

            migrationBuilder.AlterColumn<string>(
                name: "Resolution",
                table: "TdxCurrentKlineView",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.RenameTable(
                name: "TdxCurrentKlineView",
                newName: "UserWatchRecord");

            migrationBuilder.RenameIndex(
                name: "IX_TdxCurrentKlineView_Symbol_Resolution",
                table: "UserWatchRecord",
                newName: "IX_UserWatchRecord_Symbol_Resolution");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserWatchRecord",
                table: "UserWatchRecord",
                column: "Id");
        }
    }
}
