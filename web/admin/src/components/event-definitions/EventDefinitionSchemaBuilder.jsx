import { useTranslation } from 'react-i18next'

import { Card, CardContent, CardHeader, CardTitle } from '../ui/card'
import { EventDefinitionFieldRows } from './EventDefinitionFieldRows'
import { EventDefinitionJsonPreview } from './EventDefinitionJsonPreview'
import { EventDefinitionTargetRows } from './EventDefinitionTargetRows'

export function EventDefinitionSchemaBuilder({
  fields = [],
  targets = [],
  fieldTypeOptions = [],
  targetKindOptions = [],
  onChangeFields,
  onChangeTargets,
  fieldErrors = [],
  targetErrors = [],
  disabled = false,
}) {
  const { t } = useTranslation()

  const handleAddField = () => {
    const newField = {
      id: Math.random().toString(36).substring(2, 9),
      code: '',
      type: 'STRING',
      format: '',
      required: false,
      conditionable: true,
    }
    onChangeFields([...fields, newField])
  }

  const handleChangeField = (index, key, value) => {
    const updated = [...fields]
    const current = { ...updated[index], [key]: value }

    if (key === 'type' && value !== 'STRING') {
      current.format = ''
    }

    updated[index] = current
    onChangeFields(updated)
  }

  const handleRemoveField = (index) => {
    const updated = fields.filter((_, i) => i !== index)
    onChangeFields(updated)
  }

  const handleReorderField = (fromIndex, toIndex) => {
    if (toIndex < 0 || toIndex >= fields.length) return
    const updated = [...fields]
    const [moved] = updated.splice(fromIndex, 1)
    updated.splice(toIndex, 0, moved)
    onChangeFields(updated)
  }

  const handleAddTarget = () => {
    const newTarget = {
      id: Math.random().toString(36).substring(2, 9),
      selector: '',
      kind: 'CUSTOMER',
      idField: '',
    }
    onChangeTargets([...targets, newTarget])
  }

  const handleChangeTarget = (index, key, value) => {
    const updated = [...targets]
    updated[index] = { ...updated[index], [key]: value }
    onChangeTargets(updated)
  }

  const handleRemoveTarget = (index) => {
    const updated = targets.filter((_, i) => i !== index)
    onChangeTargets(updated)
  }

  const handleReorderTarget = (fromIndex, toIndex) => {
    if (toIndex < 0 || toIndex >= targets.length) return
    const updated = [...targets]
    const [moved] = updated.splice(fromIndex, 1)
    updated.splice(toIndex, 0, moved)
    onChangeTargets(updated)
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t('eventDefinitions.schema.builderTitle')}</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-6">
        <EventDefinitionFieldRows
          fields={fields}
          fieldTypeOptions={fieldTypeOptions}
          onChangeField={handleChangeField}
          onAddField={handleAddField}
          onRemoveField={handleRemoveField}
          onReorderField={handleReorderField}
          errors={fieldErrors}
          disabled={disabled}
        />

        <div className="h-px bg-border" aria-hidden="true" />

        <EventDefinitionTargetRows
          targets={targets}
          fields={fields}
          targetKindOptions={targetKindOptions}
          onChangeTarget={handleChangeTarget}
          onAddTarget={handleAddTarget}
          onRemoveTarget={handleRemoveTarget}
          onReorderTarget={handleReorderTarget}
          errors={targetErrors}
          disabled={disabled}
        />

        <div className="h-px bg-border" aria-hidden="true" />

        <EventDefinitionJsonPreview fields={fields} targets={targets} />
      </CardContent>
    </Card>
  )
}
