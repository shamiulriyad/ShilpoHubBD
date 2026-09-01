import apiClient from './apiClient';

export const apprenticeEnrollmentsService = {
  getMine: () => apiClient.get('/apprentice-enrollments/mine').then((res) => res.data),
  getById: (id) => apiClient.get(`/apprentice-enrollments/${id}`).then((res) => res.data),
  getByProgram: (programId) => apiClient.get(`/apprentice-enrollments/programs/${programId}`).then((res) => res.data),
  updateMilestoneProgress: (id, milestoneId, payload) =>
    apiClient.post(`/apprentice-enrollments/${id}/milestones/${milestoneId}/progress`, payload).then((res) => res.data),
  complete: (id) => apiClient.post(`/apprentice-enrollments/${id}/complete`, {}).then((res) => res.data),
};
