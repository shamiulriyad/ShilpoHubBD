import apiClient from './apiClient';

export const academyMemberProfilesService = {
  create: (payload) => apiClient.post('/academy-profiles', payload).then((res) => res.data),
  me: () => apiClient.get('/academy-profiles/me').then((res) => res.data),
  updateMe: (payload) => apiClient.put('/academy-profiles/me', payload).then((res) => res.data),
  addSkill: (heritageSkillId, level = 'Beginner') =>
    apiClient.post('/academy-profiles/me/skills', { heritageSkillId, level }).then((res) => res.data),
  removeSkill: (heritageSkillId) => apiClient.delete(`/academy-profiles/me/skills/${heritageSkillId}`).then((res) => res.data),
  learningHistory: () => apiClient.get('/academy-profiles/me/learning-history').then((res) => res.data),
};
