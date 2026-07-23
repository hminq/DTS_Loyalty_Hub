import { MagnifyingGlassIcon, XIcon } from '@phosphor-icons/react'

import { Combobox } from '../ui/combobox'
import { Input } from '../ui/input'
import { Button } from '../ui/button'

function VoucherDefinitionsFilters({
  keyword,
  rewardType,
  validityType,
  publishType,
  options,
  isLoadingOptions,
  optionsError,
  onKeywordChange,
  onRewardTypeChange,
  onValidityTypeChange,
  onPublishTypeChange,
  onRetryOptions,
  onClearFilters,
  t
}) {
  const hasActiveFilters = Boolean(keyword || rewardType || validityType || publishType)

  return (
    <div className="mb-4 flex flex-col gap-3 rounded-lg border border-border bg-muted/25 p-3 xl:flex-row xl:items-end">
      <label className="grid min-w-0 flex-1 gap-1.5">
        <span className="text-xs font-medium">{t('voucherDefinitions.filters.searchLabel')}</span>
        <div className="relative">
          <MagnifyingGlassIcon
            className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
            size={16}
          />
          <Input
            className="pl-9 pr-9"
            value={keyword}
            onChange={(event) => onKeywordChange(event.target.value)}
            placeholder={t('voucherDefinitions.filters.searchPlaceholder')}
            maxLength={100}
          />
          {keyword ? (
            <button
              type="button"
              onClick={() => onKeywordChange('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
              aria-label={t('voucherDefinitions.filters.clearSearch')}
            >
              <XIcon size={14} />
            </button>
          ) : null}
        </div>
      </label>

      <div className="grid flex-1 grid-cols-1 gap-3 sm:grid-cols-3">
        <label className="grid gap-1.5">
          <span className="text-xs font-medium">{t('voucherDefinitions.filters.rewardTypeLabel')}</span>
          <Combobox
            value={rewardType}
            onValueChange={onRewardTypeChange}
            options={options?.rewardTypes ?? []}
            placeholder={t('voucherDefinitions.filters.allRewardTypes')}
            emptyOptionLabel={t('voucherDefinitions.filters.allRewardTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('voucherDefinitions.filters.rewardTypeLabel')}
          />
        </label>

        <label className="grid gap-1.5">
          <span className="text-xs font-medium">{t('voucherDefinitions.filters.validityTypeLabel')}</span>
          <Combobox
            value={validityType}
            onValueChange={onValidityTypeChange}
            options={options?.validityTypes ?? []}
            placeholder={t('voucherDefinitions.filters.allValidityTypes')}
            emptyOptionLabel={t('voucherDefinitions.filters.allValidityTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('voucherDefinitions.filters.validityTypeLabel')}
          />
        </label>

        <label className="grid gap-1.5">
          <span className="text-xs font-medium">{t('voucherDefinitions.filters.publishTypeLabel')}</span>
          <Combobox
            value={publishType}
            onValueChange={onPublishTypeChange}
            options={options?.publishTypes ?? []}
            placeholder={t('voucherDefinitions.filters.allPublishTypes')}
            emptyOptionLabel={t('voucherDefinitions.filters.allPublishTypes')}
            isLoading={isLoadingOptions}
            error={optionsError}
            ariaLabel={t('voucherDefinitions.filters.publishTypeLabel')}
          />
        </label>
      </div>

      {optionsError ? (
        <Button variant="outline" type="button" onClick={onRetryOptions} className="shrink-0">
          {t('common.retry')}
        </Button>
      ) : null}
      
      {hasActiveFilters ? (
        <Button variant="outline" type="button" onClick={onClearFilters} className="shrink-0">
          {t('voucherDefinitions.filters.clearFilters')}
        </Button>
      ) : null}
    </div>
  )
}

export { VoucherDefinitionsFilters }
