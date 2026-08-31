import apiClient from './apiClient';

export const designCollaborationsService = {
  create: (payload) => apiClient.post('/design-collaborations', payload).then((res) => res.data),
  mine: (params) => apiClient.get('/design-collaborations', { params }).then((res) => res.data),
  received: (params) => apiClient.get('/design-collaborations/received', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/design-collaborations/${id}`).then((res) => res.data),
  respond: (id, accept) => apiClient.post(`/design-collaborations/${id}/respond`, { accept }).then((res) => res.data),
  addComment: (id, content) => apiClient.post(`/design-collaborations/${id}/comments`, { content }).then((res) => res.data),
  addFile: (id, payload) => apiClient.post(`/design-collaborations/${id}/files`, payload).then((res) => res.data),
  submitRevision: (id, payload) => apiClient.post(`/design-collaborations/${id}/revisions`, payload).then((res) => res.data),
  decideRevision: (id, revisionId, payload) =>
    apiClient.post(`/design-collaborations/${id}/revisions/${revisionId}/decision`, payload).then((res) => res.data),
  complete: (id) => apiClient.post(`/design-collaborations/${id}/complete`).then((res) => res.data),
  cancel: (id) => apiClient.post(`/design-collaborations/${id}/cancel`).then((res) => res.data),
};
