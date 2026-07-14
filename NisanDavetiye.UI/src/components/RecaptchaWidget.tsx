import { useEffect, useRef, useState } from 'react'
import { fetchClientConfig } from '../api/client'

declare global {
  interface Window {
    grecaptcha?: {
      ready: (cb: () => void) => void
      render: (
        container: HTMLElement,
        options: {
          sitekey: string
          callback: (token: string) => void
          'expired-callback'?: () => void
          'error-callback'?: () => void
          theme?: 'light' | 'dark'
          size?: 'normal' | 'compact'
        },
      ) => number
      reset: (widgetId?: number) => void
    }
  }
}

let recaptchaScriptPromise: Promise<void> | null = null

function loadRecaptchaScript(): Promise<void> {
  if (window.grecaptcha?.render) return Promise.resolve()
  if (recaptchaScriptPromise) return recaptchaScriptPromise

  recaptchaScriptPromise = new Promise((resolve, reject) => {
    const existing = document.querySelector<HTMLScriptElement>('script[data-recaptcha]')
    if (existing) {
      if (window.grecaptcha?.render) {
        resolve()
        return
      }
      existing.addEventListener('load', () => resolve(), { once: true })
      existing.addEventListener('error', () => reject(new Error('reCAPTCHA yüklenemedi')), {
        once: true,
      })
      return
    }

    const script = document.createElement('script')
    script.src = 'https://www.google.com/recaptcha/api.js?render=explicit&hl=tr'
    script.async = true
    script.defer = true
    script.dataset.recaptcha = 'true'
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('reCAPTCHA yüklenemedi'))
    document.head.appendChild(script)
  })

  return recaptchaScriptPromise
}

type RecaptchaWidgetProps = {
  onToken: (token: string) => void
  onExpire?: () => void
  /** Form gönderimi sonrası artırın; widget yeniden create edilmez, sadece reset edilir. */
  resetKey?: number
}

/** Google reCAPTCHA v2 ("Ben robot değilim"). */
export function RecaptchaWidget({ onToken, onExpire, resetKey = 0 }: RecaptchaWidgetProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const widgetIdRef = useRef<number | null>(null)
  const onTokenRef = useRef(onToken)
  const onExpireRef = useRef(onExpire)
  const [enabled, setEnabled] = useState<boolean | null>(null)
  const [failed, setFailed] = useState(false)

  onTokenRef.current = onToken
  onExpireRef.current = onExpire

  // Tek sefer mount — resetKey ile yeniden render etme (Google buna izin vermez).
  useEffect(() => {
    let cancelled = false

    async function mount() {
      try {
        const config = await fetchClientConfig()
        if (cancelled) return

        if (!config.recaptchaEnabled || !config.recaptchaSiteKey) {
          setEnabled(false)
          onTokenRef.current('disabled')
          return
        }

        setEnabled(true)
        await loadRecaptchaScript()
        if (cancelled || !containerRef.current || !window.grecaptcha) return

        await new Promise<void>((resolve) => {
          window.grecaptcha!.ready(() => resolve())
        })

        if (cancelled || !containerRef.current || !window.grecaptcha) return
        if (widgetIdRef.current !== null) return

        widgetIdRef.current = window.grecaptcha.render(containerRef.current, {
          sitekey: config.recaptchaSiteKey,
          theme: 'light',
          size: 'normal',
          callback: (token) => {
            setFailed(false)
            onTokenRef.current(token)
          },
          'expired-callback': () => {
            onTokenRef.current('')
            onExpireRef.current?.()
          },
          'error-callback': () => {
            // Token kullanıldıktan sonra veya ağ kesintisinde tetiklenebilir;
            // domain hatası sandığımız kalıcı UI göstermeyelim.
            onTokenRef.current('')
          },
        })
      } catch {
        if (!cancelled) {
          setFailed(true)
          onTokenRef.current('')
        }
      }
    }

    mount()

    return () => {
      cancelled = true
    }
  }, [])

  // Başarılı/başarısız gönderim sonrası yalnızca checkbox'ı sıfırla.
  useEffect(() => {
    if (resetKey === 0) return
    if (widgetIdRef.current === null || !window.grecaptcha) return

    onTokenRef.current('')
    try {
      window.grecaptcha.reset(widgetIdRef.current)
      setFailed(false)
    } catch {
      // ignore
    }
  }, [resetKey])

  if (enabled === false) return null

  if (failed) {
    return (
      <p className="recaptcha-widget__error" role="alert">
        Güvenlik doğrulaması yüklenemedi. Sayfayı yenileyip tekrar deneyin. Domain&apos;in Google
        reCAPTCHA konsolunda tanımlı olduğundan emin olun.
      </p>
    )
  }

  // enabled null iken de container hazır olsun; script gelince render edilir.
  return <div ref={containerRef} className="recaptcha-widget" />
}
