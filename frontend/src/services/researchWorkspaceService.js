import apiClient from './apiClient';

export const researchWorkspaceService = {
  // Projects
  listProjects: (params) => apiClient.get('/research/projects', { params }).then((res) => res.data),
  getProject: (id) => apiClient.get(`/research/projects/${id}`).then((res) => res.data),
  createProject: (payload) => apiClient.post('/research/projects', payload).then((res) => res.data),
  updateProject: (id, payload) => apiClient.put(`/research/projects/${id}`, payload).then((res) => res.data),
  updateProjectStatus: (id, payload) => apiClient.put(`/research/projects/${id}/status`, payload).then((res) => res.data),
  removeProject: (id) => apiClient.delete(`/research/projects/${id}`).then((res) => res.data),
  addMember: (id, payload) => apiClient.post(`/research/projects/${id}/members`, payload).then((res) => res.data),
  removeMember: (id, memberId) => apiClient.delete(`/research/projects/${id}/members/${memberId}`).then((res) => res.data),
  getActivity: (id, take = 50) => apiClient.get(`/research/projects/${id}/activity`, { params: { take } }).then((res) => res.data),

  // Papers
  listPapers: (projectId) => apiClient.get(`/research/projects/${projectId}/papers`).then((res) => res.data),
  createPaper: (projectId, payload) => apiClient.post(`/research/projects/${projectId}/papers`, payload).then((res) => res.data),
  updatePaper: (projectId, paperId, payload) =>
    apiClient.put(`/research/projects/${projectId}/papers/${paperId}`, payload).then((res) => res.data),
  removePaper: (projectId, paperId) =>
    apiClient.delete(`/research/projects/${projectId}/papers/${paperId}`).then((res) => res.data),

  // Notes
  listNotes: (projectId) => apiClient.get(`/research/projects/${projectId}/notes`).then((res) => res.data),
  createNote: (projectId, payload) => apiClient.post(`/research/projects/${projectId}/notes`, payload).then((res) => res.data),
  updateNote: (projectId, noteId, payload) =>
    apiClient.put(`/research/projects/${projectId}/notes/${noteId}`, payload).then((res) => res.data),
  removeNote: (projectId, noteId) =>
    apiClient.delete(`/research/projects/${projectId}/notes/${noteId}`).then((res) => res.data),

  // Tasks
  listTasks: (projectId, params) => apiClient.get(`/research/projects/${projectId}/tasks`, { params }).then((res) => res.data),
  createTask: (projectId, payload) => apiClient.post(`/research/projects/${projectId}/tasks`, payload).then((res) => res.data),
  updateTask: (projectId, taskId, payload) =>
    apiClient.put(`/research/projects/${projectId}/tasks/${taskId}`, payload).then((res) => res.data),
  updateTaskStatus: (projectId, taskId, payload) =>
    apiClient.put(`/research/projects/${projectId}/tasks/${taskId}/status`, payload).then((res) => res.data),
  removeTask: (projectId, taskId) =>
    apiClient.delete(`/research/projects/${projectId}/tasks/${taskId}`).then((res) => res.data),

  // Milestones
  listMilestones: (projectId) => apiClient.get(`/research/projects/${projectId}/milestones`).then((res) => res.data),
  createMilestone: (projectId, payload) => apiClient.post(`/research/projects/${projectId}/milestones`, payload).then((res) => res.data),
  updateMilestone: (projectId, milestoneId, payload) =>
    apiClient.put(`/research/projects/${projectId}/milestones/${milestoneId}`, payload).then((res) => res.data),
  removeMilestone: (projectId, milestoneId) =>
    apiClient.delete(`/research/projects/${projectId}/milestones/${milestoneId}`).then((res) => res.data),
};
