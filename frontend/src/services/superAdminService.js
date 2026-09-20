import apiClient from './apiClient';

// Uses the same authenticated client and refresh flow as the rest of the app.
export function createSuperAdminService(client) {
  return {
    list: (path, params) => client.get(path, { params }).then(r => r.data),
    detail: (path, id) => client.get(`${path}/${encodeURIComponent(id)}`).then(r => r.data),
    save: (path, id, payload) => (id ? client.put(`${path}/${encodeURIComponent(id)}`, payload) : client.post(path, payload)).then(r => r.data),
    action: (method, path, payload, params) => client.request({ method, url: path, data: payload, params, ...(/\/(scans|backups)$/.test(path) && method === 'post' ? { timeout: 300000 } : {}) }).then(r => r.data),
  };
}
export const superAdminService = createSuperAdminService(apiClient);
