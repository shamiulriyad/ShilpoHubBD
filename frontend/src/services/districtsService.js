import apiClient from './apiClient';

export const districtsService = {
  list: () => apiClient.get('/districts').then((res) => res.data),
};
