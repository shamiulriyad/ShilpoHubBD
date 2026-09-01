import apiClient from './apiClient';

export const fieldResearchService = {
  // Surveys
  listSurveys: (params) => apiClient.get('/field-research/surveys', { params }).then((res) => res.data),
  getSurvey: (id) => apiClient.get(`/field-research/surveys/${id}`).then((res) => res.data),
  createSurvey: (payload) => apiClient.post('/field-research/surveys', payload).then((res) => res.data),
  updateSurveyStatus: (id, payload) => apiClient.put(`/field-research/surveys/${id}/status`, payload).then((res) => res.data),
  removeSurvey: (id) => apiClient.delete(`/field-research/surveys/${id}`).then((res) => res.data),
  addQuestion: (id, payload) => apiClient.post(`/field-research/surveys/${id}/questions`, payload).then((res) => res.data),
  removeQuestion: (id, questionId) => apiClient.delete(`/field-research/surveys/${id}/questions/${questionId}`).then((res) => res.data),
  assignFieldResearcher: (id, payload) => apiClient.post(`/field-research/surveys/${id}/field-researchers`, payload).then((res) => res.data),
  removeFieldResearcher: (id, assignmentId) =>
    apiClient.delete(`/field-research/surveys/${id}/field-researchers/${assignmentId}`).then((res) => res.data),

  // Responses
  listResponses: (surveyId, params) => apiClient.get(`/field-research/surveys/${surveyId}/responses`, { params }).then((res) => res.data),
  reviewResponse: (surveyId, responseId, payload) =>
    apiClient.post(`/field-research/surveys/${surveyId}/responses/${responseId}/review`, payload).then((res) => res.data),

  // Evidence
  listEvidence: (surveyId, params) => apiClient.get(`/field-research/surveys/${surveyId}/evidence`, { params }).then((res) => res.data),
  createEvidence: (surveyId, payload) => apiClient.post(`/field-research/surveys/${surveyId}/evidence`, payload).then((res) => res.data),
  removeEvidence: (surveyId, evidenceId) =>
    apiClient.delete(`/field-research/surveys/${surveyId}/evidence/${evidenceId}`).then((res) => res.data),
};
