import { ImageSquareIcon, TrashIcon, UploadIcon } from '@phosphor-icons/react'
import { useCallback, useEffect, useRef, useState } from 'react'

import { Button } from '../ui/button'

const MAX_FILE_SIZE = 5 * 1024 * 1024 // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

export function VoucherDefinitionBannerField({ 
  file, 
  onChange, 
  onClear, 
  error,
  disabled,
  t
}) {
  const [previewUrl, setPreviewUrl] = useState(null)
  const [localError, setLocalError] = useState('')
  const fileInputRef = useRef(null)

  useEffect(() => {
    if (file) {
      const url = URL.createObjectURL(file)
      setPreviewUrl(url)
      return () => URL.revokeObjectURL(url)
    } else {
      setPreviewUrl(null)
    }
  }, [file])

  const handleFileChange = useCallback((e) => {
    const selectedFile = e.target.files?.[0]
    setLocalError('')

    if (!selectedFile) {
      onChange(null)
      return
    }

    if (!ALLOWED_TYPES.includes(selectedFile.type)) {
      setLocalError(t('voucherDefinitions.form.bannerTypeInvalid'))
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
      return
    }

    if (selectedFile.size > MAX_FILE_SIZE) {
      setLocalError(t('voucherDefinitions.form.bannerSizeInvalid'))
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
      return
    }

    onChange(selectedFile)
  }, [onChange, t])

  const handleClear = useCallback(() => {
    setLocalError('')
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
    onClear()
  }, [onClear])

  const displayError = error || localError

  return (
    <div className="space-y-3">
      <input
        type="file"
        ref={fileInputRef}
        accept={ALLOWED_TYPES.join(',')}
        onChange={handleFileChange}
        className="hidden"
        disabled={disabled}
      />

      {previewUrl ? (
        <div className="relative overflow-hidden rounded-lg border bg-muted/30">
          <img 
            src={previewUrl} 
            alt="Banner preview" 
            className="w-full h-[200px] object-cover" 
          />
          <div className="absolute top-2 right-2">
            <Button
              type="button"
              variant="destructive"
              size="icon"
              className="h-8 w-8 rounded-full opacity-90 shadow-sm hover:opacity-100"
              onClick={handleClear}
              disabled={disabled}
              title={t('common.clear')}
            >
              <TrashIcon size={16} />
            </Button>
          </div>
        </div>
      ) : (
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled}
          className="flex w-full flex-col items-center justify-center gap-2 rounded-lg border border-dashed py-8 text-muted-foreground transition-colors hover:border-primary/50 hover:bg-muted/30 hover:text-foreground disabled:pointer-events-none disabled:opacity-50"
        >
          <div className="rounded-full bg-muted p-3">
            {disabled ? <ImageSquareIcon size={24} /> : <UploadIcon size={24} />}
          </div>
          <div className="text-center">
            <p className="text-sm font-medium">
              {disabled 
                ? t('voucherDefinitions.form.bannerUploadDisabled') 
                : t('voucherDefinitions.form.bannerUploadPrompt')}
            </p>
            <p className="mt-1 text-[13px] opacity-70">
              {t('voucherDefinitions.form.bannerRequirements')}
            </p>
          </div>
        </button>
      )}

      {displayError ? (
        <p className="text-[13px] font-medium text-destructive">{displayError}</p>
      ) : null}
    </div>
  )
}
