import apiClient from './apiClient';

export const workshopGalleryService = {
  listForProducer: (producerId) => apiClient.get(`/producers/${producerId}/workshop-gallery`).then((res) => res.data),
};
