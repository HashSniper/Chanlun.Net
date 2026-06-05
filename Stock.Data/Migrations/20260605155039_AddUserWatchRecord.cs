using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stock.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWatchRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWatchRecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWatchRecord", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWatchRecord_Symbol_Resolution",
                table: "UserWatchRecord",
                columns: new[] { "Symbol", "Resolution" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWatchRecord");
        }
    }
}
