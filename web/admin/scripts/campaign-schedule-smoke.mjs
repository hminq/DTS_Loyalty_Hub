import assert from 'node:assert/strict'

import { formatCampaignSchedule } from '../src/components/campaigns/campaignFormatters.js'
import {
  buildCampaignScheduleCron,
  CAMPAIGN_SCHEDULE_MODES,
  getMonthlyScheduleWarnings,
  isValidCampaignScheduleCron,
  parseCampaignScheduleCron,
} from '../src/components/campaigns/campaignSchedule.js'

const t = (key, values) => values?.defaultValue || key

console.log('Running campaign schedule smoke checks...')

assert.equal(
  buildCampaignScheduleCron({
    mode: CAMPAIGN_SCHEDULE_MODES.DAILY,
    time: '09:05',
  }),
  '0 5 9 * * ?',
)
assert.equal(
  buildCampaignScheduleCron({
    mode: CAMPAIGN_SCHEDULE_MODES.WEEKLY,
    time: '09:05',
    weekdays: ['SAT', 'MON', 'WED'],
  }),
  '0 5 9 ? * MON,WED,SAT',
)
assert.equal(
  buildCampaignScheduleCron({
    mode: CAMPAIGN_SCHEDULE_MODES.MONTHLY,
    time: '09:05',
    daysOfMonth: [31, 1, 6, 5, 5],
  }),
  '0 5 9 1,5,6,31 * ?',
)
assert.equal(
  buildCampaignScheduleCron({
    mode: CAMPAIGN_SCHEDULE_MODES.LAST_DAY,
    time: '09:05',
  }),
  '0 5 9 L * ?',
)

assert.equal(parseCampaignScheduleCron('0 5 9 * * ?').mode, CAMPAIGN_SCHEDULE_MODES.DAILY)
assert.deepEqual(
  parseCampaignScheduleCron('0 5 9 ? * MON,WED,SAT').weekdays,
  ['MON', 'WED', 'SAT'],
)
assert.deepEqual(
  parseCampaignScheduleCron('0 5 9 1,5,6,31 * ?').daysOfMonth,
  [1, 5, 6, 31],
)
assert.equal(
  parseCampaignScheduleCron('0 5 9 L * ?').mode,
  CAMPAIGN_SCHEDULE_MODES.LAST_DAY,
)
assert.equal(
  parseCampaignScheduleCron('0 5 9 01 * ?').mode,
  CAMPAIGN_SCHEDULE_MODES.CUSTOM,
)
assert.equal(
  parseCampaignScheduleCron('0 5 9 5,1 * ?').mode,
  CAMPAIGN_SCHEDULE_MODES.CUSTOM,
)

assert.equal(isValidCampaignScheduleCron('0 5 9 29 * ?'), true)
assert.equal(isValidCampaignScheduleCron('0 5 9 1,5,6 * ?'), true)
assert.equal(isValidCampaignScheduleCron('0 5 9 L * ?'), true)
assert.equal(isValidCampaignScheduleCron('0 5 9 32 * ?'), false)
assert.deepEqual(getMonthlyScheduleWarnings([1, 28]), [])
assert.deepEqual(
  getMonthlyScheduleWarnings([1, 29, 30, 31]),
  ['FEBRUARY_NON_LEAP', 'FEBRUARY', 'SHORT_MONTHS'],
)

assert.equal(
  formatCampaignSchedule('0 5 9 1,5,6 * ?', null, t),
  'Days 1, 5, 6 of every month at 09:05',
)
assert.equal(
  formatCampaignSchedule('0 5 9 L * ?', null, t),
  'Last day of every month at 09:05',
)

console.log('Campaign schedule smoke checks passed.')
