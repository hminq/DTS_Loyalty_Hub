import { CircleNotchIcon, FileCsvIcon } from '@phosphor-icons/react'
import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { Button } from '../ui/button'
import { Input } from '../ui/input'

const MAXIMUM_IMPORT_FILE_SIZE_BYTES = 256 * 1024 * 1024

function VoucherPoolImportDialog({ open, onClose, onImport }) {
  const { t } = useTranslation()
  const dialogRef = useRef(null)
  const [file, setFile] = useState(null)
  const [isUploading, setIsUploading] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return

    if (open && !dialog.open) {
      setFile(null)
      setErrorMessage('')
      setIsUploading(false)
      dialog.showModal()
    } else if (!open && dialog.open) {
      dialog.close()
    }
  }, [open])

  async function handleSubmit() {
    if (!file) {
      setErrorMessage(t('voucherDefinitions.import.fileRequired'))
      return
    }

    if (
      !file.name.toLowerCase().endsWith('.csv') ||
      file.size <= 0 ||
      file.size > MAXIMUM_IMPORT_FILE_SIZE_BYTES
    ) {
      setErrorMessage(t('voucherDefinitions.import.fileInvalid'))
      return
    }

    setIsUploading(true)
    setErrorMessage('')
    try {
      await onImport(file)
    } catch (error) {
      setErrorMessage(error.message || t('voucherDefinitions.import.failed'))
      setIsUploading(false)
    }
  }

  function handleCancel(event) {
    event.preventDefault()
    if (!isUploading) onClose()
  }

  return (
    <dialog
      ref={dialogRef}
      className="m-auto w-[calc(100%-2rem)] max-w-lg rounded-xl border border-border bg-background p-0 text-foreground shadow-2xl backdrop:bg-foreground/45"
      aria-labelledby="voucher-pool-import-title"
      onCancel={handleCancel}
      onClick={(event) => {
        if (event.target === event.currentTarget && !isUploading) onClose()
      }}
    >
      <section className="p-5">
        <h2 id="voucher-pool-import-title" className="text-base font-semibold">
          {t('voucherDefinitions.import.title')}
        </h2>
        <p className="mt-2 text-[13px] leading-5 text-muted-foreground">
          {t('voucherDefinitions.import.description')}
        </p>

        <label className="mt-5 block text-[13px] font-medium" htmlFor="voucher-pool-csv">
          {t('voucherDefinitions.import.fileLabel')}
        </label>
        <Input
          id="voucher-pool-csv"
          className="mt-2"
          type="file"
          accept=".csv,text/csv"
          disabled={isUploading}
          onChange={(event) => {
            setFile(event.target.files?.[0] ?? null)
            setErrorMessage('')
          }}
        />
        {file ? (
          <div className="mt-3 flex items-center gap-2 rounded-lg border border-border bg-muted/30 px-3 py-2 text-[13px]">
            <FileCsvIcon size={18} className="text-[#217346]" aria-hidden="true" />
            <span className="min-w-0 flex-1 truncate">{file.name}</span>
            <span className="text-xs text-muted-foreground">
              {new Intl.NumberFormat(undefined, { style: 'unit', unit: 'kilobyte', maximumFractionDigits: 1 }).format(file.size / 1024)}
            </span>
          </div>
        ) : null}

        {errorMessage ? (
          <p role="alert" className="mt-4 rounded-lg border border-destructive/20 bg-destructive/5 px-3 py-2 text-[13px] font-medium text-destructive">
            {errorMessage}
          </p>
        ) : null}

        <div className="mt-5 flex justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={isUploading}>
            {t('common.cancel')}
          </Button>
          <Button onClick={handleSubmit} disabled={isUploading || !file}>
            {isUploading ? <CircleNotchIcon className="mr-2 animate-spin" size={16} /> : null}
            {isUploading
              ? t('voucherDefinitions.import.uploading')
              : t('voucherDefinitions.import.submit')}
          </Button>
        </div>
      </section>
    </dialog>
  )
}

export { VoucherPoolImportDialog }
