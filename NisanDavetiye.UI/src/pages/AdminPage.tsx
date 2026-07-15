import { type FormEvent, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  deleteAllUploadedGalleryPhotos,
  deleteGalleryPhoto,
  deleteRsvp,
  downloadGalleryZip,
  exportRsvpExcel,
  fetchDavetiyeAdmin,
  fetchDriveStatus,
  fetchRsvpList,
  triggerDriveOffload,
  updateDavetiye,
  verifyPanelAccess,
  type GaleriDriveStatus,
} from '../api/client'
import type { DavetiyeAdmin, RsvpRecord } from '../types'

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  const units = ['KB', 'MB', 'GB']
  let value = bytes / 1024
  let unit = 0
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024
    unit++
  }
  return `${value.toFixed(1)} ${units[unit]}`
}

const ADMIN_KEY_STORAGE = 'nisan-admin-key'
const PANEL_UID_PATTERN = /^[a-f0-9]{32}$/i

export function AdminPage() {
  const { panelUid: panelUidParam = '' } = useParams()
  const panelUid = panelUidParam.trim()

  const [accessValid, setAccessValid] = useState<boolean | null>(null)
  const [adminKey, setAdminKey] = useState(() => localStorage.getItem(ADMIN_KEY_STORAGE) ?? '')
  const [loggedIn, setLoggedIn] = useState(false)
  const [davetiye, setDavetiye] = useState<DavetiyeAdmin | null>(null)
  const [rsvpList, setRsvpList] = useState<RsvpRecord[]>([])
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [driveStatus, setDriveStatus] = useState<GaleriDriveStatus | null>(null)
  const [driveBusy, setDriveBusy] = useState(false)

  const loadData = async (key: string) => {
    const [d, r] = await Promise.all([
      fetchDavetiyeAdmin(panelUid, key),
      fetchRsvpList(panelUid, key),
    ])
    setDavetiye(d)
    setRsvpList(r)
    setLoggedIn(true)
    localStorage.setItem(ADMIN_KEY_STORAGE, key)
    fetchDriveStatus(panelUid, key)
      .then(setDriveStatus)
      .catch(() => setDriveStatus(null))
  }

  useEffect(() => {
    if (!PANEL_UID_PATTERN.test(panelUid)) {
      setAccessValid(false)
      return
    }

    verifyPanelAccess(panelUid)
      .then(setAccessValid)
      .catch(() => setAccessValid(false))
  }, [panelUid])

  useEffect(() => {
    if (accessValid && adminKey) {
      loadData(adminKey).catch(() => setLoggedIn(false))
    }
  }, [accessValid])

  // Bilgi/başarı mesajları birkaç saniye sonra otomatik kaybolsun.
  useEffect(() => {
    if (!message && !error) return
    const t = setTimeout(() => {
      setMessage('')
      setError('')
    }, 4000)
    return () => clearTimeout(t)
  }, [message, error])

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      await loadData(adminKey)
    } catch {
      setError('Geçersiz yönetim anahtarı')
      setLoggedIn(false)
    }
  }

  const handleSave = async (e: FormEvent) => {
    e.preventDefault()
    if (!davetiye) return
    setMessage('')
    try {
      const updated = await updateDavetiye(davetiye, panelUid, adminKey)
      setDavetiye(updated)
      setMessage('Davetiye ayarları kaydedildi.')
    } catch {
      setMessage('Kayıt başarısız.')
    }
  }

  const handleExport = async () => {
    try {
      const blob = await exportRsvpExcel(panelUid, adminKey)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'katilim-yanitlari.xlsx'
      a.click()
      URL.revokeObjectURL(url)
    } catch {
      setMessage('Excel indirilemedi.')
    }
  }

  const uploadedPhotos = davetiye?.galeri.filter((g) => g.misafirYuklemesi) ?? []

  const showError = (text: string) => {
    setMessage('')
    setError(text)
  }

  const showSuccess = (text: string) => {
    setError('')
    setMessage(text)
  }

  const handleDelete = async (id: number) => {
    if (
      !confirm(
        'Bu kayıt listeden kaldırılsın mı? Veritabanında ve Excel arşivinde kalır.',
      )
    ) {
      return
    }
    try {
      await deleteRsvp(id, panelUid, adminKey)
      setRsvpList((prev) => prev.filter((r) => r.id !== id))
      showSuccess('Kayıt listeden kaldırıldı. Excel indirdiğinizde hâlâ görünür.')
    } catch (err) {
      showError(err instanceof Error ? err.message : 'Kayıt kaldırılamadı.')
    }
  }

  const handleRefresh = async () => {
    try {
      await loadData(adminKey)
      setMessage('Veriler yenilendi.')
    } catch {
      setMessage('Veriler yenilenemedi.')
    }
  }

  const handleDownloadGalleryZip = async () => {
    try {
      const blob = await downloadGalleryZip(panelUid, adminKey)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `nisandavetiye-galeri-${new Date().toISOString().slice(0, 10)}.zip`
      a.click()
      URL.revokeObjectURL(url)
      showSuccess('Galeri zip dosyası indirildi.')
    } catch (err) {
      showError(err instanceof Error ? err.message : 'Galeri indirilemedi.')
    }
  }

  const handleDriveOffload = async () => {
    setDriveBusy(true)
    try {
      const result = await triggerDriveOffload(panelUid, adminKey)
      showSuccess(result.message)
      const status = await fetchDriveStatus(panelUid, adminKey).catch(() => null)
      if (status) setDriveStatus(status)
    } catch (err) {
      showError(err instanceof Error ? err.message : 'Drive aktarımı başlatılamadı.')
    } finally {
      setDriveBusy(false)
    }
  }

  const handleDeletePhoto = async (id: number) => {
    if (!confirm('Bu fotoğraf sunucudan silinsin mi? Bilgisayarınızda veya telefonunuzda yedeği olduğundan emin olun.')) {
      return
    }
    try {
      await deleteGalleryPhoto(id, panelUid, adminKey)
      await loadData(adminKey)
      showSuccess('Fotoğraf sunucudan silindi.')
    } catch (err) {
      showError(err instanceof Error ? err.message : 'Fotoğraf silinemedi.')
    }
  }

  const handleDeleteAllPhotos = async () => {
    if (uploadedPhotos.length === 0) return
    if (
      !confirm(
        `${uploadedPhotos.length} fotoğraf sunucudan kalıcı olarak silinecek. Önce zip indirdiğinizden veya yedeklediğinizden emin misiniz?`,
      )
    ) {
      return
    }
    try {
      const result = await deleteAllUploadedGalleryPhotos(panelUid, adminKey)
      await loadData(adminKey)
      showSuccess(`${result.deletedCount} fotoğraf sunucudan silindi; alan açıldı.`)
    } catch (err) {
      showError(err instanceof Error ? err.message : 'Fotoğraflar silinemedi.')
    }
  }

  const inviteUrl = davetiye
    ? `${window.location.origin}/i/${davetiye.davetUid}`
    : ''

  const panelUrl = davetiye
    ? `${window.location.origin}/p/${davetiye.panelUid}`
    : `${window.location.origin}/p/${panelUid}`

  const handleCopyInviteLink = async () => {
    if (!inviteUrl) return
    try {
      await navigator.clipboard.writeText(inviteUrl)
      showSuccess('Davetiye linki panoya kopyalandı.')
    } catch {
      showError('Link kopyalanamadı. Aşağıdaki adresi elle paylaşın.')
    }
  }

  const handleCopyPanelLink = async () => {
    if (!panelUrl) return
    try {
      await navigator.clipboard.writeText(panelUrl)
      showSuccess('Yönetim paneli linki panoya kopyalandı.')
    } catch {
      showError('Link kopyalanamadı. Aşağıdaki adresi elle paylaşın.')
    }
  }

  if (accessValid === null) {
    return <div className="loading-screen">Yükleniyor...</div>
  }

  if (!accessValid) {
    return <div className="loading-screen">Geçersiz yönetim bağlantısı.</div>
  }

  if (!loggedIn) {
    return (
      <div className="admin">
        <div className="admin__login">
          <h1>Yönetim Paneli</h1>
          <p className="admin__hint">Yalnızca yetkili kişiler giriş yapabilir.</p>
          <form onSubmit={handleLogin}>
            <input
              type="password"
              value={adminKey}
              onChange={(e) => setAdminKey(e.target.value)}
              placeholder="Yönetim anahtarı"
              required
            />
            <button type="submit" className="btn-primary">Giriş</button>
          </form>
          {error && <p className="rsvp__error">{error}</p>}
        </div>
      </div>
    )
  }

  if (!davetiye) return <div className="loading-screen">Yükleniyor...</div>

  return (
    <div className="admin">
      <header className="admin__header">
        <h1>Yönetim Paneli</h1>
        <div className="admin__actions">
          <button type="button" className="btn-outline" onClick={handleRefresh}>
            Yenile
          </button>
          <button type="button" className="btn-outline" onClick={handleExport}>
            Katılım Listesi (Excel)
          </button>
          <Link to={`/i/${davetiye.davetUid}`} className="btn-outline">Davetiyeyi Gör</Link>
        </div>
      </header>

      <section className="admin__invite-link">
        <h2>Yönetim Paneli Linki</h2>
        <p className="admin__hint">
          Bu gizli linki yalnızca yetkili kişilerle paylaşın. Tahmin edilebilir adresler
          (<code>/admin</code>, <code>/yonetim</code> vb.) kullanılmaz.
        </p>
        <div className="admin__invite-row">
          <input readOnly value={panelUrl} aria-label="Yönetim paneli linki" />
          <button type="button" className="btn-outline" onClick={handleCopyPanelLink}>
            Kopyala
          </button>
        </div>
      </section>

      <section className="admin__invite-link">
        <h2>Davetiye Linki</h2>
        <p className="admin__hint">
          Bu linki yalnızca davetlilerle paylaşın. Ana sayfa (<code>/</code>) davetiyeyi göstermez.
        </p>
        <div className="admin__invite-row">
          <input readOnly value={inviteUrl} aria-label="Davetiye linki" />
          <button type="button" className="btn-outline" onClick={handleCopyInviteLink}>
            Kopyala
          </button>
        </div>
      </section>

      {message && <p className="admin__message">{message}</p>}
      {error && <p className="admin__error">{error}</p>}

      <form className="admin__form" onSubmit={handleSave}>
        <h2>Davetiye Ayarları</h2>
        <div className="admin__grid">
          <label>Gelin Adı<input value={davetiye.gelinAdi} onChange={(e) => setDavetiye({ ...davetiye, gelinAdi: e.target.value })} /></label>
          <label>Damat Adı<input value={davetiye.damatAdi} onChange={(e) => setDavetiye({ ...davetiye, damatAdi: e.target.value })} /></label>
          <label>Baş Harfler<input value={davetiye.basHarpler} onChange={(e) => setDavetiye({ ...davetiye, basHarpler: e.target.value })} /></label>
          <label>Başlık<input value={davetiye.baslik} onChange={(e) => setDavetiye({ ...davetiye, baslik: e.target.value })} /></label>
          <label className="admin__full">Hoş geldin metni<textarea rows={3} value={davetiye.hosgeldinMetni} onChange={(e) => setDavetiye({ ...davetiye, hosgeldinMetni: e.target.value })} /></label>
          <label>Tarih<input type="datetime-local" value={davetiye.etkinlikTarihi.slice(0, 16)} onChange={(e) => setDavetiye({ ...davetiye, etkinlikTarihi: new Date(e.target.value).toISOString() })} /></label>
          <label>Mekân<input value={davetiye.mekanAdi} onChange={(e) => setDavetiye({ ...davetiye, mekanAdi: e.target.value })} /></label>
          <label className="admin__full">Adres<input value={davetiye.adres} onChange={(e) => setDavetiye({ ...davetiye, adres: e.target.value })} /></label>
          <label className="admin__full">Kapak görsel URL<input value={davetiye.kapakGorselUrl} onChange={(e) => setDavetiye({ ...davetiye, kapakGorselUrl: e.target.value })} /></label>
          <label className="admin__full">Çift foto URL<input value={davetiye.ciftFotoUrl} onChange={(e) => setDavetiye({ ...davetiye, ciftFotoUrl: e.target.value })} /></label>
          <label className="admin__full">Açılış video URL<input value={davetiye.acilisVideoUrl} onChange={(e) => setDavetiye({ ...davetiye, acilisVideoUrl: e.target.value })} /></label>
          <label className="admin__full">
            Müzik (Ballerina — Yehezkel Raz)
            <input value={davetiye.muzikUrl} onChange={(e) => setDavetiye({ ...davetiye, muzikUrl: e.target.value })} />
            <small className="admin__hint">
              MP3: <code>public/assets/audio/ballerina.mp3</code> — Yehezkel Raz, «Ballerina»
            </small>
          </label>
          <label className="admin__full">Zarf arka plan URL<input value={davetiye.zarfArkaPlanUrl} onChange={(e) => setDavetiye({ ...davetiye, zarfArkaPlanUrl: e.target.value })} /></label>
          <label className="admin__full">Harita embed URL<textarea rows={2} value={davetiye.haritaEmbedUrl} onChange={(e) => setDavetiye({ ...davetiye, haritaEmbedUrl: e.target.value })} /></label>
          <label className="admin__full">Harita link<input value={davetiye.haritaLink} onChange={(e) => setDavetiye({ ...davetiye, haritaLink: e.target.value })} /></label>
          <label className="admin__full admin__toggle">
            <input
              type="checkbox"
              checked={davetiye.galeriYuklemeAcik}
              onChange={(e) => setDavetiye({ ...davetiye, galeriYuklemeAcik: e.target.checked })}
            />
            <span>
              Misafir fotoğraf yüklemesi açık
              <small className="admin__hint">
                Kapalıyken davetlilere "yükleme kapalı" mesajı gösterilir. Değişikliği
                uygulamak için <strong>Kaydet</strong>'e basın.
              </small>
            </span>
          </label>
        </div>
        <button type="submit" className="btn-primary">Kaydet</button>
      </form>

      <section className="admin__rsvp">
        <h2>Katılım Yanıtları ({rsvpList.length})</h2>
        <p className="admin__hint">
          Davetlilerin «geleceğim / gelemeyeceğim» bildirimleri. Listeden kaldırılan kayıtlar
          arşivde kalır; Excel dosyasında tüm yanıtlar görünür.
        </p>
        <table>
          <thead>
            <tr>
              <th>Ad Soyad</th>
              <th>Telefon</th>
              <th>Katılım</th>
              <th>Kişi</th>
              <th>Mesaj</th>
              <th>Tarih</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rsvpList.map((r) => (
              <tr key={r.id}>
                <td>{r.adSoyad}</td>
                <td>{r.telefon}</td>
                <td>{r.katilacak ? 'Evet' : 'Hayır'}</td>
                <td>{r.kisiSayisi}</td>
                <td>{r.mesaj}</td>
                <td>{new Date(r.olusturmaTarihi).toLocaleString('tr-TR')}</td>
                <td>
                  <button type="button" onClick={() => handleDelete(r.id)}>Listeden Kaldır</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section className="admin__gallery">
        <div className="admin__gallery-header">
          <h2>Misafir Fotoğrafları ({uploadedPhotos.length})</h2>
          {uploadedPhotos.length > 0 && (
            <div className="admin__gallery-actions">
              <button type="button" className="btn-outline" onClick={handleDownloadGalleryZip}>
                Zip İndir
              </button>
              <button type="button" className="btn-outline admin__danger" onClick={handleDeleteAllPhotos}>
                Sunucudan Tümünü Sil
              </button>
            </div>
          )}
        </div>
        <p className="admin__hint">
          Önce zip indirip bilgisayarınıza veya telefonunuza kaydedin, ardından sunucudan silerek alan açabilirsiniz.
        </p>

        {driveStatus && (
          <div className="admin__drive">
            <div className="admin__drive-row">
              <span>
                Sunucu kullanımı: <strong>{formatBytes(driveStatus.localUsedBytes)}</strong> /{' '}
                {driveStatus.thresholdMegabytes} MB eşik
              </span>
              <span>
                Drive'a aktarılan: <strong>{driveStatus.offloadedCount}</strong> · Bekleyen:{' '}
                <strong>{driveStatus.pendingCount}</strong>
              </span>
            </div>
            {!driveStatus.driveEnabled ? (
              <p className="admin__hint">
                Google Drive entegrasyonu kapalı. Railway ortam değişkenlerinde
                <code> DriveOffload__Enabled=true</code> ve OAuth anahtarlarını tanımlayın.
              </p>
            ) : (
              <>
                {driveStatus.overThreshold && (
                  <p className="admin__hint">
                    Eşik aşıldı; fotoğraflar arka planda otomatik olarak Drive'a aktarılıyor.
                  </p>
                )}
                <button
                  type="button"
                  className="btn-outline"
                  onClick={handleDriveOffload}
                  disabled={driveBusy || driveStatus.pendingCount === 0}
                >
                  {driveBusy ? 'Kuyruğa alınıyor…' : "Şimdi Drive'a Aktar"}
                </button>
              </>
            )}
          </div>
        )}
        {uploadedPhotos.length === 0 ? (
          <p className="admin__hint">Henüz misafir fotoğrafı yok.</p>
        ) : (
          <div className="admin__gallery-grid">
            {uploadedPhotos.map((photo) => (
              <div key={photo.id} className="admin__gallery-card">
                <a
                  href={photo.url}
                  target="_blank"
                  rel="noreferrer"
                  className="admin__gallery-item"
                  title={photo.altMetin || 'Fotoğraf'}
                >
                  <img src={photo.url} alt={photo.altMetin || 'Galeri fotoğrafı'} loading="lazy" />
                </a>
                {photo.driveAktarildi && (
                  <span className="admin__gallery-badge">Drive'a taşındı</span>
                )}
                <button
                  type="button"
                  className="admin__gallery-delete"
                  onClick={() => handleDeletePhoto(photo.id)}
                >
                  Sil
                </button>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
