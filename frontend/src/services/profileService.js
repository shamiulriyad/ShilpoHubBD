import apiClient from './apiClient';

export const profileService = {
  getMine: () => apiClient.get('/profile/me').then((res) => res.data),
  expertiseOptions: () => apiClient.get('/profile/expertise-options').then((res) => res.data),
  uploadPhoto: (file) => { const form = new FormData(); form.append('file', file); return apiClient.post('/profile/photo', form).then((res) => res.data); },
  removePhoto: () => apiClient.delete('/profile/photo').then((res) => res.data),
  saveMine: (payload) => apiClient.put('/profile/me', payload).then((res) => res.data),

  // Admin review
  listForAdmin: (params) => apiClient.get('/admin/profiles', { params }).then((res) => res.data),
  approve: (id, notes) => apiClient.post(`/admin/profiles/${id}/approve`, { notes }).then((res) => res.data),
  reject: (id, notes) => apiClient.post(`/admin/profiles/${id}/reject`, { notes }).then((res) => res.data),
};
