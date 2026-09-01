import apiClient from './apiClient';

const base = '/governance/heritage-intelligence';

export const heritageIntelligenceService = {
  compute: (payload) => apiClient.post(`${base}/compute`, payload).then((res) => res.data),
  listRecords: (params) => apiClient.get(`${base}/records`, { params }).then((res) => res.data),
  getTrend: (params) => apiClient.get(`${base}/trends`, { params }).then((res) => res.data),
};
