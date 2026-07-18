using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NisanDavetiye.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimelineGunAkisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Aciklama", "Baslik", "Saat" },
                values: new object[] { "Ailelerimizin geleneksel isteme merasimi.", "İsteme Merasimi", "19:00" });

            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 2,
                column: "Saat",
                value: "19:30");

            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Aciklama", "Baslik" },
                values: new object[] { "Yemek ikramımız başlayacaktır.", "Akşam Yemeği" });

            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Aciklama", "Baslik", "Saat" },
                values: new object[] { "Gecemiz müzik ve eğlence eşliğinde devam edecektir.", "Müzik & Eğlence", "20:30" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Aciklama", "Baslik", "Saat" },
                values: new object[] { "Misafirlerimizi karşılıyoruz.", "Karşılama", "18:30" });

            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 2,
                column: "Saat",
                value: "19:00");

            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Aciklama", "Baslik" },
                values: new object[] { "Kokteyl ve fotoğraf çekimi.", "Kokteyl" });

            migrationBuilder.UpdateData(
                table: "TimelineOgeleri",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Aciklama", "Baslik", "Saat" },
                values: new object[] { "Akşam yemeği servisi.", "Akşam Yemeği", "21:00" });
        }
    }
}
