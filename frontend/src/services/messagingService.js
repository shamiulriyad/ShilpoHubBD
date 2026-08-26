import apiClient from './apiClient';

export const messagingService = {
  listConversations: (params) => apiClient.get('/messaging/conversations', { params }).then((res) => res.data),
  getConversation: (id) => apiClient.get(`/messaging/conversations/${id}`).then((res) => res.data),
  startConversation: (recipientId, body) =>
    apiClient.post('/messaging/conversations', { recipientId, body }).then((res) => res.data),
  sendMessage: (id, body) => apiClient.post(`/messaging/conversations/${id}/messages`, { body }).then((res) => res.data),
  markAsRead: (id) => apiClient.post(`/messaging/conversations/${id}/read`),
};
