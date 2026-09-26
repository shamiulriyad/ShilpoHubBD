import apiClient from './apiClient';

export const tourismLocationsService = {
  list: (params) => apiClient.get('/tourism-locations', { params }).then((res) => res.data),
  // Our backend fills these from the tourism database and OpenStreetMap; the client never sees Overpass.
  accommodations: (districtId) => apiClient.get('/tourism/accommodations', { params: { districtId } }).then((res) => res.data),
  // Hotels, hostels, resorts and guest houses within radiusKm of the destination, with distances.
  nearbyAccommodations: (districtId, radiusKm) =>
    apiClient.get('/tourism/accommodations/nearby', { params: { districtId, radiusKm } }).then((res) => res.data),
  pois: (districtId) => apiClient.get('/tourism/pois', { params: { districtId } }).then((res) => res.data),
  getById: (id) => apiClient.get(`/tourism-locations/${id}`).then((res) => res.data),
};
