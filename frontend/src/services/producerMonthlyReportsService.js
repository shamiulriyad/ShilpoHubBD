import apiClient from './apiClient';

export const producerMonthlyReportsService = {
  intelligenceList: (params) =>
    apiClient.get('/admin/producer-monthly-reports/intelligence', { params }).then((res) => res.data),
  intelligenceDashboard: (params) =>
    apiClient.get('/admin/producer-monthly-reports/intelligence/dashboard', { params }).then((res) => res.data),
  history: (producerId, params) =>
    apiClient
      .get('/admin/producer-monthly-reports', { params: { ...params, producerId } })
      .then((res) => res.data),
  compare: (producerId, params) =>
    apiClient
      .get(`/admin/producer-monthly-reports/producer/${producerId}/compare`, { params })
      .then((res) => res.data),
};
