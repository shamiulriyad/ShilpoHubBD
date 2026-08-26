import apiClient from './apiClient';

export const heritageSkillsService = {
  list: (activeOnly = true) => apiClient.get('/heritage-skills', { params: { activeOnly } }).then((res) => res.data),
};
