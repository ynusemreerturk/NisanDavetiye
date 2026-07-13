import { useEffect, useRef } from 'react'
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

export function TurnstileWidget({ onToken, onExpire, resetKey = 0 }: TurnstileWidgetProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const widgetIdRef = useRef<string | null>(null)
  const onTokenRef = useRef(onToken)
  const onExpireRef = useRef(onExpire)

  onTokenRef.current = onToken
  onExpireRef.current = onExpire

  useEffect(() => {
    let cancelled = false

    async function mount() {
      const config = await fetchClientConfig()
      if (cancelled) return

      if (!config.turnstileEnabled || !config.turnstileSiteKey) {
        onTokenRef.current('disabled')
        return
      }

      await loadTurnstileScript()
      if (cancelled || !containerRef.current || !window.turnstile) return

      if (widgetIdRef.current) {
        window.turnstile.remove(widgetIdRef.current)
        widgetIdRef.current = null
      }

      widgetIdRef.current = window.turnstile.render(containerRef.current, {
        sitekey: config.turnstileSiteKey,
        callback: (token) => onTokenRef.current(token),
        'expired-callback': () => {
          onTokenRef.current('')
          onExpireRef.current?.()
        },
      })
    }

    mount().catch(() => onTokenRef.current(''))

    return () => {
      cancelled = true
      if (widgetIdRef.current && window.turnstile) {
        window.turnstile.remove(widgetIdRef.current)
        widgetIdRef.current = null
      }
    }
  }, [resetKey])

  return <div ref={containerRef} className="turnstile-widget" />
}
