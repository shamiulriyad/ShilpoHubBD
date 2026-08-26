import apiClient from './apiClient';

export const culturalEventsService = {
  list: (params) => apiClient.get('/cultural-events', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/cultural-events/${id}`).then((res) => res.data),
};
