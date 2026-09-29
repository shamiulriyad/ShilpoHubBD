import apiClient from './apiClient';

// AI-assisted product search (backend: GET /api/product-search). Prices, stock and ratings in the answer come from PostgreSQL.
export const productSearchService = {
  // Gemini query analysis + embeddings can take longer than the default API timeout, especially under free-tier
  // rate limiting where the Python service retries with a 10-60s backoff before falling back to keyword search.
  search: ({ q, page = 1, pageSize = 12 }, signal) => apiClient.get('/product-search', { params: { q, page, pageSize }, signal, timeout: 45000 }).then((res) => res.data),
};
