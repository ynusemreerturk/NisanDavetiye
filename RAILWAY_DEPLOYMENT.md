# Railway Deployment

Bu proje **tek bir Railway servisi** olarak deploy edilir. React/Vite UI, ASP.NET Core API
tarafından `wwwroot` üzerinden sunulur; SQLite veritabanı ve yüklenen fotoğraflar
kalıcı bir Railway Volume altında (`/data`) saklanır.

> PostgreSQL veya ayrı bir veritabanı servisi eklenmez. `BLL`, `DAL` ve UI ayrı servis
> olarak deploy edilmez. `NisanDavetiye.UI.Runner` yalnızca yerel geliştirme içindir ve
> production image'a dahil edilmez.

## 1. Railway projesi oluştur

1. [Railway](https://railway.app) üzerinde **New Project** ile yeni bir proje oluştur.
2. **Deploy from GitHub repo** seçeneğiyle bu repoyu bağla.

## 2. Build ayarı (Dockerfile)

3. Servisin **Root Directory** alanını **boş bırak** (repo kökü).
4. Railway, repo kökündeki `Dockerfile` dosyasını otomatik algılar ve build bunun üzerinden
   yapılır. Ayrı bir build/start command girmene gerek yoktur (port bağlama Dockerfile
   `CMD` içinde `--urls http://0.0.0.0:${PORT}` ile hallolur).

## 3. Kalıcı Volume ekle

5. Servise bir **Volume** ekle (**Settings → Volumes → Add Volume**).
6. **Mount Path** değerini `/data` yap.

Bu volume hem SQLite veritabanını hem de yüklenen fotoğrafları tutar:

```
/data/nisandavetiye.db      → SQLite veritabanı
/data/uploads/galeri        → Misafir fotoğraf yüklemeleri
```

Uygulama başlangıçta bu klasörleri otomatik oluşturur.

## 4. Environment Variables

**Variables** sekmesinden aşağıdaki değişkenleri ekle. Section isimleri koddaki
configuration key'leriyle bire bir eşleşir (`__` iki alt çizgi = iç içe section).

### Zorunlu

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=Data Source=/data/nisandavetiye.db
GaleriStorage__UploadDirectory=/data/uploads/galeri
GaleriStorage__PublicUrlPrefix=/uploads/galeri
Admin__ApiKey=CHANGE_ME_EN_AZ_32_KARAKTER
MediaSigning__SigningKey=CHANGE_ME_EN_AZ_32_KARAKTER
```

> Production'da `Admin__ApiKey` **en az 32 karakter** olmalıdır, aksi halde uygulama
> başlatılırken hata verir. `MediaSigning__SigningKey` production'da zorunludur (boşsa
> uygulama başlamaz). Güçlü rastgele değerler üret, örn:
> `openssl rand -base64 48`

### CAPTCHA (Google reCAPTCHA v2)

Formların (RSVP + fotoğraf yükleme) çalışması için ya gerçek reCAPTCHA anahtarları gir,
ya da CAPTCHA'yı kapat.

CAPTCHA varsayılan olarak kapalıdır (`Recaptcha__Enabled=false`).

Google reCAPTCHA kullanılacaksa ([Admin Console](https://www.google.com/recaptcha/admin)):

1. **reCAPTCHA v2** → "I'm not a robot" checkbox seç.
2. Domain olarak Railway hostname'ini ekle: `cerenemre.up.railway.app` (ve lokal için `localhost`).
3. Site Key + Secret Key'i Railway Variables'a yaz:

```text
Recaptcha__Enabled=true
Recaptcha__SiteKey=YOUR_RECAPTCHA_SITE_KEY
Recaptcha__SecretKey=YOUR_RECAPTCHA_SECRET_KEY
```

> Eski `Turnstile__*` değişkenlerini kaldır. Domain Google konsolunda yoksa checkbox
> yüklenmez veya "invalid domain" hatası verir.

### İsteğe bağlı (varsayılanları var)

```text
GaleriStorage__MaxDailyUploadCount=100
MediaSigning__PublicUrlLifetimeHours=24
MediaSigning__AdminUrlLifetimeHours=12
```

> Gerçek secret değerlerini repoya commit etme; yalnızca Railway Variables üzerinden gir.

## 5. Domain

7. **Settings → Networking → Generate Domain** ile public bir domain oluştur.
   UI ve API aynı domain altında çalışır (`https://<sizin-domain>.up.railway.app`).

## 6. Replica ve Volume notları

8. SQLite kullanıldığı için servis **tek replica (1)** ile çalışmalıdır. Birden fazla
   replica aynı SQLite dosyasına yazamaz.
9. Volume için **backup** özelliğini etkinleştirmen önerilir (veri kaybına karşı).

## 7. Yapılmaması gerekenler

- ❌ PostgreSQL veya başka bir veritabanı servisi **ekleme** (SQLite kullanılıyor).
- ❌ Railway **pre-deploy migration command** ekleme. Migration'lar uygulama başlangıcında
  (`Database.Migrate()`) otomatik çalışır ve volume ancak runtime'da mount edilir; pre-deploy
  aşamasında `/data` erişilebilir değildir.
- ❌ UI ve API için ayrı servisler oluşturma.

## 8. Health check

10. İstersen **Settings → Deploy → Healthcheck Path** alanına `/health` gir.
    Bu endpoint kimlik doğrulaması gerektirmez ve `{ "status": "ok", "timestamp": ... }` döner.

## 9. Frontend / domain notu

Frontend, API'ye **relative `/api`** yolu ile istek atar; UI ve API aynı origin altında
çalıştığı için ekstra domain/origin ayarı gerekmez.

reCAPTCHA kullanıyorsan Google Admin Console'da ilgili **site key** için Railway
domain'ini (`<sizin-domain>.up.railway.app`) izinli domain olarak eklemeyi unutma.
