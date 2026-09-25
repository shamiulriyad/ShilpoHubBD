import apiClient from './apiClient';

export const orderComplaintsService = {
  create: (payload) => apiClient.post('/order-complaints', payload).then((res) => res.data),
  mine: () => apiClient.get('/order-complaints/mine').then((res) => res.data),
  received: () => apiClient.get('/order-complaints/received').then((res) => res.data),
  respond: (id, message) => apiClient.post(`/order-complaints/${id}/respond`, { message }).then((res) => res.data),
  satisfied: (id, note) => apiClient.post(`/order-complaints/${id}/satisfied`, { note }).then((res) => res.data),
  reopen: (id, note) => apiClient.post(`/order-complaints/${id}/reopen`, { note }).then((res) => res.data),
  withdraw: (id) => apiClient.post(`/order-complaints/${id}/withdraw`).then((res) => res.data),
};
