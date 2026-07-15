using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NisanDavetiye.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DavetiyeAyarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GelinAdi = table.Column<string>(type: "TEXT", nullable: false),
                    DamatAdi = table.Column<string>(type: "TEXT", nullable: false),
                    BasHarpler = table.Column<string>(type: "TEXT", nullable: false),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    HosgeldinMetni = table.Column<string>(type: "TEXT", nullable: false),
                    EtkinlikTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MekanAdi = table.Column<string>(type: "TEXT", nullable: false),
                    Adres = table.Column<string>(type: "TEXT", nullable: false),
                    HaritaEmbedUrl = table.Column<string>(type: "TEXT", nullable: false),
                    HaritaLink = table.Column<string>(type: "TEXT", nullable: false),
                    KapakGorselUrl = table.Column<string>(type: "TEXT", nullable: false),
                    CiftFotoUrl = table.Column<string>(type: "TEXT", nullable: false),
                    AcilisVideoUrl = table.Column<string>(type: "TEXT", nullable: false),
                    MuzikUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ZarfArkaPlanUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DavetiyeAyarlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GaleriResimleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    AltMetin = table.Column<string>(type: "TEXT", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GaleriResimleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RsvpKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", nullable: false),
                    Katilacak = table.Column<bool>(type: "INTEGER", nullable: false),
                    KisiSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    Mesaj = table.Column<string>(type: "TEXT", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RsvpKayitlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimelineOgeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    Saat = table.Column<string>(type: "TEXT", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineOgeleri", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DavetiyeAyarlari",
                columns: new[] { "Id", "AcilisVideoUrl", "Adres", "BasHarpler", "Baslik", "CiftFotoUrl", "DamatAdi", "EtkinlikTarihi", "GelinAdi", "HaritaEmbedUrl", "HaritaLink", "HosgeldinMetni", "KapakGorselUrl", "MekanAdi", "MuzikUrl", "ZarfArkaPlanUrl" },
                values: new object[] { 1, "/assets/video/acilis.mp4", "Bebek, Cevdet Paşa Cd. No:12, Beşiktaş/İstanbul", "A & M", "Nişanımıza Davetlisiniz", "/assets/images/cift.jpg", "Emre", new DateTime(2026, 8, 15, 19, 0, 0, 0, DateTimeKind.Unspecified), "Ceren", "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3006.0!2d29.043!3d41.077!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zNDHCsDA0JzM3LjIiTiAyOcKwMDInMzQuOCJF!5e0!3m2!1str!2str!4v1", "https://maps.google.com/?q=Bebek+İstanbul", "Bu mutlu günümüzde sizleri de aramızda görmekten büyük mutluluk duyarız. Sevgi ve saygılarımızla.", "/assets/images/kapak.jpg", "Grand Bosphorus Salon", "/assets/audio/muzik.mp3", "/assets/images/zarf-arka.jpg" });

            migrationBuilder.InsertData(
                table: "GaleriResimleri",
                columns: new[] { "Id", "AltMetin", "Sira", "Url" },
                values: new object[,]
                {
                    { 1, "Anı 1", 1, "/assets/images/galeri-1.jpg" },
                    { 2, "Anı 2", 2, "/assets/images/galeri-2.jpg" },
                    { 3, "Anı 3", 3, "/assets/images/galeri-3.jpg" },
                    { 4, "Anı 4", 4, "/assets/images/galeri-4.jpg" }
                });

            migrationBuilder.InsertData(
                table: "TimelineOgeleri",
                columns: new[] { "Id", "Aciklama", "Baslik", "Saat", "Sira" },
                values: new object[,]
                {
                    { 1, "Misafirlerimizi karşılıyoruz.", "Karşılama", "18:30", 1 },
                    { 2, "Nişan merasimimiz başlıyor.", "Nişan Töreni", "19:00", 2 },
                    { 3, "Kokteyl ve fotoğraf çekimi.", "Kokteyl", "20:00", 3 },
                    { 4, "Akşam yemeği servisi.", "Akşam Yemeği", "21:00", 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DavetiyeAyarlari");

            migrationBuilder.DropTable(
                name: "GaleriResimleri");

            migrationBuilder.DropTable(
                name: "RsvpKayitlari");

            migrationBuilder.DropTable(
                name: "TimelineOgeleri");
        }
    }
}
