import axios from 'axios';
import { API_BASE_URL, API_TIMEOUT_MS } from '../config/runtime';
import { applyAxiosInterceptors } from './interceptors/axiosInterceptor';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT_MS,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

applyAxiosInterceptors(apiClient);

export default apiClient;