import { useEffect, useImperativeHandle, useRef, useState, forwardRef } from 'react'

export interface MusicPlayerHandle {
  start: () => void
}

interface Props {
  url: string
  /** Müzik düğmesini göster (ses her zaman mount edilir). */
  showButton?: boolean
}

export const MusicPlayer = forwardRef<MusicPlayerHandle, Props>(function MusicPlayer(
  { url, showButton = true },
  ref,
) {
  const audioRef = useRef<HTMLAudioElement>(null)
  const [playing, setPlaying] = useState(false)
  const [muted, setMuted] = useState(false)

  useImperativeHandle(ref, () => ({
    start: () => {
      const audio = audioRef.current
      if (!audio || !url) return
      audio.volume = 0.5
      void audio
        .play()
        .then(() => {
          setPlaying(true)
          setMuted(false)
        })
        .catch(() => {
          setPlaying(false)
        })
    },
  }))

  const toggleMute = () => {
    const audio = audioRef.current
    if (!audio) return
    if (muted || audio.paused) {
      audio.volume = 0.5
      void audio.play().catch(() => undefined)
      setMuted(false)
      setPlaying(true)
    } else {
      audio.pause()
      setMuted(true)
      setPlaying(false)
    }
  }

  useEffect(() => {
    const audio = audioRef.current
    if (!audio) return
    const onEnded = () => setPlaying(false)
    const onPlay = () => setPlaying(true)
    const onPause = () => setPlaying(false)
    audio.addEventListener('ended', onEnded)
    audio.addEventListener('play', onPlay)
    audio.addEventListener('pause', onPause)
    return () => {
      audio.removeEventListener('ended', onEnded)
      audio.removeEventListener('play', onPlay)
      audio.removeEventListener('pause', onPause)
    }
  }, [])

  if (!url) return null

  return (
    <>
      <audio ref={audioRef} src={url} loop preload="auto" playsInline />
      {showButton && (
        <button
          type="button"
          className={`ex-music-btn ${playing && !muted ? 'ex-music-btn--on' : ''}`}
          onClick={toggleMute}
          aria-label={playing && !muted ? 'Müziği kapat' : 'Müziği aç'}
        >
          {playing && !muted ? '♫' : '♪'}
        </button>
      )}
    </>
  )
})
