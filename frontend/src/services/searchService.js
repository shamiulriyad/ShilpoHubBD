import apiClient from './apiClient';

// AI Search — GET /api/search?q=&page=&pageSize= -> PagedResult<ProductListItemDto>
export const searchService = {
  search: (q, params = {}) =>
    apiClient.get('/search', { params: { q, ...params } }).then((res) => res.data),
};
