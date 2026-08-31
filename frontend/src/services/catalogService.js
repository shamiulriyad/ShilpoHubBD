import apiClient from './apiClient';

const unwrap = (res) => res.data;

/**
 * Read APIs for the public "Explore" surface: districts, heritage villages,
 * craft categories, craft stories and the product catalog used to derive
 * producer / product listings where a dedicated endpoint does not yet exist.
 */
export const catalogService = {
  // Districts — GET /api/districts (list only; no detail endpoint yet)
  getDistricts: () => apiClient.get('/districts').then(unwrap),

  // Villages — GET /api/villages, GET /api/villages/{id}
  getVillages: () => apiClient.get('/villages').then(unwrap),
  getVillage: (id) => apiClient.get(`/villages/${id}`).then(unwrap),

  // Craft categories — GET /api/categories, GET /api/categories/{id}
  getCategories: () => apiClient.get('/categories').then(unwrap),
  getCategory: (id) => apiClient.get(`/categories/${id}`).then(unwrap),

  // Craft story for a category — GET /api/craft-stories/category/{categoryId}
  getCraftStoryByCategory: (categoryId) =>
    apiClient.get(`/craft-stories/category/${categoryId}`).then(unwrap),

  // Producer story — GET /api/producer-stories/{producerId}
  getProducerStory: (producerId) =>
    apiClient.get(`/producer-stories/${producerId}`).then(unwrap),

  /**
   * Product catalog — GET /api/products (paged).
   * Supported filters: search, categoryId, districtId, minPrice, maxPrice, sortBy, page, pageSize.
   * Returns PagedResult<ProductListItemDto>: { items, totalCount, page, pageSize, totalPages }.
   */
  getProducts: (params = {}) => apiClient.get('/products', { params }).then(unwrap),

  // Curated collection — GET /api/products/featured?count=
  getFeaturedProducts: (count = 12) =>
    apiClient.get('/products/featured', { params: { count } }).then(unwrap),
};

export default catalogService;
