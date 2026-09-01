import apiClient from './apiClient';

export const knowledgeGraphService = {
  listNodes: (params) => apiClient.get('/knowledge-graph/nodes', { params }).then((res) => res.data),
  getNode: (id) => apiClient.get(`/knowledge-graph/nodes/${id}`).then((res) => res.data),
  createNode: (payload) => apiClient.post('/knowledge-graph/nodes', payload).then((res) => res.data),
  updateNode: (id, payload) => apiClient.put(`/knowledge-graph/nodes/${id}`, payload).then((res) => res.data),
  removeNode: (id) => apiClient.delete(`/knowledge-graph/nodes/${id}`).then((res) => res.data),
  getNeighbors: (id) => apiClient.get(`/knowledge-graph/nodes/${id}/neighbors`).then((res) => res.data),
  traverse: (id, params) => apiClient.get(`/knowledge-graph/nodes/${id}/traverse`, { params }).then((res) => res.data),

  listRelationships: (params) => apiClient.get('/knowledge-graph/relationships', { params }).then((res) => res.data),
  createRelationship: (payload) => apiClient.post('/knowledge-graph/relationships', payload).then((res) => res.data),
  removeRelationship: (id) => apiClient.delete(`/knowledge-graph/relationships/${id}`).then((res) => res.data),

  findPath: (params) => apiClient.get('/knowledge-graph/paths', { params }).then((res) => res.data),
};
