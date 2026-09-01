import apiClient from './apiClient';

export const policyComplianceService = {
  runSimulation: (payload) => apiClient.post('/governance/policy-simulator/simulations', payload).then((res) => res.data),
  listSimulations: (params) => apiClient.get('/governance/policy-simulator/simulations', { params }).then((res) => res.data),
  getSimulation: (id) => apiClient.get(`/governance/policy-simulator/simulations/${id}`).then((res) => res.data),
  removeSimulation: (id) => apiClient.delete(`/governance/policy-simulator/simulations/${id}`).then((res) => res.data),

  listComplianceRecords: (params) => apiClient.get('/governance/compliance/records', { params }).then((res) => res.data),
  getComplianceRecord: (id) => apiClient.get(`/governance/compliance/records/${id}`).then((res) => res.data),
  createComplianceRecord: (payload) => apiClient.post('/governance/compliance/records', payload).then((res) => res.data),
  updateComplianceRecord: (id, payload) => apiClient.put(`/governance/compliance/records/${id}`, payload).then((res) => res.data),
  upsertRequirement: (id, payload) => apiClient.put(`/governance/compliance/records/${id}/requirements`, payload).then((res) => res.data),
  removeRequirement: (id, requirementId) =>
    apiClient.delete(`/governance/compliance/records/${id}/requirements/${requirementId}`).then((res) => res.data),
  removeComplianceRecord: (id) => apiClient.delete(`/governance/compliance/records/${id}`).then((res) => res.data),
};
