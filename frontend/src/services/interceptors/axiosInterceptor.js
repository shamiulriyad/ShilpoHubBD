import axios from 'axios';
import { useAuthStore } from '../../stores/useAuthStore';

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

// Plain instance (no interceptors) used only for the refresh call itself,
// so a failed refresh can't recursively trigger this same response interceptor.
const refreshClient = axios.create({ baseURL });

let isRefreshing = false;
let refreshQueue = [];

function flushQueue(newAccessToken) {
  refreshQueue.forEach((resolveWithToken) => resolveWithToken(newAccessToken));
  refreshQueue = [];
}

function redirectToLogin() {
  useAuthStore.getState().clearSession();
  window.location.assign('/login');
}

export function applyAxiosInterceptors(apiClient) {
  apiClient.interceptors.request.use((config) => {
    const token = useAuthStore.getState().accessToken;

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  });

  apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
      const { response, config } = error;
      const isAuthCall = ['/auth/login', '/auth/register', '/auth/refresh'].some((path) =>
        config?.url?.includes(path),
      );

      if (response?.status !== 401 || isAuthCall || config._retry) {
        return Promise.reject(error);
      }

      const { refreshToken } = useAuthStore.getState();
      if (!refreshToken) {
        redirectToLogin();
        return Promise.reject(error);
      }

      config._retry = true;

      if (isRefreshing) {
        return new Promise((resolve) => {
          refreshQueue.push((newAccessToken) => {
            config.headers.Authorization = `Bearer ${newAccessToken}`;
            resolve(apiClient(config));
          });
        });
      }

      isRefreshing = true;
      try {
        const { data } = await refreshClient.post('/auth/refresh', { refreshToken });
        useAuthStore.getState().setSession(data);
        flushQueue(data.accessToken);
        config.headers.Authorization = `Bearer ${data.accessToken}`;
        return apiClient(config);
      } catch (refreshError) {
        refreshQueue = [];
        redirectToLogin();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    },
  );
}
