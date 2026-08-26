import apiClient from './apiClient';

export const enrollmentsService = {
  enroll: (courseId) => apiClient.post(`/enrollments/courses/${courseId}/enroll`).then((res) => res.data),
  mine: () => apiClient.get('/enrollments/mine').then((res) => res.data),
  getById: (id) => apiClient.get(`/enrollments/${id}`).then((res) => res.data),
  markProgress: (id, lessonId, isCompleted = true) =>
    apiClient.post(`/enrollments/${id}/progress`, { lessonId, isCompleted }).then((res) => res.data),
  complete: (id) => apiClient.post(`/enrollments/${id}/complete`).then((res) => res.data),
};
