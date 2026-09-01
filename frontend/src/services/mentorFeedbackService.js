import apiClient from './apiClient';

export const mentorFeedbackService = {
  submit: (payload) => apiClient.post('/mentor-feedback', payload).then((res) => res.data),
  getMine: () => apiClient.get('/mentor-feedback/mine').then((res) => res.data),
};
