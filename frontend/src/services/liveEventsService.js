import apiClient from './apiClient';

// Live Shopping Events — GET /api/live-events (paged), GET /api/live-events/{id},
// plus authenticated interactions (comment / react / buy).
export const liveEventsService = {
  list: (params) => apiClient.get('/live-events', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/live-events/${id}`).then((res) => res.data),
  addComment: (id, body) => apiClient.post(`/live-events/${id}/comments`, { body }).then((res) => res.data),
  addReaction: (id, type) => apiClient.post(`/live-events/${id}/reactions`, { type }).then((res) => res.data),
  buyDuringLive: (id, payload) => apiClient.post(`/live-events/${id}/buy`, payload).then((res) => res.data),
};
