import assert from 'node:assert/strict'
import { mapEventDefinitionOptions } from '../src/components/event-definitions/eventDefinitionOptions.js'
import {
  getCompatibleIdFields,
  validateMetadataForm,
  validateSchemaForm,
} from '../src/components/event-definitions/eventDefinitionValidation.js'
import {
  buildCanonicalPayloadSchema,
  buildCreateEventDefinitionPayload,
  buildUpdateDraftSchemaPayload,
  buildUpdateMetadataPayload,
  mapVersionToFormState,
} from '../src/components/event-definitions/eventDefinitionPayloads.js'

const t = (key, fallback) => fallback || key

const sampleBackendOptions = {
  fieldTypes: [
    { code: 'STRING', formats: ['UUID'], operators: ['EQUALS', 'NOT_EQUALS', 'CONTAINS'] },
    { code: 'NUMBER', formats: [], operators: ['EQUALS', 'NOT_EQUALS', 'GT', 'GTE', 'LT', 'LTE'] },
    { code: 'BOOLEAN', formats: [], operators: ['EQUALS', 'NOT_EQUALS'] },
  ],
  targetKinds: [
    { code: 'CUSTOMER', identityFieldType: 'STRING', identityFieldFormat: 'UUID' },
  ],
  eventTypeStatuses: ['ACTIVE', 'RETIRED'],
  versionStatuses: ['DRAFT', 'PUBLISHED', 'RETIRED'],
  limits: {
    maximumFields: 100,
    maximumTargets: 20,
    maximumSchemaBytes: 65536,
  },
}

async function runSmokeTests() {
  console.log('Running M7 event definitions contract smoke checks...')

  // 1. Options mapping
  const mappedOpts = mapEventDefinitionOptions(sampleBackendOptions, t)
  assert.equal(mappedOpts.fieldTypeOptions.length, 3)
  assert.equal(mappedOpts.targetKindOptions.length, 1)
  assert.equal(mappedOpts.statusOptions.length, 2)
  assert.equal(mappedOpts.versionStatusOptions.length, 3)

  // 2. Metadata form validation
  const validMeta = validateMetadataForm({
    code: 'CUSTOMER_TRANSACTION_SUCCEEDED',
    routingKey: 'customer.transaction.succeeded',
    name: 'Customer Transaction Succeeded',
    description: 'Raised when transaction succeeds',
  })
  assert.equal(validMeta.isValid, true)
  assert.deepEqual(validMeta.errors, {})

  const invalidMeta = validateMetadataForm({
    code: 'invalid_code',
    routingKey: 'INVALID ROUTING KEY',
    name: '',
  })
  assert.equal(invalidMeta.isValid, false)
  assert.ok(invalidMeta.errors.code)
  assert.ok(invalidMeta.errors.routingKey)
  assert.ok(invalidMeta.errors.name)

  // 3. Compatible ID fields filtering
  const testFields = [
    { code: 'customerId', type: 'STRING', format: 'UUID', required: true, conditionable: false },
    { code: 'optionalId', type: 'STRING', format: 'UUID', required: false, conditionable: false },
    { code: 'textName', type: 'STRING', format: null, required: true, conditionable: true },
    { code: 'amount', type: 'NUMBER', format: null, required: true, conditionable: true },
  ]
  const compatible = getCompatibleIdFields(testFields)
  assert.equal(compatible.length, 1)
  assert.equal(compatible[0].code, 'customerId')

  // 4. Field duplicate detection
  const dupFields = [
    { code: 'amount', type: 'NUMBER', format: null, required: true, conditionable: true },
    { code: 'amount', type: 'NUMBER', format: null, required: false, conditionable: true },
  ]
  const dupFieldVal = validateSchemaForm(dupFields, [])
  assert.equal(dupFieldVal.isValid, false)
  assert.ok(dupFieldVal.fieldErrors[1].code)

  // 5. Target duplicate detection & incompatible idField
  const testTargets = [
    { selector: 'TARGET_CUST', kind: 'CUSTOMER', idField: 'customerId' },
    { selector: 'TARGET_CUST', kind: 'CUSTOMER', idField: 'amount' },
  ]
  const targetVal = validateSchemaForm(testFields, testTargets)
  assert.equal(targetVal.isValid, false)
  assert.ok(targetVal.targetErrors[1].selector)
  assert.ok(targetVal.targetErrors[1].idField)

  // 6. Payload builder output shape
  const canonical = buildCanonicalPayloadSchema(
    [
      { code: 'customerId', type: 'STRING', format: 'UUID', required: true, conditionable: false },
      { code: 'amount', type: 'NUMBER', format: 'UUID', required: true, conditionable: true }, // format should be reset to null for NUMBER
    ],
    [{ selector: 'TARGET_CUST', kind: 'CUSTOMER', idField: 'customerId' }],
  )
  assert.equal(canonical.fields.length, 2)
  assert.equal(canonical.fields[0].format, 'UUID')
  assert.equal(canonical.fields[1].format, null)
  assert.equal(canonical.targets.length, 1)
  assert.equal(canonical.targets[0].idField, 'customerId')

  const createPayload = buildCreateEventDefinitionPayload(
    {
      code: 'CUSTOMER_TRANSACTION_SUCCEEDED',
      routingKey: 'customer.transaction.succeeded',
      name: 'Transaction Succeeded',
      description: 'Desc',
    },
    testFields,
    [{ selector: 'TARGET_CUST', kind: 'CUSTOMER', idField: 'customerId' }],
  )
  assert.equal(createPayload.code, 'CUSTOMER_TRANSACTION_SUCCEEDED')
  assert.equal(createPayload.payloadSchema.fields.length, 4)

  const updateMetaPayload = buildUpdateMetadataPayload({
    code: 'NEW_CODE',
    routingKey: 'new.routing',
    name: 'New Name',
    description: null,
  })
  assert.equal(updateMetaPayload.name, 'New Name')

  const updateDraftPayload = buildUpdateDraftSchemaPayload(testFields, [])
  assert.equal(updateDraftPayload.payloadSchema.fields.length, 4)

  // 7. Version detail to form state mapping
  const mappedForm = mapVersionToFormState({
    payloadSchema: {
      fields: [{ code: 'userId', type: 'STRING', format: 'UUID', required: true, conditionable: true }],
      targets: [{ selector: 'USER_TARGET', kind: 'CUSTOMER', idField: 'userId' }],
    },
  })
  assert.equal(mappedForm.fields.length, 1)
  assert.equal(mappedForm.fields[0].code, 'userId')
  assert.equal(mappedForm.targets.length, 1)
  assert.equal(mappedForm.targets[0].selector, 'USER_TARGET')

  console.log('All M7 contract smoke checks passed cleanly!')
}

runSmokeTests().catch((err) => {
  console.error('M7 contract smoke checks failed:', err)
  process.exit(1)
})
