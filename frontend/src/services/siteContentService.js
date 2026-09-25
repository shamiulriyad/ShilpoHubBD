import apiClient from './apiClient';

export const siteContentService = {
  list: (params) => apiClient.get('/cms/site-content', { params }).then((res) => res.data),
};

export const craftHeritageService = {
  list: (params) => apiClient.get('/craft-heritage', { params }).then((res) => res.data),
};
