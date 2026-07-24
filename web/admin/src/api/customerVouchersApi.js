import httpClient from './httpClient'

export async function getAllCustomerVouchers(
  {
    page = 1,
    pageSize = 20,
    voucherKeyword,
    rewardType,
    redeemAtFrom,
    redeemAtTo,
    userKeyword,
  } = {},
  signal,
) {
  const response = await httpClient.get('/customer-vouchers', {
    params: compactParams({
      page,
      pageSize,
      voucherKeyword,
      rewardType,
      redeemAtFrom,
      redeemAtTo,
      userKeyword,
    }),
    signal,
  })

  return response.data
}

export async function getCustomerVoucherDetail(customerVoucherId, signal) {
  const response = await httpClient.get(`/customer-vouchers/${customerVoucherId}`, { signal })
  return response.data.data
}

export async function getCustomerRedeems(
  {
    page = 1,
    pageSize = 20,
    voucherKeyword,
    rewardType,
    redeemAtFrom,
    redeemAtTo,
    campaignName,
    userKeyword,
  } = {},
  signal,
) {
  const response = await httpClient.get('/customer-redeems', {
    params: compactParams({
      page,
      pageSize,
      voucherKeyword,
      rewardType,
      redeemAtFrom,
      redeemAtTo,
      campaignName,
      userKeyword,
    }),
    signal,
  })

  return response.data
}

export async function getCustomerRedeemDetail(voucherRedemptionId, signal) {
  const response = await httpClient.get(`/customer-redeems/${voucherRedemptionId}`, { signal })
  return response.data.data
}

function compactParams(params) {
  return Object.fromEntries(
    Object.entries(params).flatMap(([key, value]) => {
      if (typeof value === 'string') {
        const normalizedValue = value.trim()
        return normalizedValue ? [[key, normalizedValue]] : []
      }

      return value === null || value === undefined ? [] : [[key, value]]
    }),
  )
}
