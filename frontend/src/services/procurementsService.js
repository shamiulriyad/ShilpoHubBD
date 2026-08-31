import apiClient from './apiClient';

export const procurementsService = {
  create: (payload) => apiClient.post('/procurements', payload).then((res) => res.data),
  createFromQuotation: (quotationResponseId, payload) =>
    apiClient.post(`/procurements/from-quotation/${quotationResponseId}`, payload).then((res) => res.data),
  mine: (params) => apiClient.get('/procurements', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/procurements/${id}`).then((res) => res.data),
  approve: (id, notes) => apiClient.post(`/procurements/${id}/approve`, { notes }).then((res) => res.data),
  reject: (id, notes) => apiClient.post(`/procurements/${id}/reject`, { notes }).then((res) => res.data),
  convertToOrder: (id) => apiClient.post(`/procurements/${id}/convert-to-order`).then((res) => res.data),
  cancel: (id) => apiClient.post(`/procurements/${id}/cancel`).then((res) => res.data),
};
