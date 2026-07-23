import { InfoIcon } from '@phosphor-icons/react'
import { useEffect, useState } from 'react'

import { toFieldErrorMap } from '../../api/apiError'
import { Button } from '../ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { Combobox } from '../ui/combobox'
import { DateTimePicker } from '../ui/date-time-picker'
import { Field, FieldError, FieldLabel } from '../ui/field'
import { Input } from '../ui/input'
import { VoucherDefinitionBannerField } from './VoucherDefinitionBannerField'

export function VoucherDefinitionForm({
  initialValues = {},
  options = {},
  isSubmitting = false,
  apiError = null,
  submitLabel,
  submittingLabel,
  onSubmit,
  onCancel,
  canUploadBanner = false,
  t,
}) {
  const [formValues, setFormValues] = useState({
    code: initialValues.code ?? '',
    name: initialValues.name ?? '',
    description: initialValues.description ?? '',
    bannerFile: initialValues.bannerFile ?? null,
    rewardType: initialValues.rewardType ?? '',
    rewardValue: initialValues.rewardValue ?? '',
    validityType: initialValues.validityType ?? '',
    validFrom: initialValues.validFrom ?? '',
    validTo: initialValues.validTo ?? '',
    durationDay: initialValues.durationDay ?? '',
    publishType: initialValues.publishType ?? '',
    generationType: initialValues.generationType ?? '',
    totalStock: initialValues.totalStock ?? '',
  })

  const [fieldErrors, setFieldErrors] = useState({})
  const [formLevelError, setFormLevelError] = useState('')

  useEffect(() => {
    if (!apiError) {
      setFieldErrors({})
      setFormLevelError('')
      return
    }

    const mapped = toFieldErrorMap(apiError.details)
    
    if (apiError.code === 'VOUCHER_CODE_ALREADY_EXISTS') {
      mapped.code = apiError.message || t('voucherDefinitions.errors.codeExists')
    }

    // Map file/type errors back to banner field
    if (mapped.file || mapped.type || apiError.code === 'VOUCHER_BANNER_IMAGE_KEY_INVALID') {
      mapped.bannerImageUrl = mapped.file || mapped.type || apiError.message
    }

    setFieldErrors(mapped)

    if (!apiError.details?.length && !mapped.code && !mapped.bannerImageUrl) {
      setFormLevelError(apiError.message || t('voucherDefinitions.errors.submitFailed'))
    } else {
      setFormLevelError('')
    }
  }, [apiError, t])

  const handleValueChange = (field, value) => {
    setFormValues((prev) => {
      const next = { ...prev, [field]: value }

      if (field === 'rewardType') {
        if (value === 'GIFT') {
          next.rewardValue = ''
        }
      } else if (field === 'publishType') {
        if (value === 'PUBLIC') {
          next.generationType = 'NONE'
        } else if (value === 'PRIVATE') {
          next.code = ''
          if (next.generationType === 'NONE') {
            next.generationType = ''
          }
        }
      } else if (field === 'validityType') {
        if (value === 'FIXED') {
          next.durationDay = ''
        } else if (value === 'DYNAMIC') {
          next.validTo = ''
        }
      }

      return next
    })
  }

  function validate() {
    const errors = {}
    const trimmedName = formValues.name?.trim() || ''
    const trimmedCode = formValues.code?.trim() || ''

    if (!trimmedName) {
      errors.name = t('voucherDefinitions.validation.nameRequired')
    }

    if (formValues.publishType === 'PUBLIC' && !trimmedCode) {
      errors.code = t('voucherDefinitions.validation.codeRequired')
    }

    if (!formValues.rewardType) {
      errors.rewardType = t('voucherDefinitions.validation.rewardTypeRequired')
    }

    if (formValues.rewardType && formValues.rewardType !== 'GIFT') {
      const val = Number(formValues.rewardValue)
      if (!formValues.rewardValue || Number.isNaN(val) || val <= 0 || (formValues.rewardType === 'PERCENT' && val > 100)) {
        errors.rewardValue = t('voucherDefinitions.validation.rewardValueInvalid')
      }
    }

    if (!formValues.validityType) {
      errors.validityType = t('voucherDefinitions.validation.validityTypeRequired')
    }

    if (!formValues.validFrom) {
      errors.validFrom = t('voucherDefinitions.validation.validFromRequired')
    }

    if (formValues.validityType === 'FIXED') {
      if (!formValues.validTo) {
        errors.validTo = t('voucherDefinitions.validation.validToRequired')
      } else if (formValues.validFrom && new Date(formValues.validFrom) >= new Date(formValues.validTo)) {
        errors.validTo = t('voucherDefinitions.validation.validToRange')
      }
    } else if (formValues.validityType === 'DYNAMIC') {
      const days = Number(formValues.durationDay)
      if (!formValues.durationDay || Number.isNaN(days) || !Number.isInteger(days) || days <= 0) {
        errors.durationDay = t('voucherDefinitions.validation.durationDayInvalid')
      }
    }

    if (!formValues.publishType) {
      errors.publishType = t('voucherDefinitions.validation.publishTypeRequired')
    }

    if (!formValues.generationType) {
      errors.generationType = t('voucherDefinitions.validation.generationTypeRequired')
    }

    const stock = Number(formValues.totalStock)
    if (!formValues.totalStock || Number.isNaN(stock) || !Number.isInteger(stock) || stock <= 0) {
      errors.totalStock = t('voucherDefinitions.validation.totalStockInvalid')
    }

    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  function handleSubmit(event) {
    event.preventDefault()
    setFormLevelError('')

    if (!validate()) return

    onSubmit({
      ...formValues,
      name: formValues.name.trim(),
      code: formValues.code?.trim() || null,
      description: formValues.description?.trim() || null,
      rewardValue: formValues.rewardType === 'GIFT' ? null : Number(formValues.rewardValue),
      durationDay: formValues.validityType === 'DYNAMIC' ? Number(formValues.durationDay) : null,
      validTo: formValues.validityType === 'FIXED' ? formValues.validTo : null,
      totalStock: Number(formValues.totalStock),
    })
  }

  const { rewardTypes = [], validityTypes = [], generationTypes = [], publishTypes = [] } = options || {}
  
  const displayGenerationTypes = formValues.publishType === 'PRIVATE' 
    ? generationTypes.filter(g => g.value !== 'NONE')
    : generationTypes

  const isGift = formValues.rewardType === 'GIFT'
  const isPublic = formValues.publishType === 'PUBLIC'
  const isPrivate = formValues.publishType === 'PRIVATE'
  const isFixed = formValues.validityType === 'FIXED'
  const isDynamic = formValues.validityType === 'DYNAMIC'

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {formLevelError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/5 px-4 py-3 text-[13px] font-medium text-destructive">
          {formLevelError}
        </div>
      ) : null}

      <div className="grid grid-cols-1 items-start gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          {/* 1. Basic information */}
          <Card>
            <CardHeader>
              <CardTitle>{t('voucherDefinitions.detail.generalTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <Field>
                <FieldLabel required>{t('voucherDefinitions.detail.name')}</FieldLabel>
                <Input
                  value={formValues.name}
                  onChange={(e) => handleValueChange('name', e.target.value)}
                  disabled={isSubmitting}
                  invalid={!!fieldErrors.name}
                />
                <FieldError>{fieldErrors.name}</FieldError>
              </Field>

              <Field>
                <FieldLabel>{t('voucherDefinitions.detail.description')}</FieldLabel>
                <Input
                  value={formValues.description}
                  onChange={(e) => handleValueChange('description', e.target.value)}
                  disabled={isSubmitting}
                  invalid={!!fieldErrors.description}
                />
                <FieldError>{fieldErrors.description}</FieldError>
              </Field>
              
              <Field>
                <FieldLabel>{t('voucherDefinitions.detail.bannerTitle')}</FieldLabel>
                <VoucherDefinitionBannerField
                  file={formValues.bannerFile}
                  onChange={(file) => handleValueChange('bannerFile', file)}
                  onClear={() => handleValueChange('bannerFile', null)}
                  error={fieldErrors.bannerImageUrl}
                  disabled={isSubmitting || !canUploadBanner}
                  t={t}
                />
              </Field>
            </CardContent>
          </Card>

          {/* 2. Reward */}
          <Card>
            <CardHeader>
              <CardTitle>{t('voucherDefinitions.detail.rewardTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <Field>
                  <FieldLabel required>{t('voucherDefinitions.detail.rewardType')}</FieldLabel>
                  <Combobox
                    value={formValues.rewardType}
                    selectedLabel={rewardTypes.find((o) => o.value === formValues.rewardType)?.label}
                    options={rewardTypes}
                    onValueChange={(val) => handleValueChange('rewardType', val)}
                    disabled={isSubmitting}
                    invalid={!!fieldErrors.rewardType}
                    shouldFilter={false}
                  />
                  <FieldError>{fieldErrors.rewardType}</FieldError>
                </Field>

                <Field>
                  <FieldLabel required={!isGift}>{t('voucherDefinitions.detail.rewardValue')}</FieldLabel>
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    value={formValues.rewardValue}
                    onChange={(e) => handleValueChange('rewardValue', e.target.value)}
                    disabled={isSubmitting || isGift}
                    invalid={!!fieldErrors.rewardValue}
                  />
                  <FieldError>{fieldErrors.rewardValue}</FieldError>
                </Field>
              </div>
            </CardContent>
          </Card>

          {/* 3. Validity */}
          <Card>
            <CardHeader>
              <CardTitle>{t('voucherDefinitions.detail.validityTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <Field>
                <FieldLabel required>{t('voucherDefinitions.detail.validityType')}</FieldLabel>
                <Combobox
                  value={formValues.validityType}
                  selectedLabel={validityTypes.find((o) => o.value === formValues.validityType)?.label}
                  options={validityTypes}
                  onValueChange={(val) => handleValueChange('validityType', val)}
                  disabled={isSubmitting}
                  invalid={!!fieldErrors.validityType}
                  shouldFilter={false}
                />
                <FieldError>{fieldErrors.validityType}</FieldError>
              </Field>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <Field>
                  <FieldLabel required>{t('voucherDefinitions.detail.validFrom')}</FieldLabel>
                  <DateTimePicker
                    value={formValues.validFrom}
                    onChange={(val) => handleValueChange('validFrom', val)}
                    disabled={isSubmitting}
                  />
                  <FieldError>{fieldErrors.validFrom}</FieldError>
                </Field>

                {isDynamic ? (
                  <Field>
                    <FieldLabel required>{t('voucherDefinitions.detail.durationDay')}</FieldLabel>
                    <Input
                      type="number"
                      min="1"
                      step="1"
                      value={formValues.durationDay}
                      onChange={(e) => handleValueChange('durationDay', e.target.value)}
                      disabled={isSubmitting}
                      invalid={!!fieldErrors.durationDay}
                    />
                    <FieldError>{fieldErrors.durationDay}</FieldError>
                  </Field>
                ) : (
                  <Field>
                    <FieldLabel required={isFixed}>{t('voucherDefinitions.detail.validTo')}</FieldLabel>
                    <DateTimePicker
                      value={formValues.validTo}
                      onChange={(val) => handleValueChange('validTo', val)}
                      disabled={isSubmitting || !isFixed}
                    />
                    <FieldError>{fieldErrors.validTo}</FieldError>
                  </Field>
                )}
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          {/* 4. Publishing */}
          <Card>
            <CardHeader>
              <CardTitle>{t('voucherDefinitions.detail.publishType')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <Field>
                <FieldLabel required>{t('voucherDefinitions.detail.publishType')}</FieldLabel>
                <Combobox
                  value={formValues.publishType}
                  selectedLabel={publishTypes.find((o) => o.value === formValues.publishType)?.label}
                  options={publishTypes}
                  onValueChange={(val) => handleValueChange('publishType', val)}
                  disabled={isSubmitting}
                  invalid={!!fieldErrors.publishType}
                  shouldFilter={false}
                />
                <FieldError>{fieldErrors.publishType}</FieldError>
              </Field>
              
              {isPrivate ? (
                <div className="flex gap-2 rounded-md bg-warning/10 p-3 text-sm text-warning-foreground">
                  <InfoIcon size={16} className="mt-0.5 shrink-0" weight="fill" />
                  <p>{t('voucherDefinitions.form.privatePoolWarning')}</p>
                </div>
              ) : null}

              <Field>
                <FieldLabel required={isPublic}>{t('voucherDefinitions.detail.code')}</FieldLabel>
                <Input
                  value={formValues.code}
                  onChange={(e) => handleValueChange('code', e.target.value)}
                  disabled={isSubmitting || isPrivate}
                  invalid={!!fieldErrors.code}
                />
                <FieldError>{fieldErrors.code}</FieldError>
              </Field>

              <Field>
                <FieldLabel required>{t('voucherDefinitions.detail.generationType')}</FieldLabel>
                <Combobox
                  value={formValues.generationType}
                  selectedLabel={displayGenerationTypes.find((o) => o.value === formValues.generationType)?.label}
                  options={displayGenerationTypes}
                  onValueChange={(val) => handleValueChange('generationType', val)}
                  disabled={isSubmitting || isPublic}
                  invalid={!!fieldErrors.generationType}
                  shouldFilter={false}
                />
                <FieldError>{fieldErrors.generationType}</FieldError>
              </Field>
            </CardContent>
          </Card>

          {/* 5. Inventory */}
          <Card>
            <CardHeader>
              <CardTitle>{t('voucherDefinitions.detail.inventoryTitle')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <Field>
                <FieldLabel required>{t('voucherDefinitions.detail.totalStock')}</FieldLabel>
                <Input
                  type="number"
                  min="1"
                  step="1"
                  value={formValues.totalStock}
                  onChange={(e) => handleValueChange('totalStock', e.target.value)}
                  disabled={isSubmitting}
                  invalid={!!fieldErrors.totalStock}
                />
                <FieldError>{fieldErrors.totalStock}</FieldError>
              </Field>
            </CardContent>
          </Card>
        </div>
      </div>

      <div className="flex items-center justify-end gap-3 pt-2">
        <Button
          type="button"
          variant="outline"
          disabled={isSubmitting}
          onClick={onCancel}
        >
          {t('common.cancel')}
        </Button>
        <Button
          type="submit"
          disabled={isSubmitting}
        >
          {isSubmitting ? submittingLabel : submitLabel}
        </Button>
      </div>
    </form>
  )
}
