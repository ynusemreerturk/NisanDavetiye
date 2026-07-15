import { motion } from 'framer-motion'
import { useEffect, useRef, useState } from 'react'
import type { Davetiye } from '../../types'

interface Props {
  data: Davetiye
  showContent: boolean
  playVideo: boolean
  scrollLocked: boolean
  onUnlockScroll: () => void
}

export function HeroSection({
  data,
  showContent,
  playVideo,
  scrollLocked,
  onUnlockScroll,
}: Props) {
  const videoRef = useRef<HTMLVideoElement>(null)
  const startedRef = useRef(false)
  const endedRef = useRef(false)
  const onUnlockRef = useRef(onUnlockScroll)
  const [showCta, setShowCta] = useState(false)

  onUnlockRef.current = onUnlockScroll

  const isVideo = data.kapakGorselUrl.toLowerCase().endsWith('.mp4')

  // Intro oynarken hero videosunu önceden buffer'la; ilk kareyi boya.
  useEffect(() => {
    if (!isVideo || playVideo) return
    const video = videoRef.current
    if (!video) return

    const paintFirstFrame = () => {
      try {
        if (video.currentTime < 0.01) video.currentTime = 0.001
      } catch {
        // ignore
      }
    }

    if (video.readyState >= 2) paintFirstFrame()
    else video.addEventListener('loadeddata', paintFirstFrame, { once: true })

    return () => video.removeEventListener('loadeddata', paintFirstFrame)
  }, [isVideo, playVideo, data.kapakGorselUrl])

  useEffect(() => {
    if (!playVideo) return

    if (!isVideo) {
      setShowCta(true)
      onUnlockRef.current()
      return
    }

    const video = videoRef.current
    if (!video || endedRef.current) return

    const finish = () => {
      if (endedRef.current) return
      endedRef.current = true
      if (Number.isFinite(video.duration) && video.duration > 0) {
        video.currentTime = Math.max(0, video.duration - 0.04)
      }
      video.pause()
      setShowCta(true)
      onUnlockRef.current()
    }

    const onEnded = () => finish()

    if (!startedRef.current) {
      startedRef.current = true
      video.currentTime = 0
      video.play().catch(() => finish())
    }

    video.addEventListener('ended', onEnded)
    return () => video.removeEventListener('ended', onEnded)
  }, [playVideo, isVideo])

  useEffect(() => {
    document.body.style.overflow = scrollLocked ? 'hidden' : ''
    return () => {
      if (!scrollLocked) document.body.style.overflow = ''
    }
  }, [scrollLocked])

  const scrollToCountdown = () => {
    document.getElementById('countdown')?.scrollIntoView({ behavior: 'smooth' })
  }

  return (
    <header className="ex-hero">
      {isVideo ? (
        <video
          ref={videoRef}
          src={data.kapakGorselUrl}
          muted
          playsInline
          preload="auto"
          className="ex-hero__media"
        />
      ) : (
        <div
          className="ex-hero__media ex-hero__media--image"
          style={{ backgroundImage: `url(${data.kapakGorselUrl})` }}
        />
      )}
      <div className={`ex-hero__overlay ${showContent ? '' : 'ex-hero__overlay--light'}`} />
      <div className="ex-hero__content">
        {showContent && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 1.2, delay: 0.2 }}
          >
            <p className="ex-hero__eyebrow">{data.baslik}</p>
            <h1 className="ex-hero__names">
              <span>{data.gelinAdi}</span>
              <span className="ex-hero__amp">&</span>
              <span>{data.damatAdi}</span>
            </h1>
            <p className="ex-hero__date">
              {new Date(data.etkinlikTarihi)
                .toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' })
                .toLocaleUpperCase('tr-TR')}
            </p>
          </motion.div>
        )}
        {showCta && (
          <motion.button
            type="button"
            className="ex-hero__cta"
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            onClick={scrollToCountdown}
          >
            Aşağı kaydırın ve yanıtlayın
            <span className="ex-hero__cta-chevron" aria-hidden>
              ↓
            </span>
          </motion.button>
        )}
      </div>
    </header>
  )
}
