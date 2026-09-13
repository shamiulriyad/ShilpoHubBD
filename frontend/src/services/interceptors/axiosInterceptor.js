import axios from 'axios';
import { API_BASE_URL, API_TIMEOUT_MS } from '../../config/runtime';
import { queryClient } from '../../lib/queryClient';
import { useAuthStore } from '../../stores/useAuthStore';

const refreshClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT_MS,
  headers: { Accept: 'application/json', 'Content-Type': 'application/json' },
});

let isRefreshing = false;
let refreshQueue = [];

function flushQueue(error, newAccessToken = null) {
  refreshQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve(newAccessToken);
  });
  refreshQueue = [];
}

function redirectToLogin() {
  useAuthStore.getState().clearSession();
  queryClient.clear();

  if (window.location.pathname !== '/login') {
    window.location.replace('/login?reason=session-expired');
  }
}

export function applyAxiosInterceptors(apiClient) {
  apiClient.interceptors.request.use((config) => {
    const token = useAuthStore.getState().accessToken;

    if (token) {
      config.headers = config.headers ?? {};
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

      if (response?.status !== 401 || isAuthCall || !config || config._retry) {
        return Promise.reject(error);
      }

      const { accessToken, refreshToken } = useAuthStore.getState();
      if (!refreshToken) {
        if (accessToken) redirectToLogin();
        return Promise.reject(error);
      }

      config._retry = true;

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          refreshQueue.push({
            resolve: (newAccessToken) => {
              config.headers = config.headers ?? {};
              config.headers.Authorization = `Bearer ${newAccessToken}`;
              resolve(apiClient(config));
            },
            reject,
          });
        });
      }

      isRefreshing = true;
      try {
        const { data } = await refreshClient.post('/auth/refresh', { refreshToken });
        useAuthStore.getState().setSession(data);
        flushQueue(null, data.accessToken);
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${data.accessToken}`;
        return apiClient(config);
      } catch (refreshError) {
        flushQueue(refreshError);
        redirectToLogin();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    },
  );
}