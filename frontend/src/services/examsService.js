import apiClient from './apiClient';

export const examsService = {
  listForCourse: (courseId) => apiClient.get(`/exams/courses/${courseId}`).then((res) => res.data),
  getById: (id) => apiClient.get(`/exams/${id}`).then((res) => res.data),
  startAttempt: (id) => apiClient.post(`/exams/${id}/attempts/start`).then((res) => res.data),
  submitAttempt: (attemptId, answers) =>
    apiClient.post(`/exams/attempts/${attemptId}/submit`, { answers }).then((res) => res.data),
  getAttempt: (attemptId) => apiClient.get(`/exams/attempts/${attemptId}`).then((res) => res.data),
  myAttempts: (examId) => apiClient.get(`/exams/${examId}/attempts/mine`).then((res) => res.data),
};
