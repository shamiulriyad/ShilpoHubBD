import apiClient from './apiClient';

export const impactService = {
  mine: () => apiClient.get('/impact/mine').then((res) => res.data),
};
