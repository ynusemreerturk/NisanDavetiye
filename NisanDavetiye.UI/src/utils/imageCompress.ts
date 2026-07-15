const MAX_EDGE = 1600
const JPEG_QUALITY = 0.8
/** Bu boyuttan küçük dosyaları yeniden işlemeye gerek yok. */
const SKIP_UNDER_BYTES = 450_000

/**
 * Telefon fotoğraflarını (çoğunlukla 5–15 MB) yüklemeden önce
 * yeniden boyutlandırıp JPEG'e çevirir. HEIC/HEIF tarayıcıda
 * decode edilemediği için olduğu gibi bırakılır.
 */
export async function compressImagesForUpload(files: File[]): Promise<File[]> {
  const result: File[] = []
  for (const file of files) {
    result.push(await compressOne(file))
  }
  return result
}

async function compressOne(file: File): Promise<File> {
  const type = file.type.toLowerCase()
  if (type.includes('heic') || type.includes('heif'))
    return file

  if (file.size <= SKIP_UNDER_BYTES && (type === 'image/jpeg' || type === 'image/webp'))
    return file

  try {
    const bitmap = await createImageBitmap(file)
    try {
      const { width, height } = fitWithin(bitmap.width, bitmap.height, MAX_EDGE)
      const canvas = document.createElement('canvas')
      canvas.width = width
      canvas.height = height
      const ctx = canvas.getContext('2d')
      if (!ctx) return file

      ctx.drawImage(bitmap, 0, 0, width, height)

      const blob = await new Promise<Blob | null>((resolve) =>
        canvas.toBlob(resolve, 'image/jpeg', JPEG_QUALITY),
      )

      if (!blob || blob.size <= 0) return file

      // Sıkıştırma büyüttüyse (nadir) orijinali kullan.
      if (blob.size >= file.size) return file

      const baseName = file.name.replace(/\.[^.]+$/, '') || 'foto'
      return new File([blob], `${baseName}.jpg`, {
        type: 'image/jpeg',
        lastModified: Date.now(),
      })
    } finally {
      bitmap.close()
    }
  } catch {
    return file
  }
}

function fitWithin(w: number, h: number, maxEdge: number): { width: number; height: number } {
  if (w <= maxEdge && h <= maxEdge) return { width: w, height: h }
  const scale = maxEdge / Math.max(w, h)
  return {
    width: Math.max(1, Math.round(w * scale)),
    height: Math.max(1, Math.round(h * scale)),
  }
}
