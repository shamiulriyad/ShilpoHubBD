import apiClient from './apiClient';

export const govReportsService = {
  listReports: (params) => apiClient.get('/governance/reports', { params }).then((res) => res.data),
  getReport: (id) => apiClient.get(`/governance/reports/${id}`).then((res) => res.data),
  generateReport: (payload) => apiClient.post('/governance/reports/generate', payload).then((res) => res.data),
  updateReport: (id, payload) => apiClient.put(`/governance/reports/${id}`, payload).then((res) => res.data),
  removeReport: (id) => apiClient.delete(`/governance/reports/${id}`).then((res) => res.data),

  listForecasts: (params) => apiClient.get('/governance/forecasts', { params }).then((res) => res.data),
  getForecast: (id) => apiClient.get(`/governance/forecasts/${id}`).then((res) => res.data),
  generateForecast: (payload) => apiClient.post('/governance/forecasts/generate', payload).then((res) => res.data),
  removeForecast: (id) => apiClient.delete(`/governance/forecasts/${id}`).then((res) => res.data),
};
