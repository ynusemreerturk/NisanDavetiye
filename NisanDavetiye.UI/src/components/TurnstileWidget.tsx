import { useEffect, useRef, useState } from 'react'
import { fetchClientConfig } from '../api/client'

declare global {
  interface Window {
    turnstile?: {
      render: (
        element: HTMLElement,
        options: {
          sitekey: string
          callback: (token: string) => void
          'expired-callback'?: () => void
          'error-callback'?: () => void
          theme?: 'light' | 'dark' | 'auto'
          size?: 'normal' | 'compact' | 'flexible'
        },
      ) => string
      remove: (widgetId: string) => void
    }
  }
}

let turnstileScriptPromise: Promise<void> | null = null

function loadTurnstileScript(): Promise<void> {
  if (window.turnstile) return Promise.resolve()
  if (turnstileScriptPromise) return turnstileScriptPromise

  turnstileScriptPromise = new Promise((resolve, reject) => {
    const existing = document.querySelector<HTMLScriptElement>('script[data-turnstile]')
    if (existing) {
      existing.addEventListener('load', () => resolve(), { once: true })
      existing.addEventListener('error', () => reject(new Error('Turnstile yüklenemedi')), { once: true })
      return
    }

    const script = document.createElement('script')
    script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit'
    script.async = true
    script.defer = true
    script.dataset.turnstile = 'true'
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('Turnstile yüklenemedi'))
    document.head.appendChild(script)
  })

  return turnstileScriptPromise
}

type TurnstileWidgetProps = {
  onToken: (token: string) => void
  onExpire?: () => void
  resetKey?: number
}

/**
 * Cloudflare Turnstile. Yanlış domain / geçersiz key olduğunda Cloudflare
 * "Troubleshoot" gösterir ve boş modal açar — bunu yakalayıp kendi mesajımızı gösteririz.
 */
export function TurnstileWidget({ onToken, onExpire, resetKey = 0 }: TurnstileWidgetProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const widgetIdRef = useRef<string | null>(null)
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

        if (!config.turnstileEnabled || !config.turnstileSiteKey) {
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
      if (cancelled || !config.turnstileSiteKey) return

      try {
        await loadTurnstileScript()
      } catch {
        if (!cancelled) {
          setFailed(true)
          onTokenRef.current('')
        }
        return
      }

      if (cancelled || !containerRef.current || !window.turnstile) return

      if (widgetIdRef.current) {
        window.turnstile.remove(widgetIdRef.current)
        widgetIdRef.current = null
      }

      // Cloudflare'un kırık "Troubleshoot" arayüzünü temizleyip kendi hata durumumuza geç.
      const fail = () => {
        if (widgetIdRef.current && window.turnstile) {
          window.turnstile.remove(widgetIdRef.current)
          widgetIdRef.current = null
        }
        if (containerRef.current) containerRef.current.innerHTML = ''
        setFailed(true)
        onTokenRef.current('')
      }

      try {
        widgetIdRef.current = window.turnstile.render(containerRef.current, {
          sitekey: config.turnstileSiteKey,
          theme: 'light',
          size: 'flexible',
          callback: (token) => {
            setFailed(false)
            onTokenRef.current(token)
          },
          'expired-callback': () => {
            onTokenRef.current('')
            onExpireRef.current?.()
          },
          'error-callback': fail,
        })
      } catch {
        fail()
      }
    }

    mount()

    return () => {
      cancelled = true
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.remove(widgetIdRef.current)
        widgetIdRef.current = null
      }
    }
  }, [enabled, resetKey])

  if (enabled !== true) return null

  if (failed) {
    return (
      <p className="turnstile-widget__error" role="alert">
        Güvenlik doğrulaması bu alanda yüklenemedi. Sayfayı yenileyin; sorun sürerse yöneticiye
        Turnstile domain ayarını bildirın.
      </p>
    )
  }

  return <div ref={containerRef} className="turnstile-widget" />
}
