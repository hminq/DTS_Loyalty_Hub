export const CAMPAIGN_SCHEDULE_MODES = Object.freeze({
  DAILY: 'DAILY',
  WEEKLY: 'WEEKLY',
  MONTHLY: 'MONTHLY',
  LAST_DAY: 'LAST_DAY',
  CUSTOM: 'CUSTOM',
})

export const CAMPAIGN_SCHEDULE_WEEKDAYS = Object.freeze([
  'MON',
  'TUE',
  'WED',
  'THU',
  'FRI',
  'SAT',
  'SUN',
])

export const DEFAULT_CAMPAIGN_SCHEDULE_TIME = '02:00'

function parseCanonicalNumber(value, minimum, maximum) {
  const parsed = Number(value)
  if (
    !Number.isInteger(parsed) ||
    parsed < minimum ||
    parsed > maximum ||
    value !== String(parsed)
  ) {
    return null
  }

  return parsed
}

function toTime(hour, minute) {
  return `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
}

function parseTime(value) {
  if (typeof value !== 'string') return null
  const match = /^(\d{2}):(\d{2})$/.exec(value)
  if (!match) return null

  const hour = Number(match[1])
  const minute = Number(match[2])
  if (hour < 0 || hour > 23 || minute < 0 || minute > 59) return null

  return { hour, minute }
}

function parseWeekdays(dayOfWeek) {
  const weekdays = dayOfWeek.split(',')
  if (weekdays.length === 0 || weekdays.some((day) => !day)) return null

  let previousIndex = -1
  for (const weekday of weekdays) {
    const currentIndex = CAMPAIGN_SCHEDULE_WEEKDAYS.indexOf(weekday)
    if (currentIndex === -1 || currentIndex <= previousIndex) return null
    previousIndex = currentIndex
  }

  return weekdays
}

function parseDaysOfMonth(dayOfMonth) {
  const tokens = dayOfMonth.split(',')
  if (tokens.length === 0 || tokens.some((day) => !day)) return null

  const days = []
  let previousDay = 0

  for (const token of tokens) {
    const day = parseCanonicalNumber(token, 1, 31)
    if (day === null || day <= previousDay) return null
    previousDay = day
    days.push(day)
  }

  return days
}

export function parseCampaignScheduleCron(value) {
  const cron = typeof value === 'string' ? value.trim() : ''
  const parts = cron.split(/\s+/)
  const fallback = {
    mode: CAMPAIGN_SCHEDULE_MODES.CUSTOM,
    time: DEFAULT_CAMPAIGN_SCHEDULE_TIME,
    weekdays: [],
    daysOfMonth: [1],
    dayOfMonth: 1,
    cron,
  }

  if (parts.length !== 6) return fallback

  const [second, minuteToken, hourToken, dayOfMonth, month, dayOfWeek] = parts
  const minute = parseCanonicalNumber(minuteToken, 0, 59)
  const hour = parseCanonicalNumber(hourToken, 0, 23)
  if (second !== '0' || minute === null || hour === null) return fallback

  const time = toTime(hour, minute)

  if (dayOfMonth === '*' && month === '*' && dayOfWeek === '?') {
    return {
      ...fallback,
      mode: CAMPAIGN_SCHEDULE_MODES.DAILY,
      time,
    }
  }

  if (dayOfMonth === '?' && month === '*') {
    const weekdays = parseWeekdays(dayOfWeek)
    if (!weekdays) return { ...fallback, time }

    return {
      ...fallback,
      mode: CAMPAIGN_SCHEDULE_MODES.WEEKLY,
      time,
      weekdays,
    }
  }

  if (month === '*' && dayOfWeek === '?') {
    if (dayOfMonth === 'L') {
      return {
        ...fallback,
        mode: CAMPAIGN_SCHEDULE_MODES.LAST_DAY,
        time,
      }
    }

    const parsedDays = parseDaysOfMonth(dayOfMonth)
    if (parsedDays) {
      return {
        ...fallback,
        mode: CAMPAIGN_SCHEDULE_MODES.MONTHLY,
        time,
        daysOfMonth: parsedDays,
        dayOfMonth: parsedDays.length === 1 ? parsedDays[0] : null,
      }
    }
  }

  return { ...fallback, time }
}

export function buildCampaignScheduleCron({
  mode,
  time = DEFAULT_CAMPAIGN_SCHEDULE_TIME,
  weekdays = [],
  daysOfMonth = [],
  dayOfMonth = 1,
}) {
  const parsedTime = parseTime(time)
  if (!parsedTime) return ''

  const { hour, minute } = parsedTime
  if (mode === CAMPAIGN_SCHEDULE_MODES.DAILY) {
    return `0 ${minute} ${hour} * * ?`
  }

  if (mode === CAMPAIGN_SCHEDULE_MODES.WEEKLY) {
    const selectedDays = new Set(weekdays)
    const canonicalDays = CAMPAIGN_SCHEDULE_WEEKDAYS.filter((day) => selectedDays.has(day))
    if (canonicalDays.length === 0) return ''
    return `0 ${minute} ${hour} ? * ${canonicalDays.join(',')}`
  }

  if (mode === CAMPAIGN_SCHEDULE_MODES.MONTHLY) {
    const requestedDays = daysOfMonth.length > 0 ? daysOfMonth : [dayOfMonth]
    const parsedDays = requestedDays.map(Number)
    if (
      parsedDays.some(
        (day) => !Number.isInteger(day) || day < 1 || day > 31,
      )
    ) {
      return ''
    }

    const canonicalDays = [...new Set(parsedDays)].sort((left, right) => left - right)
    if (canonicalDays.length === 0) return ''
    return `0 ${minute} ${hour} ${canonicalDays.join(',')} * ?`
  }

  if (mode === CAMPAIGN_SCHEDULE_MODES.LAST_DAY) {
    return `0 ${minute} ${hour} L * ?`
  }

  return ''
}

export function isValidCampaignScheduleCron(value) {
  return parseCampaignScheduleCron(value).mode !== CAMPAIGN_SCHEDULE_MODES.CUSTOM
}

export function getMonthlyScheduleWarnings(daysOfMonth) {
  const selectedDays = new Set((daysOfMonth || []).map(Number))
  const warnings = []

  if (selectedDays.has(29)) warnings.push('FEBRUARY_NON_LEAP')
  if (selectedDays.has(30)) warnings.push('FEBRUARY')
  if (selectedDays.has(31)) warnings.push('SHORT_MONTHS')

  return warnings
}
