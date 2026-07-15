import { useCallback, useEffect, useRef, useState } from 'react'
import { excellenceAssets } from '../../excellence/assets'

type Phase = 'idle' | 'playing' | 'fading' | 'done'

interface Props {
  videoUrl: string
  onComplete: () => void
  /** İlk kullanıcı dokunuşunda (autoplay politikası için) çağrılır. */
  onUserGesture?: () => void
}

export function IntroOverlay({ videoUrl, onComplete, onUserGesture }: Props) {
  const videoRef = useRef<HTMLVideoElement>(null)
  const startingRef = useRef(false)
  const [phase, setPhase] = useState<Phase>('idle')

  useEffect(() => {
    document.body.style.overflow = 'hidden'
    return () => {
      document.body.style.overflow = ''
    }
  }, [])

  useEffect(() => {
    const video = videoRef.current
    if (!video) return
    video.load()
  }, [videoUrl])

  useEffect(() => {
    if (phase !== 'fading') return
    const t = setTimeout(() => {
      setPhase('done')
      onComplete()
    }, 1200)
    return () => clearTimeout(t)
  }, [phase, onComplete])

  const startPlayback = useCallback(async () => {
    if (phase !== 'idle' || startingRef.current) return
    startingRef.current = true

    // Kullanıcı jesti içinde müziği de başlat (tarayıcı autoplay kısıtı).
    onUserGesture?.()

    const video = videoRef.current
    if (!video) {
      startingRef.current = false
      return
    }

    video.currentTime = 0
    video.muted = true

    try {
      await video.play()
      setPhase('playing')
    } catch {
      setPhase('fading')
    } finally {
      startingRef.current = false
    }
  }, [phase, onUserGesture])

  if (phase === 'done') return null

  return (
    <div
      className={`ex-intro ${phase === 'playing' ? 'ex-intro--playing' : ''} ${phase === 'fading' ? 'ex-intro--fading' : ''}`}
    >
      <video
        ref={videoRef}
        src={videoUrl}
        poster={phase === 'idle' ? excellenceAssets.introPoster : undefined}
        muted
        playsInline
        preload="auto"
        className="ex-intro__video"
        onEnded={() => setPhase('fading')}
      />
      {phase === 'idle' && (
        <button
          type="button"
          className="ex-intro__tap"
          onPointerDown={(e) => {
            e.preventDefault()
            void startPlayback()
          }}
          onClick={(e) => {
            e.preventDefault()
            void startPlayback()
          }}
          aria-label="Zarfa dokunun, videoyu başlat"
        >
          <span className="ex-intro__hint-text">Zarfa dokunun</span>
        </button>
      )}
    </div>
  )
}
