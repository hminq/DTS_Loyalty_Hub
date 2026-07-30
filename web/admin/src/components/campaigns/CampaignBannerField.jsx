import {
  ImageSquareIcon,
  TrashIcon,
  UploadIcon,
} from '@phosphor-icons/react'
import { useCallback, useEffect, useRef, useState } from 'react'

import { Button } from '../ui/button'

const MAX_FILE_SIZE = 5 * 1024 * 1024
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

export function CampaignBannerField({
  file,
  existingUrl = '',
  onChange,
  onClear,
  error,
  disabled = false,
  t,
}) {
  const [previewUrl, setPreviewUrl] = useState(null)
  const [localError, setLocalError] = useState('')
  const fileInputRef = useRef(null)

  useEffect(() => {
    if (!file) {
      setPreviewUrl(null)
      return undefined
    }

    const url = URL.createObjectURL(file)
    setPreviewUrl(url)
    return () => URL.revokeObjectURL(url)
  }, [file])

  const handleFileChange = useCallback((event) => {
    const selectedFile = event.target.files?.[0]
    setLocalError('')

    if (!selectedFile) {
      onChange(null)
      return
    }

    if (!ALLOWED_TYPES.includes(selectedFile.type)) {
      setLocalError(t('campaigns.form.bannerTypeInvalid'))
      event.target.value = ''
      return
    }

    if (selectedFile.size <= 0 || selectedFile.size > MAX_FILE_SIZE) {
      setLocalError(t('campaigns.form.bannerSizeInvalid'))
      event.target.value = ''
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
    <div
      className="flex flex-col gap-3"
      data-invalid={displayError ? true : undefined}
      data-disabled={disabled ? true : undefined}
    >
      <input
        ref={fileInputRef}
        type="file"
        accept={ALLOWED_TYPES.join(',')}
        onChange={handleFileChange}
        className="hidden"
        disabled={disabled}
        aria-invalid={Boolean(displayError)}
      />

      {previewUrl || existingUrl ? (
        <div className="flex flex-col gap-3">
          <div className="relative overflow-hidden rounded-lg border bg-muted/30">
            <img
              src={previewUrl || existingUrl}
              alt={t('campaigns.form.bannerPreviewAlt')}
              className="h-[200px] w-full object-cover"
            />
            <div className="absolute right-2 top-2">
              <Button
                type="button"
                variant="destructive"
                size="icon"
                className="rounded-full opacity-90 shadow-sm hover:opacity-100"
                onClick={handleClear}
                disabled={disabled}
                title={t('common.clear')}
                aria-label={t('common.clear')}
              >
                <TrashIcon aria-hidden="true" />
              </Button>
            </div>
          </div>

          {previewUrl ? (
            <p className="text-xs text-muted-foreground">
              {t('campaigns.form.bannerSelected')}
            </p>
          ) : null}
        </div>
      ) : (
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled}
          className="flex w-full flex-col items-center justify-center gap-2 rounded-lg border border-dashed py-8 text-muted-foreground transition-colors hover:border-primary/50 hover:bg-muted/30 hover:text-foreground disabled:pointer-events-none disabled:opacity-50"
        >
          <div className="rounded-full bg-muted p-3">
            {disabled
              ? <ImageSquareIcon aria-hidden="true" />
              : <UploadIcon aria-hidden="true" />}
          </div>
          <div className="text-center">
            <p className="text-sm font-medium">
              {t(disabled
                ? 'campaigns.form.bannerUploadDisabled'
                : 'campaigns.form.bannerUploadPrompt')}
            </p>
            <p className="mt-1 text-[13px] opacity-70">
              {t('campaigns.form.bannerRequirements')}
            </p>
          </div>
        </button>
      )}

      {displayError ? (
        <p className="text-[13px] font-medium text-destructive">
          {displayError}
        </p>
      ) : null}
    </div>
  )
}
