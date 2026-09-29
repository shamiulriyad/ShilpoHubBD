import apiClient from './apiClient';

const base = '/governance/producer-monthly-reports';

// Government/NGO-facing view: the backend forces results to reports actually shared with the caller,
// regardless of what filters are passed here — never all Producer reports.
export const govProducerReportsService = {
  list: (params) => apiClient.get(base, { params }).then((r) => r.data),
  getById: (id) => apiClient.get(`${base}/${id}`).then((r) => r.data),
};
