import apiClient from './apiClient';

export const quizzesService = {
  listForCourse: (courseId) => apiClient.get(`/quizzes/courses/${courseId}`).then((res) => res.data),
  getById: (id) => apiClient.get(`/quizzes/${id}`).then((res) => res.data),
  startAttempt: (id) => apiClient.post(`/quizzes/${id}/attempts/start`).then((res) => res.data),
  submitAttempt: (attemptId, answers) =>
    apiClient.post(`/quizzes/attempts/${attemptId}/submit`, { answers }).then((res) => res.data),
  getAttempt: (attemptId) => apiClient.get(`/quizzes/attempts/${attemptId}`).then((res) => res.data),
  myAttempts: (quizId) => apiClient.get(`/quizzes/${quizId}/attempts/mine`).then((res) => res.data),
};
