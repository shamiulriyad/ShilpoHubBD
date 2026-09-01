import apiClient from './apiClient';

export const jobBoardService = {
  listListings: (params) => apiClient.get('/job-listings', { params }).then((res) => res.data),
  getListing: (id) => apiClient.get(`/job-listings/${id}`).then((res) => res.data),
  getMyListings: () => apiClient.get('/job-listings/mine').then((res) => res.data),
  createListing: (payload) => apiClient.post('/job-listings', payload).then((res) => res.data),
  updateListing: (id, payload) => apiClient.put(`/job-listings/${id}`, payload).then((res) => res.data),
  publishListing: (id) => apiClient.post(`/job-listings/${id}/publish`, {}).then((res) => res.data),
  closeListing: (id) => apiClient.post(`/job-listings/${id}/close`, {}).then((res) => res.data),
  removeListing: (id) => apiClient.delete(`/job-listings/${id}`).then((res) => res.data),
  addSkillRequirement: (id, payload) => apiClient.post(`/job-listings/${id}/skill-requirements`, payload).then((res) => res.data),
  removeSkillRequirement: (id, requirementId) =>
    apiClient.delete(`/job-listings/${id}/skill-requirements/${requirementId}`).then((res) => res.data),

  apply: (payload) => apiClient.post('/job-applications', payload).then((res) => res.data),
  getMyApplications: () => apiClient.get('/job-applications/mine').then((res) => res.data),
  getApplicationsForListing: (jobListingId) => apiClient.get(`/job-applications/job-listings/${jobListingId}`).then((res) => res.data),
  shortlistApplication: (id, payload) => apiClient.post(`/job-applications/${id}/shortlist`, payload).then((res) => res.data),
  rejectApplication: (id, payload) => apiClient.post(`/job-applications/${id}/reject`, payload).then((res) => res.data),
  hireApplication: (id, payload) => apiClient.post(`/job-applications/${id}/hire`, payload).then((res) => res.data),
  withdrawApplication: (id) => apiClient.post(`/job-applications/${id}/withdraw`, {}).then((res) => res.data),

  getRecommendedJobs: (payload) => apiClient.post('/job-matching/recommended', payload).then((res) => res.data),
};
