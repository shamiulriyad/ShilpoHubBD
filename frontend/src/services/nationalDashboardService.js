import apiClient from './apiClient';

const base = '/governance/dashboard';

export const nationalDashboardService = {
  getOverview: (params) => apiClient.get(`${base}/overview`, { params }).then((res) => res.data),
  getDistrictRankings: (params) => apiClient.get(`${base}/district-rankings`, { params }).then((res) => res.data),
  listSnapshots: (params) => apiClient.get(`${base}/snapshots`, { params }).then((res) => res.data),
  getSnapshot: (id) => apiClient.get(`${base}/snapshots/${id}`).then((res) => res.data),
  captureSnapshot: (payload) => apiClient.post(`${base}/snapshots`, payload).then((res) => res.data),
  removeSnapshot: (id) => apiClient.delete(`${base}/snapshots/${id}`).then((res) => res.data),
  getTrend: (params) => apiClient.get(`${base}/trends`, { params }).then((res) => res.data),
};
