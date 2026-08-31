import apiClient from './apiClient';

// Public publication repository — GET /api/research/publications (paged), requires auth.
export const researchPublicationsService = {
  browse: (params) => apiClient.get('/research/publications', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/research/publications/${id}`).then((res) => res.data),
};
