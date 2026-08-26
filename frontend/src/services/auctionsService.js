import apiClient from './apiClient';

export const auctionsService = {
  list: (params) => apiClient.get('/auctions', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/auctions/${id}`).then((res) => res.data),
  placeBid: (id, amount) => apiClient.post(`/auctions/${id}/bids`, { amount }).then((res) => res.data),
};
