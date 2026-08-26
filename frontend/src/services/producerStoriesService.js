import apiClient from './apiClient';

export const producerStoriesService = {
  getByProducer: (producerId) => apiClient.get(`/producer-stories/${producerId}`).then((res) => res.data),
};
