import apiClient from './apiClient';

export const assignmentsService = {
  listForCourse: (courseId) => apiClient.get(`/assignments/courses/${courseId}`).then((res) => res.data),
  getById: (id) => apiClient.get(`/assignments/${id}`).then((res) => res.data),
  submit: (id, payload) => apiClient.post(`/assignments/${id}/submit`, payload).then((res) => res.data),
  mySubmission: (id) => apiClient.get(`/assignments/${id}/my-submission`).then((res) => res.data),
};
