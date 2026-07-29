import assert from 'node:assert/strict'
import { mapCampaignOptions } from '../src/components/campaigns/campaignOptions.js'
import {
  buildConditionFromFormState,
  createMatchAllConditionFormState,
  inspectPersistedCondition,
  mapConditionToFormState,
  validateConditionFormState,
} from '../src/components/campaigns/campaignConditions.js'
import {
  buildCampaignCreatePayload,
  buildCampaignActionPayload,
  mapCampaignDetailToFormValues
} from '../src/components/campaigns/campaignPayloads.js'
import { describeCampaignAction, describeCampaignCondition } from '../src/components/campaigns/campaignPresentation.js'

// Mock translation function
const t = (key, fallback) => fallback?.defaultValue || key

const sampleOptions = {
  data: {
    eventTypeVersions: [
      {
        eventTypeId: 'type-uuid-1',
        eventTypeVersionId: 'version-uuid-1',
        code: 'REGISTRATION',
        name: 'Registration',
        version: 1,
        condition: {
          combinators: ['ALL'],
          fields: [
            { code: 'username', dataType: 'STRING', operators: ['EQUALS'] },
            { code: 'age', dataType: 'NUMBER', operators: ['GT'] },
            { code: 'isVip', dataType: 'BOOLEAN', operators: ['EQUALS'] }
          ]
        },
        targets: [
          { selector: 'REGISTERED_CUSTOMER', targetKind: 'CUSTOMER' },
          { selector: 'ACCOUNT_ID', targetKind: 'ACCOUNT' }
        ]
      }
    ],
    actionTypes: [
      {
        code: 'ISSUE_POINT',
        requiredTargetKind: 'CUSTOMER',
        parameters: [{ code: 'amount', dataType: 'DECIMAL', required: true }]
      }
    ]
  }
}

async function runSmokeTests() {
  console.log('Running M3 contract smoke checks...')

  // 1. options map eventTypeVersions
  // 2. logical filter values use eventTypeId
  // 3. target mapping uses selector
  const mappedOptions = mapCampaignOptions(sampleOptions.data, t)
  
  assert.equal(mappedOptions.eventTypeVersions.length, 1)
  assert.equal(mappedOptions.eventTypeVersions[0].value, 'version-uuid-1')
  
  assert.equal(mappedOptions.eventTypeFilters.length, 1)
  assert.equal(mappedOptions.eventTypeFilters[0].value, 'type-uuid-1')
  
  assert.equal(mappedOptions.eventTypeVersions[0].targets[0].value, 'REGISTERED_CUSTOMER')
  assert.equal(mappedOptions.eventTypeVersions[0].targets[0].selector, 'REGISTERED_CUSTOMER')

  const selectedVersion = mappedOptions.eventTypeVersions[0]

  // 4. match-all serializes to {"all":[]}
  const matchAllForm = { conditionMode: 'MATCH_ALL', conditionPredicates: [] }
  const matchAllPayload = buildConditionFromFormState(matchAllForm, selectedVersion)
  assert.deepEqual(matchAllPayload, { all: [] })

  // 5. string value remains a string
  // 6. number value becomes a number
  // 7. boolean value becomes a boolean
  const matchFieldsForm = {
    conditionMode: 'MATCH_FIELDS',
    conditionPredicates: [
      { field: 'username', operator: 'EQUALS', value: 'john' },
      { field: 'age', operator: 'GT', value: '18' },
      { field: 'isVip', operator: 'EQUALS', value: 'true' }
    ]
  }
  
  const typedPayload = buildConditionFromFormState(matchFieldsForm, selectedVersion)
  assert.deepEqual(typedPayload.all[0], { field: 'username', operator: 'EQUALS', value: 'john' })
  assert.deepEqual(typedPayload.all[1], { field: 'age', operator: 'GT', value: 18 })
  assert.deepEqual(typedPayload.all[2], { field: 'isVip', operator: 'EQUALS', value: true })

  // 8. create payload contains eventTypeVersionId
  // 9. create payload does not contain eventType
  const campaignFormValues = {
    campaignName: 'Test',
    eventTypeVersionId: 'version-uuid-1',
    conditionMode: 'MATCH_ALL',
    conditionPredicates: []
  }
  const fullPayload = buildCampaignCreatePayload(campaignFormValues, mappedOptions)
  assert.equal(fullPayload.eventTypeVersionId, 'version-uuid-1')
  assert.equal(fullPayload.eventType, undefined)
  assert.equal(fullPayload.conditionPresetCode, undefined)

  // 10. action config contains only target + parameters
  const actionFormValues = {
    actionType: 'ISSUE_POINT',
    targetSelector: 'REGISTERED_CUSTOMER',
    executeOrder: '1',
    parameters: { amount: '50' }
  }
  const actionPayload = buildCampaignActionPayload(actionFormValues, 1, mappedOptions)
  assert.deepEqual(actionPayload.actionConfig, {
    target: { selector: 'REGISTERED_CUSTOMER' },
    parameters: { amount: 50 }
  })
  assert.equal(actionPayload.target, undefined)

  // 11. detail hydration uses campaign.eventDefinition.eventTypeVersionId
  const sampleCampaign = {
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    condition: { all: [] }
  }
  const hydratedForm = mapCampaignDetailToFormValues(sampleCampaign)
  assert.equal(hydratedForm.eventTypeVersionId, 'version-uuid-1')
  assert.equal(hydratedForm.eventType, undefined)

  // 12. Strict condition inspection tests
  assert.equal(inspectPersistedCondition({ all: [] }, selectedVersion).isMatchAll, true)
  assert.equal(inspectPersistedCondition({ any: [] }, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition({ all: [], unexpected: true }, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition({ all: {} }, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition(null, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition([], selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition({ all: [{ field: 'username', operator: 'EQUALS', value: 'john' }] }).isSupported, false)

  // 13. Invalid condition predicates fail inspection
  assert.equal(inspectPersistedCondition({ all: [{ field: 'unknown', operator: 'EQUALS', value: 'test' }] }, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition({ all: [{ field: 'username', operator: 'UNKNOWN_OP', value: 'test' }] }, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition({ all: [{ field: 'age', operator: 'GT', value: 'not-a-number' }] }, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition({ all: [{ field: 'username', operator: 'EQUALS', value: '' }] }, selectedVersion).isSupported, false)
  assert.equal(inspectPersistedCondition({ all: [{ field: 'username', operator: 'EQUALS', value: 'john' }, { field: 'username', operator: 'EQUALS', value: 'john' }] }, selectedVersion).isSupported, false)

  assert.equal(mapConditionToFormState(null, selectedVersion).conditionMode, 'UNSUPPORTED')
  assert.equal(mapConditionToFormState({ any: [] }, selectedVersion).conditionMode, 'UNSUPPORTED')
  assert.equal(
    describeCampaignCondition({
      condition: { all: [{ field: 'username', operator: 'EQUALS', value: 'john' }] },
      eventDefinition: { eventTypeVersionId: 'missing-version' },
      options: mappedOptions,
      t
    }).isSupported,
    false
  )

  // 14. buildConditionFromFormState throws on invalid input
  assert.throws(() => buildConditionFromFormState({ conditionMode: 'UNSUPPORTED' }, selectedVersion))
  assert.throws(() => buildConditionFromFormState({ conditionMode: 'MATCH_ALL', conditionPredicates: [] }))
  assert.throws(() => buildConditionFromFormState({ conditionMode: 'MATCH_FIELDS', conditionPredicates: [] }, selectedVersion))
  assert.throws(() => buildConditionFromFormState({ conditionMode: 'MATCH_FIELDS', conditionPredicates: [{ field: 'age', operator: 'GT', value: '' }] }, selectedVersion))
  assert.throws(() => buildConditionFromFormState({ conditionMode: 'MATCH_FIELDS', conditionPredicates: [{ field: 'unknown', operator: 'EQUALS', value: 'val' }] }, selectedVersion))
  assert.throws(() => buildConditionFromFormState({ conditionMode: 'MATCH_FIELDS', conditionPredicates: [{ field: 'age', operator: 'GT', value: '1' }, { field: 'age', operator: 'GT', value: '01' }] }, selectedVersion))
  assert.ok(validateConditionFormState({ conditionMode: 'MATCH_FIELDS', conditionPredicates: [{ field: 'age', operator: 'GT', value: '1' }, { field: 'age', operator: 'GT', value: '01' }] }, selectedVersion, t)?.conditionRows?.[1]?.duplicate)

  // 15. Shared version change reset helper
  const resetState = createMatchAllConditionFormState()
  assert.equal(resetState.conditionMode, 'MATCH_ALL')
  assert.deepEqual(resetState.conditionPredicates, [])
  assert.equal(resetState.originalCondition, null)

  // 16. Action presentation target-kind and parameter verification
  const validActionDesc = describeCampaignAction({
    action: { actionType: 'ISSUE_POINT', actionConfig: { target: { selector: 'REGISTERED_CUSTOMER' }, parameters: { amount: 50 } } },
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    options: mappedOptions,
    t
  })
  assert.equal(validActionDesc.isSupported, true)

  const wrongKindDesc = describeCampaignAction({
    action: { actionType: 'ISSUE_POINT', actionConfig: { target: { selector: 'ACCOUNT_ID' }, parameters: { amount: 50 } } },
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    options: mappedOptions,
    t
  })
  assert.equal(wrongKindDesc.isSupported, false)

  const missingReqParamDesc = describeCampaignAction({
    action: { actionType: 'ISSUE_POINT', actionConfig: { target: { selector: 'REGISTERED_CUSTOMER' }, parameters: {} } },
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    options: mappedOptions,
    t
  })
  assert.equal(missingReqParamDesc.isSupported, false)

  const unknownParamDesc = describeCampaignAction({
    action: { actionType: 'ISSUE_POINT', actionConfig: { target: { selector: 'REGISTERED_CUSTOMER' }, parameters: { amount: 50, extra: 123 } } },
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    options: mappedOptions,
    t
  })
  assert.equal(unknownParamDesc.isSupported, false)

  const malformedEnvelopeDesc = describeCampaignAction({
    action: { actionType: 'ISSUE_POINT', actionConfig: null },
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    options: mappedOptions,
    t
  })
  assert.equal(malformedEnvelopeDesc.isSupported, false)

  const extraEnvelopeDesc = describeCampaignAction({
    action: { actionType: 'ISSUE_POINT', actionConfig: { target: { selector: 'REGISTERED_CUSTOMER' }, parameters: { amount: 50 }, unexpected: true } },
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    options: mappedOptions,
    t
  })
  assert.equal(extraEnvelopeDesc.isSupported, false)

  const unsupportedParameterTypeOptions = {
    ...mappedOptions,
    actionTypes: [
      {
        value: 'ISSUE_POINT',
        label: 'Issue points',
        requiredTargetKind: 'CUSTOMER',
        parameters: [{ code: 'memo', label: 'Memo', dataType: 'STRING', required: true }]
      }
    ]
  }
  const unsupportedParameterTypeDesc = describeCampaignAction({
    action: { actionType: 'ISSUE_POINT', actionConfig: { target: { selector: 'REGISTERED_CUSTOMER' }, parameters: { memo: 'note' } } },
    eventDefinition: { eventTypeVersionId: 'version-uuid-1' },
    options: unsupportedParameterTypeOptions,
    t
  })
  assert.equal(unsupportedParameterTypeDesc.isSupported, false)

  console.log('Smoke checks passed.')
}

runSmokeTests().catch(err => {
  console.error('Smoke tests failed:', err)
  process.exit(1)
})
