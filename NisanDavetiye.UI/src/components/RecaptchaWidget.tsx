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

  useEffect(() => {
    let cancelled = false
    setFailed(false)

    fetchClientConfig()
      .then((config) => {
        if (cancelled) return

        if (!config.recaptchaEnabled || !config.recaptchaSiteKey) {
          setEnabled(false)
          onTokenRef.current('disabled')
          return
        }

        setEnabled(true)
      })
      .catch(() => {
        if (!cancelled) {
          setEnabled(false)
          onTokenRef.current('disabled')
        }
      })

    return () => {
      cancelled = true
    }
  }, [resetKey])

  useEffect(() => {
    if (enabled !== true) return

    let cancelled = false

    async function mount() {
      const config = await fetchClientConfig()
      if (cancelled || !config.recaptchaSiteKey) return

      try {
        await loadRecaptchaScript()
      } catch {
        if (!cancelled) {
          setFailed(true)
          onTokenRef.current('')
        }
        return
      }

      if (cancelled || !containerRef.current || !window.grecaptcha) return

      await new Promise<void>((resolve) => {
        window.grecaptcha!.ready(() => resolve())
      })

      if (cancelled || !containerRef.current || !window.grecaptcha) return

      // Önceki widget varsa container'ı temizle.
      containerRef.current.innerHTML = ''
      widgetIdRef.current = null

      try {
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
            setFailed(true)
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
      widgetIdRef.current = null
      if (containerRef.current) containerRef.current.innerHTML = ''
    }
  }, [enabled, resetKey])

  // Form yeniden gönderildiğinde / reset sonrası checkbox'ı sıfırla.
  useEffect(() => {
    if (enabled !== true || widgetIdRef.current === null || !window.grecaptcha) return
    if (resetKey === 0) return
    try {
      window.grecaptcha.reset(widgetIdRef.current)
    } catch {
      // ignore
    }
  }, [resetKey, enabled])

  if (enabled !== true) return null

  if (failed) {
    return (
      <p className="recaptcha-widget__error" role="alert">
        Güvenlik doğrulaması yüklenemedi. Domain&apos;in Google reCAPTCHA konsolunda tanımlı
        olduğundan emin olun (`cerenemre.up.railway.app`).
      </p>
    )
  }

  return <div ref={containerRef} className="recaptcha-widget" />
}
