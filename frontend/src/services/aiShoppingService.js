import apiClient from './apiClient';

export const aiShoppingService = {
  giftRecommendations: (payload) => apiClient.post('/ai-shopping/gift-recommendations', payload).then((res) => res.data),
  fashionMatches: (payload) => apiClient.post('/ai-shopping/fashion-matches', payload).then((res) => res.data),
  interiorPreview: (payload) => apiClient.post('/ai-shopping/interior-preview', payload).then((res) => res.data),
  translate: (payload) => apiClient.post('/ai-shopping/translate', payload).then((res) => res.data),
};
