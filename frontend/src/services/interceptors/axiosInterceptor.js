export function applyAxiosInterceptors(apiClient) {
  apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('accessToken');

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  });

  apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error?.response?.status === 401) {
        localStorage.removeItem('accessToken');
        window.location.assign('/login');
      }

      return Promise.reject(error);
    },
  );
}
export function applyAxiosInterceptors(apiClient) {
  apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('accessToken');

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  });

  apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error?.response?.status === 401) {
        localStorage.removeItem('accessToken');
        window.location.assign('/login');
      }

      return Promise.reject(error);
    },
  );
}
