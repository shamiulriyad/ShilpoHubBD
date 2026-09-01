import apiClient from './apiClient';

export const complaintsMonitoringService = {
  listComplaints: (params) => apiClient.get('/governance/complaints', { params }).then((res) => res.data),
  getComplaint: (id) => apiClient.get(`/governance/complaints/${id}`).then((res) => res.data),
  createComplaint: (payload) => apiClient.post('/governance/complaints', payload).then((res) => res.data),
  addComplaintUpdate: (id, payload) => apiClient.post(`/governance/complaints/${id}/updates`, payload).then((res) => res.data),
  assignComplaint: (id, payload) => apiClient.post(`/governance/complaints/${id}/assign`, payload).then((res) => res.data),
  resolveComplaint: (id, payload) => apiClient.post(`/governance/complaints/${id}/resolve`, payload).then((res) => res.data),
  removeComplaint: (id) => apiClient.delete(`/governance/complaints/${id}`).then((res) => res.data),

  runMonitoringScan: (payload) => apiClient.post('/governance/monitoring/scans', payload).then((res) => res.data),
  listMonitoringFlags: (params) => apiClient.get('/governance/monitoring/flags', { params }).then((res) => res.data),
  getMonitoringFlag: (id) => apiClient.get(`/governance/monitoring/flags/${id}`).then((res) => res.data),
  createMonitoringFlag: (payload) => apiClient.post('/governance/monitoring/flags', payload).then((res) => res.data),
  updateMonitoringFlagStatus: (id, payload) => apiClient.post(`/governance/monitoring/flags/${id}/status`, payload).then((res) => res.data),
  removeMonitoringFlag: (id) => apiClient.delete(`/governance/monitoring/flags/${id}`).then((res) => res.data),
  getQrOverview: (params) => apiClient.get('/governance/monitoring/qr/overview', { params }).then((res) => res.data),
};
