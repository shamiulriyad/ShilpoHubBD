import apiClient from './apiClient';

export const heritageDatabaseService = {
  getSummary: (params) => apiClient.get('/heritage-database/live/summary', { params }).then((res) => res.data),
  getLocations: (params) => apiClient.get('/heritage-database/live/locations', { params }).then((res) => res.data),
  getVillages: (params) => apiClient.get('/heritage-database/live/villages', { params }).then((res) => res.data),
  getProducers: (params) => apiClient.get('/heritage-database/live/producers', { params }).then((res) => res.data),

  getProducts: (params) => apiClient.get('/heritage-database/live/products', { params }).then((res) => res.data),
  getTourism: (params) => apiClient.get('/heritage-database/live/tourism', { params }).then((res) => res.data),
  getDemographics: () => apiClient.get('/heritage-database/live/demographics').then((res) => res.data),

  listDatasets: (params) => apiClient.get('/heritage-database/datasets', { params }).then((res) => res.data),
  getDataset: (id) => apiClient.get(`/heritage-database/datasets/${id}`).then((res) => res.data),
  createDataset: (payload) => apiClient.post('/heritage-database/datasets', payload).then((res) => res.data),
  updateDataset: (id, payload) => apiClient.put(`/heritage-database/datasets/${id}`, payload).then((res) => res.data),
  refreshDataset: (id) => apiClient.post(`/heritage-database/datasets/${id}/refresh`, {}).then((res) => res.data),
  removeDataset: (id) => apiClient.delete(`/heritage-database/datasets/${id}`).then((res) => res.data),

  exportDataset: (id, payload) => apiClient.post(`/heritage-database/datasets/${id}/exports`, payload).then((res) => res.data),
  listDatasetExports: (id, params) => apiClient.get(`/heritage-database/datasets/${id}/exports`, { params }).then((res) => res.data),
  getExportAnalytics: (id) => apiClient.get(`/heritage-database/datasets/${id}/export-analytics`).then((res) => res.data),

  listRiskRecords: (params) => apiClient.get('/heritage-database/risk', { params }).then((res) => res.data),
  createRiskRecord: (payload) => apiClient.post('/heritage-database/risk', payload).then((res) => res.data),
  updateRiskRecord: (id, payload) => apiClient.put(`/heritage-database/risk/${id}`, payload).then((res) => res.data),
  removeRiskRecord: (id) => apiClient.delete(`/heritage-database/risk/${id}`).then((res) => res.data),
};
