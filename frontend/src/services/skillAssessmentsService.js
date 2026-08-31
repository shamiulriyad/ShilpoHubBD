import apiClient from './apiClient';

export const skillAssessmentsService = {
  run: (heritageSkillId) => apiClient.post(`/skill-assessments/skills/${heritageSkillId}/run`).then((res) => res.data),
  getById: (id) => apiClient.get(`/skill-assessments/${id}`).then((res) => res.data),
  history: () => apiClient.get('/skill-assessments/history').then((res) => res.data),
};
