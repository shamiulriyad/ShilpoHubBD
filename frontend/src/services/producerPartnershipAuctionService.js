import apiClient from './apiClient';

export const producerPartnershipAuctionService = {
  // Auction — admin configures/runs it; Business Partners and admin can read it.
  create: (payload) => apiClient.post('/producer-partnership-auctions', payload).then((res) => res.data),
  list: (params) => apiClient.get('/producer-partnership-auctions', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/producer-partnership-auctions/${id}`).then((res) => res.data),
  update: (id, payload) => apiClient.put(`/producer-partnership-auctions/${id}`, payload).then((res) => res.data),
  schedule: (id) => apiClient.post(`/producer-partnership-auctions/${id}/schedule`).then((res) => res.data),
  openRegistration: (id) => apiClient.post(`/producer-partnership-auctions/${id}/open-registration`).then((res) => res.data),
  goLive: (id) => apiClient.post(`/producer-partnership-auctions/${id}/go-live`).then((res) => res.data),
  end: (id) => apiClient.post(`/producer-partnership-auctions/${id}/end`).then((res) => res.data),
  cancel: (id) => apiClient.post(`/producer-partnership-auctions/${id}/cancel`).then((res) => res.data),

  // Lots — producers entered into the auction round.
  addLot: (auctionId, producerId) => apiClient.post(`/producer-partnership-auctions/${auctionId}/lots`, { producerId }).then((res) => res.data),
  removeLot: (auctionId, lotId) => apiClient.delete(`/producer-partnership-auctions/${auctionId}/lots/${lotId}`).then((res) => res.data),
  getLots: (auctionId) => apiClient.get(`/producer-partnership-auctions/${auctionId}/lots`).then((res) => res.data),
  getLot: (auctionId, lotId) => apiClient.get(`/producer-partnership-auctions/${auctionId}/lots/${lotId}`).then((res) => res.data),

  // Bids
  placeBid: (auctionId, lotId, amount) =>
    apiClient.post(`/producer-partnership-auctions/${auctionId}/lots/${lotId}/bids`, { amount }).then((res) => res.data),
  getBidHistory: (auctionId, lotId) => apiClient.get(`/producer-partnership-auctions/${auctionId}/lots/${lotId}/bids`).then((res) => res.data),
  getMyBids: (auctionId, lotId) =>
    apiClient.get(`/producer-partnership-auctions/${auctionId}/my-bids`, { params: lotId ? { lotId } : undefined }).then((res) => res.data),

  // Participants — Business Partner registration, subject to admin approval.
  apply: (auctionId) => apiClient.post(`/producer-partnership-auctions/${auctionId}/participants`).then((res) => res.data),
  getMineAsParticipant: (auctionId) => apiClient.get(`/producer-partnership-auctions/${auctionId}/participants/me`).then((res) => res.data),
  getParticipants: (auctionId) => apiClient.get(`/producer-partnership-auctions/${auctionId}/participants`).then((res) => res.data),
  decideParticipant: (auctionId, participantId, approve, notes) =>
    apiClient.post(`/producer-partnership-auctions/${auctionId}/participants/${participantId}/decide`, { approve, notes }).then((res) => res.data),
};
