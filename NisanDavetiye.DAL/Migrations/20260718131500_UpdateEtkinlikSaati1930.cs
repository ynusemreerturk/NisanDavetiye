using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NisanDavetiye.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEtkinlikSaati1930 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DavetiyeAyarlari",
                keyColumn: "Id",
                keyValue: 1,
                column: "EtkinlikTarihi",
                value: new DateTime(2026, 7, 24, 19, 30, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DavetiyeAyarlari",
                keyColumn: "Id",
                keyValue: 1,
                column: "EtkinlikTarihi",
                value: new DateTime(2026, 7, 24, 19, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
