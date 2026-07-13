using NisanDavetiye.BLL.DTOs;
using NisanDavetiye.BLL.Security;
using NisanDavetiye.DAL.Entities;
using NisanDavetiye.DAL.Repositories;

namespace NisanDavetiye.BLL.Services;

public class RsvpService : IRsvpService
{
    private readonly IRsvpRepository _repo;

    public RsvpService(IRsvpRepository repo) => _repo = repo;

    public async Task<RsvpDto> KaydetAsync(RsvpOlusturDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AdSoyad))
            throw new ArgumentException("Ad soyad zorunludur.");

        if (dto.AdSoyad.Trim().Length > 100)
            throw new ArgumentException("Ad soyad en fazla 100 karakter olabilir.");

        var telefon = NormalizeTelefon(dto.Telefon);
        if (telefon is null)
            throw new ArgumentException("Telefon numarası 0 ile başlamalı ve 11 haneli olmalıdır.");

        if (await _repo.ExistsByTelefonAsync(telefon))
            throw new ArgumentException("Bu telefon numarasıyla daha önce yanıt verilmiş.");

        if (dto.KisiSayisi < 1)
            throw new ArgumentException("Kişi sayısı en az 1 olmalıdır.");

        if (dto.KisiSayisi > 10)
            throw new ArgumentException("Kişi sayısı en fazla 10 olabilir.");

        var mesaj = dto.Mesaj?.Trim() ?? string.Empty;
        if (mesaj.Length > 500)
            throw new ArgumentException("Mesaj en fazla 500 karakter olabilir.");

        var kayit = new RsvpKayit
        {
            AdSoyad = dto.AdSoyad.Trim(),
            Telefon = telefon,
            Katilacak = dto.Katilacak,
            KisiSayisi = dto.Katilacak ? dto.KisiSayisi : 0,
            Mesaj = mesaj,
            OlusturmaTarihi = DateTime.UtcNow
        };

        var saved = await _repo.AddAsync(kayit);
        return Map(saved);
    }

    public async Task<IReadOnlyList<RsvpDto>> ListeleAsync()
    {
        var list = await _repo.GetVisibleForAdminAsync();
        return list.Select(Map).ToList();
    }

    public Task GizleFromAdminAsync(int id) => _repo.HideFromAdminAsync(id);

    public async Task<byte[]> ExcelExportAsync()
    {
        var list = await _repo.GetAllAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var sheet = workbook.Worksheets.Add("Katılım Yanıtları");

        sheet.Cell(1, 1).Value = "Ad Soyad";
        sheet.Cell(1, 2).Value = "Telefon";
        sheet.Cell(1, 3).Value = "Katılım";
        sheet.Cell(1, 4).Value = "Kişi Sayısı";
        sheet.Cell(1, 5).Value = "Mesaj";
        sheet.Cell(1, 6).Value = "Tarih";
        sheet.Cell(1, 7).Value = "Listeden Gizli";

        var row = 2;
        foreach (var item in list)
        {
            sheet.Cell(row, 1).Value = item.AdSoyad;
            sheet.Cell(row, 2).Value = item.Telefon;
            sheet.Cell(row, 3).Value = item.Katilacak ? "Evet" : "Hayır";
            sheet.Cell(row, 4).Value = item.KisiSayisi;
            sheet.Cell(row, 5).Value = item.Mesaj;
            sheet.Cell(row, 6).Value = item.OlusturmaTarihi.ToLocalTime().ToString("g");
            sheet.Cell(row, 7).Value = item.AdminListedenGizli ? "Evet" : "Hayır";
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static RsvpDto Map(RsvpKayit k) =>
        new(k.Id, k.AdSoyad, k.Telefon, k.Katilacak, k.KisiSayisi, k.Mesaj, k.OlusturmaTarihi);

    private static string? NormalizeTelefon(string input)
    {
        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits[0] == '0')
            return digits;

        return null;
    }
}
