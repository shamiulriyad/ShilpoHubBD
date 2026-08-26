import apiClient from './apiClient';

export const quotationsService = {
  create: (payload) => apiClient.post('/quotations', payload).then((res) => res.data),
  mine: (params) => apiClient.get('/quotations', { params }).then((res) => res.data),
  received: (params) => apiClient.get('/quotations/received', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/quotations/${id}`).then((res) => res.data),
  compare: (id) => apiClient.get(`/quotations/${id}/compare`).then((res) => res.data),
  submitResponse: (id, payload) => apiClient.post(`/quotations/${id}/responses`, payload).then((res) => res.data),
  decideResponse: (id, responseId, payload) =>
    apiClient.post(`/quotations/${id}/responses/${responseId}/decision`, payload).then((res) => res.data),
  cancel: (id) => apiClient.post(`/quotations/${id}/cancel`).then((res) => res.data),
};
