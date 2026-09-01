import apiClient from './apiClient';

export const mentorMatchingService = {
  match: (payload) => apiClient.post('/mentor-matching/match', payload).then((res) => res.data),
};
