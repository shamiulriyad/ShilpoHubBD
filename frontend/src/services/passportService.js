import apiClient from './apiClient';

export const passportService = {
  listAllBadges: () => apiClient.get('/passport/badges').then((res) => res.data),
  myBadges: () => apiClient.get('/passport/badges/mine').then((res) => res.data),
  claimDistrictBadge: (districtId) =>
    apiClient.post('/passport/badges/claim/district', { districtId }).then((res) => res.data),
  claimFestivalBadge: (badgeId) =>
    apiClient.post('/passport/badges/claim/festival', { badgeId }).then((res) => res.data),
  evaluatePurchaseBadges: () => apiClient.post('/passport/badges/evaluate-purchases').then((res) => res.data),
  checkIn: (payload) => apiClient.post('/passport/checkins', payload).then((res) => res.data),
  myCheckIns: () => apiClient.get('/passport/checkins/mine').then((res) => res.data),
  addJournalEntry: (payload) => apiClient.post('/passport/journal', payload).then((res) => res.data),
  myJournal: () => apiClient.get('/passport/journal/mine').then((res) => res.data),
  updateJournalEntry: (id, payload) => apiClient.put(`/passport/journal/${id}`, payload).then((res) => res.data),
  deleteJournalEntry: (id) => apiClient.delete(`/passport/journal/${id}`),
};
