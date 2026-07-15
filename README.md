# NisanDavetiye

Nişan e-davetiyesi — DAL → BLL → API → UI katmanlı mimari.

## Proje yapısı

```
NisanDavetiye.sln
├── NisanDavetiye.DAL       Veritabanı, entity, repository
├── NisanDavetiye.BLL       İş kuralları, servisler
├── NisanDavetiye.API       REST API
└── NisanDavetiye.UI        React davetiye arayüzü + admin
```

## Çalıştırma

### 1. API (Visual Studio veya terminal)

```bash
cd NisanDavetiye.API
dotnet run
```

API: http://localhost:5279

### 2. UI

```bash
cd NisanDavetiye.UI
npm install
npm run dev
```

Davetiye linki yönetim panelinde görünür. Yönetim paneli linki API başlatıldığında konsola yazılır (`/p/{panelUid}`).  
Ana sayfa (`/`) davetiyeyi göstermez; davetiye adresi: `/i/24temmuz2026`.

**Yönetim paneli:** API'yi çalıştırın; konsolda `Yönetim paneli: http://localhost:5173/p/...` satırını görün.  
Bu gizli linki yer imlerine ekleyin; `/admin` artık çalışmaz.

**Yönetim anahtarı:** Geliştirmede `appsettings.Development.json` içinde tanımlıdır (en az 32 karakter).  
Canlı ortamda `Admin__ApiKey`, `MediaSigning__SigningKey` ve reCAPTCHA anahtarlarını ortam değişkeni ile verin.

## Railway Deployment

Proje tek bir Railway servisi olarak (UI + API aynı container, SQLite ve yüklemeler `/data`
volume'unda) Dockerfile ile deploy edilir. Ayrıntılı adımlar ve environment variable listesi
için [`RAILWAY_DEPLOYMENT.md`](./RAILWAY_DEPLOYMENT.md) dosyasına bakın.

## Güvenlik

- Davetiye URL'si sabit slug ile açılır (`/i/24temmuz2026`)
- Yönetim paneli URL'si ayrı 32 karakterlik gizli `uid` ile korunur (`/p/xyz789…`)
- RSVP ve fotoğraf yükleme: davet anahtarı + Google reCAPTCHA v2 + telefon başına tek kayıt
- Yönetim işlemleri: `X-Panel-Uid` + `X-Admin-Key` (üretimde en az 32 karakter)
- Form isteklerinde IP başına dakikada 20 istek; panel API'de dakikada 60 istek
- Misafir fotoğrafları magic byte doğrulaması, günlük kota ve yönetici onayından sonra yayınlanır
- Yüklenen fotoğraflar imzalı URL ile sunulur (`/api/media/galeri/...`); doğrudan `/uploads` erişimi kapalı
- Seeder yalnızca ilk kurulumda çalışır; restart'ta ayarları ezmez

## Medya dosyalarını değiştirme

Kendi resim, video ve müziğinizi şu klasöre koyun (aynı dosya adlarıyla üzerine yazın):

```
NisanDavetiye.UI/public/assets/
├── images/
│   ├── kapak.jpg
│   ├── cift.jpg
│   ├── zarf-arka.jpg
│   ├── galeri-1.jpg … galeri-4.jpg
├── video/
│   └── acilis.mp4
└── audio/
    └── muzik.mp3
```

Alternatif: Admin panelinden URL alanlarını güncelleyebilirsiniz.

## Özellikler

- Animasyonlu zarf + balmumu mührü
- Açılış videosu
- Arka plan müziği
- Kapak, hoş geldin, geri sayım
- Tarih, mekân, harita, gün akışı
- Fotoğraf galerisi
- RSVP formu
- Admin paneli + Excel dışa aktarma
- Davetiye ayarları düzenleme
