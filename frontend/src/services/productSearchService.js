import apiClient from './apiClient';

// AI-assisted product search (backend: GET /api/product-search). Prices, stock and ratings in the answer come from PostgreSQL.
export const productSearchService = {
  search: ({ q, page = 1, pageSize = 12 }, signal) => apiClient.get('/product-search', { params: { q, page, pageSize }, signal }).then((res) => res.data),
};
