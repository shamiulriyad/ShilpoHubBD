import apiClient from './apiClient';

export const productDevelopmentService = {
  create: (payload) => apiClient.post('/product-development', payload).then((res) => res.data),
  mine: (params) => apiClient.get('/product-development', { params }).then((res) => res.data),
  received: (params) => apiClient.get('/product-development/received', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/product-development/${id}`).then((res) => res.data),
  respond: (id, accept) => apiClient.post(`/product-development/${id}/respond`, { accept }).then((res) => res.data),
  addComment: (id, content) => apiClient.post(`/product-development/${id}/comments`, { content }).then((res) => res.data),
  addMilestone: (id, payload) => apiClient.post(`/product-development/${id}/milestones`, payload).then((res) => res.data),
  updateMilestoneStatus: (id, milestoneId, status) =>
    apiClient.post(`/product-development/${id}/milestones/${milestoneId}/status`, { status }).then((res) => res.data),
  submitPrototype: (id, payload) => apiClient.post(`/product-development/${id}/prototypes`, payload).then((res) => res.data),
  decidePrototype: (id, prototypeVersionId, payload) =>
    apiClient.post(`/product-development/${id}/prototypes/${prototypeVersionId}/decision`, payload).then((res) => res.data),
  convertToProduct: (id, payload) => apiClient.post(`/product-development/${id}/convert-to-product`, payload).then((res) => res.data),
  cancel: (id) => apiClient.post(`/product-development/${id}/cancel`).then((res) => res.data),
};
