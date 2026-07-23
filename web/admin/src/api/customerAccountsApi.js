import httpClient from './httpClient'

export async function getCustomerAccounts(
  { page = 1, pageSize = 20, keyword, status, tierId } = {},
  signal,
) {
  const response = await httpClient.get('/customer-users', {
    params: {
      page,
      pageSize,
      keyword: keyword || undefined,
      status: status || undefined,
      tierId: tierId || undefined,
    },
    signal,
  })

  return response.data
}

export async function getCustomerAccount(customerId, signal) {
  const response = await httpClient.get(`/customer-users/${customerId}`, { signal })
  return response.data.data
}

export async function updateCustomerAccount(customerId, payload) {
  const response = await httpClient.put(`/customer-users/${customerId}`, payload)
  return response.data.data
}

export async function updateCustomerAccountStatus(customerId, status) {
  await httpClient.patch(`/customer-users/${customerId}/status`, { status })
}

export async function getCustomerAccountPoints(customerId, signal) {
  const response = await httpClient.get(`/customer-users/${customerId}/points`, { signal })
  return response.data.data
}

export async function getCustomerVouchers(customerId, { page = 1, pageSize = 20 } = {}, signal) {
  const response = await httpClient.get(`/customer-users/${customerId}/vouchers`, {
    params: { page, pageSize },
    signal,
  })
  return response.data
}

export async function getCustomerVoucherRedemptions(customerId, { page = 1, pageSize = 20 } = {}, signal) {
  const response = await httpClient.get(`/customer-users/${customerId}/voucher-redemptions`, {
    params: { page, pageSize },
    signal,
  })
  return response.data
}

export async function getCustomerPointTransactions(customerId, { page = 1, pageSize = 20 } = {}, signal) {
  const response = await httpClient.get(`/customer-users/${customerId}/point-transactions`, {
    params: { page, pageSize },
    signal,
  })
  return response.data
}



