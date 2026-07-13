export interface TimelineItem {
  id: number
  baslik: string
  aciklama: string
  saat: string
  sira: number
}

export interface GaleriItem {
  id: number
  url: string
  altMetin: string
  sira: number
  onaylandi: boolean
  misafirYuklemesi: boolean
}

export interface Davetiye {
  gelinAdi: string
  damatAdi: string
  basHarpler: string
  baslik: string
  hosgeldinMetni: string
  etkinlikTarihi: string
  mekanAdi: string
  adres: string
  haritaEmbedUrl: string
  haritaLink: string
  kapakGorselUrl: string
  ciftFotoUrl: string
  acilisVideoUrl: string
  muzikUrl: string
  zarfArkaPlanUrl: string
  galeriDriveKlasorUrl: string
  timeline: TimelineItem[]
  galeri: GaleriItem[]
}

export interface DavetiyeAdmin extends Davetiye {
  davetUid: string
  panelUid: string
}

export interface RsvpInput {
  adSoyad: string
  telefon: string
  katilacak: boolean
  kisiSayisi: number
  mesaj: string
  captchaToken: string
}

export interface RsvpRecord extends RsvpInput {
  id: number
  olusturmaTarihi: string
}
