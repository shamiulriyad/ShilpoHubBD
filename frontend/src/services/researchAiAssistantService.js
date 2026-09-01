import apiClient from './apiClient';

const base = (projectId) => `/research/projects/${projectId}/ai`;

export const researchAiAssistantService = {
  runInsights: (projectId, payload) => apiClient.post(`${base(projectId)}/insights`, payload).then((res) => res.data),
  runTrends: (projectId, payload) => apiClient.post(`${base(projectId)}/trends`, payload).then((res) => res.data),
  runCorrelations: (projectId, payload) => apiClient.post(`${base(projectId)}/correlations`, payload).then((res) => res.data),
  runReport: (projectId, payload) => apiClient.post(`${base(projectId)}/report`, payload).then((res) => res.data),
  generateCitations: (projectId, payload) => apiClient.post(`${base(projectId)}/citations`, payload).then((res) => res.data),
  listAnalyses: (projectId, params) => apiClient.get(`${base(projectId)}/analyses`, { params }).then((res) => res.data),
  getAnalysis: (projectId, analysisId) => apiClient.get(`${base(projectId)}/analyses/${analysisId}`).then((res) => res.data),
  removeAnalysis: (projectId, analysisId) => apiClient.delete(`${base(projectId)}/analyses/${analysisId}`).then((res) => res.data),
};
