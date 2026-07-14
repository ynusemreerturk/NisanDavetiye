import type { Davetiye, DavetiyeAdmin, RsvpInput, RsvpRecord } from '../types'

const API = '/api'
const ADMIN_KEY_HEADER = 'X-Admin-Key'
const DAVET_KEY_HEADER = 'X-Davet-Key'
const PANEL_UID_HEADER = 'X-Panel-Uid'

function panelHeaders(panelUid: string, adminKey: string): HeadersInit {
  return {
    [PANEL_UID_HEADER]: panelUid,
    [ADMIN_KEY_HEADER]: adminKey,
  }
}

export async function fetchClientConfig(): Promise<{
  recaptchaSiteKey: string
  recaptchaEnabled: boolean
}> {
  const res = await fetch(`${API}/config/client`)
  if (!res.ok) return { recaptchaSiteKey: '', recaptchaEnabled: false }
  return res.json()
}

export async function verifyPanelAccess(panelUid: string): Promise<boolean> {
  const res = await fetch(`${API}/panel/access/${encodeURIComponent(panelUid)}`)
  return res.ok
}

export async function fetchDavetiye(inviteKey: string): Promise<Davetiye> {
  const res = await fetch(`${API}/davetiye/${encodeURIComponent(inviteKey)}`)
  if (!res.ok) throw new Error('Davetiye yüklenemedi')
  return res.json()
}

export async function fetchDavetiyeAdmin(
  panelUid: string,
  adminKey: string,
): Promise<DavetiyeAdmin> {
  const res = await fetch(`${API}/panel/davetiye`, {
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) throw new Error('Davetiye yüklenemedi')
  return res.json()
}

export async function submitRsvp(inviteKey: string, data: RsvpInput): Promise<void> {
  const res = await fetch(`${API}/rsvp`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      [DAVET_KEY_HEADER]: inviteKey,
    },
    body: JSON.stringify(data),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message ?? 'Yanıtınız gönderilemedi')
  }
}

export async function uploadGalleryPhotos(
  inviteKey: string,
  files: File[],
  captchaToken: string,
): Promise<{ uploadedCount: number; fileNames: string[]; message: string }> {
  const formData = new FormData()
  for (const file of files) {
    formData.append('files', file)
  }
  formData.append('captchaToken', captchaToken)

  const res = await fetch(`${API}/galeri/upload`, {
    method: 'POST',
    headers: { [DAVET_KEY_HEADER]: inviteKey },
    body: formData,
  })

  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message ?? 'Fotoğraflar yüklenemedi')
  }

  return res.json()
}

export async function downloadGalleryZip(panelUid: string, adminKey: string): Promise<Blob> {
  const res = await fetch(`${API}/panel/galeri/export`, {
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message ?? 'Galeri indirilemedi')
  }
  return res.blob()
}

export async function approveGalleryPhoto(
  id: number,
  panelUid: string,
  adminKey: string,
): Promise<void> {
  const res = await fetch(`${API}/panel/galeri/${id}/approve`, {
    method: 'POST',
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message ?? 'Fotoğraf onaylanamadı')
  }
}

export async function rejectGalleryPhoto(
  id: number,
  panelUid: string,
  adminKey: string,
): Promise<void> {
  const res = await fetch(`${API}/panel/galeri/${id}/reject`, {
    method: 'POST',
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message ?? 'Fotoğraf reddedilemedi')
  }
}

export async function deleteGalleryPhoto(
  id: number,
  panelUid: string,
  adminKey: string,
): Promise<void> {
  const res = await fetch(`${API}/panel/galeri/${id}`, {
    method: 'DELETE',
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message ?? 'Fotoğraf silinemedi')
  }
}

export async function deleteAllUploadedGalleryPhotos(
  panelUid: string,
  adminKey: string,
): Promise<{ deletedCount: number }> {
  const res = await fetch(`${API}/panel/galeri/uploaded`, {
    method: 'DELETE',
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(err.message ?? 'Fotoğraflar silinemedi')
  }
  return res.json()
}

export async function fetchRsvpList(panelUid: string, adminKey: string): Promise<RsvpRecord[]> {
  const res = await fetch(`${API}/panel/rsvp`, {
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) throw new Error('Katılım yanıtları alınamadı')
  return res.json()
}

export async function deleteRsvp(
  id: number,
  panelUid: string,
  adminKey: string,
): Promise<void> {
  const res = await fetch(`${API}/panel/rsvp/${id}`, {
    method: 'DELETE',
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) throw new Error('Kayıt silinemedi')
}

export async function exportRsvpExcel(panelUid: string, adminKey: string): Promise<Blob> {
  const res = await fetch(`${API}/panel/rsvp/export`, {
    headers: panelHeaders(panelUid, adminKey),
  })
  if (!res.ok) throw new Error('Excel indirilemedi')
  return res.blob()
}

export async function updateDavetiye(
  data: DavetiyeAdmin,
  panelUid: string,
  adminKey: string,
): Promise<DavetiyeAdmin> {
  const res = await fetch(`${API}/panel/davetiye`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      ...panelHeaders(panelUid, adminKey),
    },
    body: JSON.stringify({
      gelinAdi: data.gelinAdi,
      damatAdi: data.damatAdi,
      basHarpler: data.basHarpler,
      baslik: data.baslik,
      hosgeldinMetni: data.hosgeldinMetni,
      etkinlikTarihi: data.etkinlikTarihi,
      mekanAdi: data.mekanAdi,
      adres: data.adres,
      haritaEmbedUrl: data.haritaEmbedUrl,
      haritaLink: data.haritaLink,
      kapakGorselUrl: data.kapakGorselUrl,
      ciftFotoUrl: data.ciftFotoUrl,
      acilisVideoUrl: data.acilisVideoUrl,
      muzikUrl: data.muzikUrl,
      zarfArkaPlanUrl: data.zarfArkaPlanUrl,
      galeriDriveKlasorUrl: data.galeriDriveKlasorUrl,
      timeline: data.timeline,
      galeri: data.galeri,
    }),
  })
  if (!res.ok) throw new Error('Davetiye güncellenemedi')
  return res.json()
}
