import i18n from '../i18n'

export const fallbackErrorCodes = Object.freeze({
  network: 'NETWORK_ERROR',
  timeout: 'REQUEST_TIMEOUT',
  unknown: 'UNKNOWN_ERROR',
})

export function normalizeApiError(error) {
  const responseError = error.response?.data?.error

  if (responseError?.code && responseError?.message) {
    return {
      status: error.response.status,
      code: responseError.code,
      message: responseError.message,
      details: Array.isArray(responseError.details) ? responseError.details : [],
    }
  }

  if (error.code === 'ECONNABORTED') {
    return {
      status: null,
      code: fallbackErrorCodes.timeout,
      message: i18n.t('errors.requestTimeout'),
      details: [],
    }
  }

  if (!error.response) {
    return {
      status: null,
      code: fallbackErrorCodes.network,
      message: i18n.t('errors.network'),
      details: [],
    }
  }

  return {
    status: error.response.status,
    code: fallbackErrorCodes.unknown,
    message: i18n.t('errors.unexpected'),
    details: [],
  }
}

export function toFieldErrorMap(details = []) {
  return details.reduce((fieldErrors, detail) => {
    if (detail?.field && detail?.message && !fieldErrors[detail.field]) {
      fieldErrors[detail.field] = detail.message
    }

    return fieldErrors
  }, {})
}
