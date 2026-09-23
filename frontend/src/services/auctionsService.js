import apiClient from './apiClient';

export const auctionsService = {
  list: (params) => apiClient.get('/auctions', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/auctions/${id}`).then((res) => res.data),
  // Producer side: my auctions (any status), create one, cancel one.
  mine: (params) => apiClient.get('/auctions/mine', { params }).then((res) => res.data),
  create: (payload) => apiClient.post('/auctions', payload).then((res) => res.data),
  cancel: (id) => apiClient.post(`/auctions/${id}/cancel`).then((res) => res.data),
  placeBid: (id, amount) => apiClient.post(`/auctions/${id}/bids`, { amount }).then((res) => res.data),
};
