import apiClient from './apiClient';

export const producerFollowsService = {
  list: () => apiClient.get('/follows/producers').then((res) => res.data),
  follow: (producerId) => apiClient.post(`/follows/producers/${producerId}`),
  unfollow: (producerId) => apiClient.delete(`/follows/producers/${producerId}`),
};
