import axios from 'axios';
import { applyAxiosInterceptors } from './interceptors/axiosInterceptor';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

applyAxiosInterceptors(apiClient);

export default apiClient;
