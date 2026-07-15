import { type ChangeEvent, type FormEvent, useRef, useState } from 'react'
import { uploadGalleryPhotos } from '../api/client'
import { useInviteKey } from '../context/InviteContext'
import { compressImagesForUpload } from '../utils/imageCompress'
import { excellenceAssets } from '../excellence/assets'
import { RecaptchaWidget } from './RecaptchaWidget'

const MAX_FILES = 10

const UPLOAD_LOCKED_MESSAGE =
  'Fotoğraf yükleme şu anda kapalı. Tören sırasında/sonrasında açılacaktır.'

const UPLOAD_SUCCESS_MESSAGE = 'Fotoğraflarınız başarıyla yüklendi. Teşekkür ederiz!'

type GalleryUploadProps = {
  onUploaded?: () => void
  /** Panelden yönetilen yükleme anahtarı; kapalıysa kilitli görünüm gösterilir. */
  uploadOpen?: boolean
}

export function GalleryUpload({ onUploaded, uploadOpen = true }: GalleryUploadProps) {
  const inviteKey = useInviteKey()
  const inputRef = useRef<HTMLInputElement>(null)
  const [files, setFiles] = useState<File[]>([])
  const [status, setStatus] = useState<'idle' | 'preparing' | 'loading' | 'success' | 'error'>('idle')
  const [message, setMessage] = useState('')
  const [captchaToken, setCaptchaToken] = useState('')
  const [captchaResetKey, setCaptchaResetKey] = useState(0)

  const busy = status === 'preparing' || status === 'loading'

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    if (!uploadOpen) {
      if (inputRef.current) inputRef.current.value = ''
      return
    }

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
    if (!uploadOpen) {
      setStatus('error')
      setMessage(UPLOAD_LOCKED_MESSAGE)
      return
    }
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
      await uploadGalleryPhotos(inviteKey, prepared, captchaToken)
      setStatus('success')
      setMessage(UPLOAD_SUCCESS_MESSAGE)
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

  if (!uploadOpen) {
    return (
      <div className="ex-gallery__upload gallery__upload gallery__upload--locked">
        <p className="gallery__upload-lead">{UPLOAD_LOCKED_MESSAGE}</p>
      </div>
    )
  }

  return (
    <form className="ex-gallery__upload gallery__upload" onSubmit={handleSubmit}>
      <p className="gallery__upload-lead">
        Törende çektiğiniz anıları bizimle paylaşın. Tek seferde en fazla {MAX_FILES} fotoğraf
        yükleyebilirsiniz.
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

      {busy && (
        <div className="upload-modal" role="alertdialog" aria-busy="true" aria-live="assertive">
          <div className="upload-modal__card">
            <img
              src={excellenceAssets.monogram}
              alt=""
              className="upload-modal__monogram"
              onError={(e) => {
                e.currentTarget.style.display = 'none'
              }}
            />
            <div className="upload-modal__spinner" aria-hidden />
            <p className="upload-modal__title">Fotoğraflarınız yükleniyor</p>
            <p className="upload-modal__text">
              Lütfen bekleyin, bu işlem birkaç saniye sürebilir. Sayfayı kapatmayın.
            </p>
          </div>
        </div>
      )}
    </form>
  )
}
