import apiClient from './apiClient';

export const touristServicesService = {
  list: (params) => apiClient.get('/tourist-services', { params }).then((res) => res.data),
  getById: (id) => apiClient.get(`/tourist-services/${id}`).then((res) => res.data),
  availabilitySlots: (serviceId, params) =>
    apiClient.get(`/tourist-services/${serviceId}/availability-slots`, { params }).then((res) => res.data),
};
