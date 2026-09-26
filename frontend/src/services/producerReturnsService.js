import apiClient from './apiClient';

// Customer return requests for the signed-in producer's orders.
export const producerReturnsService = {
  list: () => apiClient.get('/producer/returns').then((res) => res.data),
  accept: (orderId) => apiClient.post(`/producer/returns/${orderId}/accept`).then((res) => res.data),
  reject: (orderId, note) => apiClient.post(`/producer/returns/${orderId}/reject`, { note }).then((res) => res.data),
};
