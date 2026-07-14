import { type ChangeEvent, type FormEvent, useRef, useState } from 'react'
import { uploadGalleryPhotos } from '../api/client'
import { useInviteKey } from '../context/InviteContext'
import { compressImagesForUpload } from '../utils/imageCompress'
import { RecaptchaWidget } from './RecaptchaWidget'

const MAX_FILES = 10

type GalleryUploadProps = {
  onUploaded?: () => void
}

export function GalleryUpload({ onUploaded }: GalleryUploadProps) {
  const inviteKey = useInviteKey()
  const inputRef = useRef<HTMLInputElement>(null)
  const [files, setFiles] = useState<File[]>([])
  const [status, setStatus] = useState<'idle' | 'preparing' | 'loading' | 'success' | 'error'>('idle')
  const [message, setMessage] = useState('')
  const [captchaToken, setCaptchaToken] = useState('')
  const [captchaResetKey, setCaptchaResetKey] = useState(0)

  const busy = status === 'preparing' || status === 'loading'

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const selected = Array.from(e.target.files ?? [])

    if (selected.length > MAX_FILES) {
      setFiles([])
      setStatus('error')
      setMessage(`Tek seferde en fazla ${MAX_FILES} fotoğraf yükleyebilirsiniz.`)
      if (inputRef.current) inputRef.current.value = ''
      return
    }

    setFiles(selected)
    setStatus('idle')
    setMessage('')
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (files.length === 0) return

    if (files.length > MAX_FILES) {
      setStatus('error')
      setMessage(`Tek seferde en fazla ${MAX_FILES} fotoğraf yükleyebilirsiniz.`)
      return
    }

    if (!captchaToken) {
      setStatus('error')
      setMessage('Lütfen güvenlik doğrulamasını tamamlayın.')
      return
    }

    setStatus('preparing')
    setMessage('')
    try {
      const prepared = await compressImagesForUpload(files)
      setStatus('loading')
      const result = await uploadGalleryPhotos(inviteKey, prepared, captchaToken)
      setStatus('success')
      setMessage(result.message)
      setFiles([])
      setCaptchaToken('')
      setCaptchaResetKey((k) => k + 1)
      if (inputRef.current) inputRef.current.value = ''
      onUploaded?.()
    } catch (err) {
      setStatus('error')
      setMessage(err instanceof Error ? err.message : 'Yükleme başarısız oldu.')
      setCaptchaToken('')
      setCaptchaResetKey((k) => k + 1)
    }
  }

  const submitLabel =
    status === 'preparing'
      ? 'Hazırlanıyor…'
      : status === 'loading'
        ? 'Yükleniyor…'
        : 'Fotoğrafları Yükle'

  return (
    <form className="ex-gallery__upload gallery__upload" onSubmit={handleSubmit}>
      <p className="gallery__upload-lead">
        Törende çektiğiniz anıları bizimle paylaşın — fotoğraflar onay sonrası galeride
        yayınlanır. Tek seferde en fazla {MAX_FILES} fotoğraf yükleyebilirsiniz.
      </p>

      <label className="gallery__upload-picker">
        <span className="ex-btn ex-btn--outline">Fotoğraf Seç</span>
        <input
          ref={inputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp,image/heic,image/heif"
          multiple
          onChange={handleFileChange}
        />
      </label>

      {files.length > 0 && status !== 'error' && (
        <p className="gallery__upload-count">
          {files.length} / {MAX_FILES} fotoğraf seçildi
        </p>
      )}

      <RecaptchaWidget
        resetKey={captchaResetKey}
        onToken={setCaptchaToken}
        onExpire={() => setCaptchaToken('')}
      />

      <button
        type="submit"
        className="ex-btn ex-btn--primary gallery__upload-submit"
        disabled={files.length === 0 || busy}
      >
        {submitLabel}
      </button>

      {status === 'success' && <p className="gallery__upload-success">{message}</p>}
      {status === 'error' && <p className="gallery__upload-error">{message}</p>}
    </form>
  )
}
