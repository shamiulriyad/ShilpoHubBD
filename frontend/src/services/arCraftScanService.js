import apiClient from './apiClient';

export const arCraftScanService = {
  scan: (code) => apiClient.post('/ar-vr/craft-scans', { code }).then((res) => res.data),
};
