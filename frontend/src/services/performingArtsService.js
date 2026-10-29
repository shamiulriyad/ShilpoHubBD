import apiClient from './apiClient';

const enc = encodeURIComponent;

// Public discovery — GET /api/performing-arts (Published records only).
export const performingArtsService = {
  list: (params, signal) => apiClient.get('/performing-arts', { params, signal }).then((res) => res.data),
  featured: (count = 6) => apiClient.get('/performing-arts/featured', { params: { count } }).then((res) => res.data),
  facets: () => apiClient.get('/performing-arts/facets').then((res) => res.data),
  getBySlug: (slug) => apiClient.get(`/performing-arts/${enc(slug)}`).then((res) => res.data),
};

// Super Admin editorial workflow — /api/admin/performing-arts.
export const performingArtsAdminService = {
  list: (params) => apiClient.get('/admin/performing-arts', { params }).then((res) => res.data),
  get: (id) => apiClient.get(`/admin/performing-arts/${enc(id)}`).then((res) => res.data),
  save: (id, payload) => (id
    ? apiClient.put(`/admin/performing-arts/${enc(id)}`, payload)
    : apiClient.post('/admin/performing-arts', payload)).then((res) => res.data),
  transition: (id, action, note) => apiClient.post(`/admin/performing-arts/${enc(id)}/${action}`, { note: note || null }).then((res) => res.data),
  remove: (id) => apiClient.delete(`/admin/performing-arts/${enc(id)}`),
};
