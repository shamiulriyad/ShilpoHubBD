import apiClient from './apiClient';

export const messagingService = {
  listConversations: (params) => apiClient.get('/messaging/conversations', { params }).then((res) => res.data),
  getConversation: (id) => apiClient.get(`/messaging/conversations/${id}`).then((res) => res.data),
  startConversation: (recipientId, body, imageUrl) =>
    apiClient.post('/messaging/conversations', { recipientId, body: body || '', imageUrl: imageUrl || undefined }).then((res) => res.data),
  sendMessage: (id, body, imageUrl) =>
    apiClient.post(`/messaging/conversations/${id}/messages`, { body: body || '', imageUrl: imageUrl || undefined }).then((res) => res.data),
  markAsRead: (id) => apiClient.post(`/messaging/conversations/${id}/read`),

  // Picture attached to a chat message, product question or complaint. Returns { url }.
  uploadImage: (file) => {
    const form = new FormData();
    form.append('file', file);
    return apiClient.post('/media/chat-images', form, { headers: { 'Content-Type': 'multipart/form-data' } }).then((res) => res.data);
  },
};
