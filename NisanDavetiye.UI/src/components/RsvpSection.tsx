import { type ChangeEvent, type FormEvent, useState } from 'react'
import { submitRsvp } from '../api/client'
import { useInviteKey } from '../context/InviteContext'
import { excellenceAssets } from '../excellence/assets'
import { FadeIn } from './excellence/FadeIn'
import { ExcellenceSectionHeader } from './excellence/SectionHeader'
import { TurnstileWidget } from './TurnstileWidget'

const PHONE_PATTERN = /^0\d{10}$/

function formatPhoneInput(value: string): string {
  return value.replace(/\D/g, '').slice(0, 11)
}

export function RsvpSection() {
  const inviteKey = useInviteKey()
  const [adSoyad, setAdSoyad] = useState('')
  const [telefon, setTelefon] = useState('')
  const [katilacak, setKatilacak] = useState<boolean | null>(null)
  const [kisiSayisi, setKisiSayisi] = useState(1)
  const [mesaj, setMesaj] = useState('')
  const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle')
  const [error, setError] = useState('')
  const [captchaToken, setCaptchaToken] = useState('')
  const [captchaResetKey, setCaptchaResetKey] = useState(0)

  const handleTelefonChange = (e: ChangeEvent<HTMLInputElement>) => {
    setTelefon(formatPhoneInput(e.target.value))
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()

    if (katilacak === null) {
      setError('Lütfen katılım durumunuzu seçin.')
      return
    }

    if (!adSoyad.trim()) {
      setError('Ad soyad zorunludur.')
      return
    }

    if (!PHONE_PATTERN.test(telefon)) {
      setError('Telefon numarası 0 ile başlamalı ve 11 haneli olmalıdır.')
      return
    }

    if (!captchaToken) {
      setError('Lütfen güvenlik doğrulamasını tamamlayın.')
      return
    }

    setStatus('loading')
    setError('')
    try {
      await submitRsvp(inviteKey, {
        adSoyad: adSoyad.trim(),
        telefon,
        katilacak,
        kisiSayisi,
        mesaj,
        captchaToken,
      })
      setStatus('success')
      setAdSoyad('')
      setTelefon('')
      setMesaj('')
      setKisiSayisi(1)
      setKatilacak(null)
      setCaptchaToken('')
      setCaptchaResetKey((k) => k + 1)
    } catch (err) {
      setStatus('error')
      setError(err instanceof Error ? err.message : 'Bir hata oluştu')
      setCaptchaToken('')
      setCaptchaResetKey((k) => k + 1)
    }
  }

  return (
    <section id="rsvp" className="ex-section ex-rsvp">
      <div className="ex-rsvp__wrap">
        <FadeIn>
          <ExcellenceSectionHeader
            title="Katılım Bildirimi"
            subtitle="Umarız gelebilirsiniz"
          />
        </FadeIn>

        <FadeIn delay={0.1}>
          <form className="ex-rsvp__form" onSubmit={handleSubmit}>
            <fieldset className="ex-rsvp__attend">
              <legend>Katılacak mısınız? *</legend>
              <div className="ex-rsvp__attend-btns">
                <button
                  type="button"
                  className={`ex-rsvp__choice ${katilacak === true ? 'ex-rsvp__choice--active' : ''}`}
                  onClick={() => setKatilacak(true)}
                >
                  Evet, geleceğim
                </button>
                <button
                  type="button"
                  className={`ex-rsvp__choice ${katilacak === false ? 'ex-rsvp__choice--active' : ''}`}
                  onClick={() => setKatilacak(false)}
                >
                  Maalesef gelemeyeceğim
                </button>
              </div>
            </fieldset>

            <label>
              Ad Soyad *
              <input
                value={adSoyad}
                onChange={(e) => setAdSoyad(e.target.value)}
                required
                placeholder="Adınız Soyadınız"
                autoComplete="name"
              />
            </label>

            <label>
              Telefon *
              <input
                value={telefon}
                onChange={handleTelefonChange}
                required
                inputMode="numeric"
                autoComplete="tel"
                maxLength={11}
                pattern="0[0-9]{10}"
                placeholder="05xxxxxxxxx"
                title="0 ile başlayan 11 haneli numara"
              />
            </label>

            {katilacak && (
              <label>
                Kişi sayısı
                <input
                  type="number"
                  min={1}
                  max={10}
                  value={kisiSayisi}
                  onChange={(e) => setKisiSayisi(Number(e.target.value))}
                />
              </label>
            )}

            <label>
              Mesajınız (isteğe bağlı)
              <textarea
                value={mesaj}
                onChange={(e) => setMesaj(e.target.value)}
                rows={4}
                placeholder="Bize birkaç söz yazın..."
              />
            </label>

            <div className="ex-rsvp__submit-wrap">
              <TurnstileWidget
                resetKey={captchaResetKey}
                onToken={setCaptchaToken}
                onExpire={() => setCaptchaToken('')}
              />
              <img
                src={excellenceAssets.vaseLeft}
                alt=""
                className="ex-rsvp__vase ex-rsvp__vase--left ornament-img"
                aria-hidden
              />
              <img
                src={excellenceAssets.vaseRight}
                alt=""
                className="ex-rsvp__vase ex-rsvp__vase--right ornament-img"
                aria-hidden
              />
              <button type="submit" className="ex-btn ex-btn--primary ex-btn--full" disabled={status === 'loading'}>
                {status === 'loading' ? 'Gönderiliyor…' : 'Yanıtımı Gönder'}
              </button>
            </div>

            {status === 'success' && (
              <p className="ex-rsvp__success">Teşekkürler! Yanıtınız alındı.</p>
            )}
            {status === 'error' && <p className="ex-rsvp__error">{error}</p>}
          </form>
        </FadeIn>
      </div>
    </section>
  )
}
