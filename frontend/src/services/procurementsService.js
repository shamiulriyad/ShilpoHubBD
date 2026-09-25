import apiClient from './apiClient';

export const procurementsService = {
  // Business partner
  create: (payload) => apiClient.post('/procurements', payload).then((res) => res.data),
  createFromQuotation: (quotationResponseId, payload) =>
    apiClient.post(`/procurements/from-quotation/${quotationResponseId}`, payload).then((res) => res.data),
  mine: (params) => apiClient.get('/procurements', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/procurements/${id}`).then((res) => res.data),
  payAdvance: (id, payload) => apiClient.post(`/procurements/${id}/advance`, payload).then((res) => res.data),
  convertToOrder: (id) => apiClient.post(`/procurements/${id}/convert-to-order`).then((res) => res.data),
  cancel: (id) => apiClient.post(`/procurements/${id}/cancel`).then((res) => res.data),

  // Selected producer accepts or declines
  incoming: (params) => apiClient.get('/producer/procurements', { params }).then((res) => res.data),
  approve: (id, notes) => apiClient.post(`/producer/procurements/${id}/approve`, { notes }).then((res) => res.data),
  reject: (id, notes) => apiClient.post(`/producer/procurements/${id}/reject`, { notes }).then((res) => res.data),

  // Admin inspection after the advance
  inspections: (params) => apiClient.get('/admin/procurements/inspections', { params }).then((res) => res.data),
  inspect: (id, payload) => apiClient.post(`/admin/procurements/${id}/inspect`, payload).then((res) => res.data),
};
