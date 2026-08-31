import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SearchBar, SectionHeader, Badge, CategoryFilter, MarketplaceFilter, AsyncState } from '../../components/ui';
import { ProductCard, EntityCard, ProducerCard } from '../../components/cards';
import { useCategories } from '../../hooks/useCategories';
import { useProducts } from '../../hooks/useProducts';
import { useRecommendedForMe } from '../../hooks/useRecommendations';
import { useLiveEvents } from '../../hooks/useLiveEvents';
import { useSearch } from '../../hooks/useSearch';
import { useAuth } from '../../hooks/useAuth';
import { toProductCardItem, toCategoryCardItem } from '../../utils/productAdapters';

export default function Marketplace() {
  const { isAuthenticated } = useAuth();
  const [activeCategoryId, setActiveCategoryId] = useState(null);
  const [searchInput, setSearchInput] = useState('');
  const [searchQuery, setSearchQuery] = useState('');

  const categoriesQuery = useCategories();
  const recommendedQuery = useRecommendedForMe(4);
  const productsQuery = useProducts({ categoryId: activeCategoryId || undefined, pageSize: 12 });
  const liveEventsQuery = useLiveEvents({ pageSize: 5 });
  const searchResults = useSearch(searchQuery);

  const liveEvent = (liveEventsQuery.data?.items || []).find(
    (e) => (e.status || '').toLowerCase() === 'live',
  );

  // No producer-directory endpoint — derive distinct producers from the catalog.
  const featuredProducers = [
    ...new Map(
      (productsQuery.data?.items || [])
        .filter((p) => p.producerName)
        .map((p) => [p.producerName, { name: p.producerName, craft: p.categoryName, district: p.districtName }]),
    ).values(),
  ].slice(0, 6);

  const categoryOptions = [
    { id: null, name: 'All' },
    ...(categoriesQuery.data || []).map((c) => ({ id: c.id, name: c.name })),
  ];

  const isSearching = searchQuery.trim().length >= 2;

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Marketplace' }]}
        title="Marketplace"
        description="Authentic heritage products, direct from verified producers across Bangladesh."
      />

      <div className="mb-10 max-w-xl">
        <SearchBar
          placeholder="Search products, categories, producers…"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onSubmit={(value) => setSearchQuery(value || '')}
        />
      </div>

      {isSearching ? (
        <>
          <SectionHeader
            eyebrow="AI Search"
            title={`Results for “${searchQuery.trim()}”`}
            action={
              <button
                type="button"
                onClick={() => {
                  setSearchInput('');
                  setSearchQuery('');
                }}
                className="text-sm font-medium text-link hover:underline"
              >
                Clear search
              </button>
            }
          />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            <AsyncState isLoading={searchResults.isLoading} isError={searchResults.isError} error={searchResults.error}>
              {(searchResults.data?.items || []).map((product) => (
                <ProductCard
                  key={product.id}
                  product={toProductCardItem(product)}
                  to={routePaths.customerProductDetails.replace(':productId', product.id)}
                />
              ))}
              {searchResults.data?.items?.length === 0 && (
                <p className="col-span-full text-sm text-body/60">No products matched your search.</p>
              )}
            </AsyncState>
          </div>
        </>
      ) : (
        <>
          {liveEvent && (
            <div className="mb-10 flex flex-wrap items-center gap-3 rounded-xl border border-primary/20 bg-primary/5 p-4">
              <Badge tone="success">Live Now</Badge>
              <p className="text-sm text-body/80">
                {liveEvent.producerName} is streaming {liveEvent.title.toLowerCase()}.
              </p>
              <Link
                to={routePaths.customerLiveShopping.replace(':workshopId', liveEvent.id)}
                className="ml-auto text-sm font-medium text-link hover:underline"
              >
                Watch now →
              </Link>
            </div>
          )}

          <SectionHeader eyebrow="Browse" title="Shop by Category" />
          <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
            <AsyncState isLoading={categoriesQuery.isLoading} isError={categoriesQuery.isError} error={categoriesQuery.error}>
              {categoriesQuery.data?.map((category) => {
                const item = toCategoryCardItem(category);
                return (
                  <EntityCard
                    key={item.id}
                    title={item.name}
                    subtitle={`${item.itemCount} items`}
                    to={routePaths.customerMarketplace}
                  />
                );
              })}
            </AsyncState>
          </div>

          {isAuthenticated && (
            <>
              <SectionHeader eyebrow="For You" title="Recommended for You" />
              <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
                <AsyncState isLoading={recommendedQuery.isLoading} isError={recommendedQuery.isError} error={recommendedQuery.error}>
                  {(recommendedQuery.data || []).map((product) => (
                    <ProductCard
                      key={product.id}
                      product={toProductCardItem(product)}
                      to={routePaths.customerProductDetails.replace(':productId', product.id)}
                    />
                  ))}
                </AsyncState>
              </div>
            </>
          )}

          <SectionHeader eyebrow="Featured" title="Featured Products" />
          <div className="grid gap-6 lg:grid-cols-[240px_1fr]">
            <MarketplaceFilter className="hidden lg:block" />
            <div>
              <CategoryFilter className="mb-6" options={categoryOptions} active={activeCategoryId} onChange={setActiveCategoryId} />
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
                <AsyncState isLoading={productsQuery.isLoading} isError={productsQuery.isError} error={productsQuery.error}>
                  {(productsQuery.data?.items || []).map((product) => (
                    <ProductCard
                      key={product.id}
                      product={toProductCardItem(product)}
                      to={routePaths.customerProductDetails.replace(':productId', product.id)}
                    />
                  ))}
                  {productsQuery.data?.items?.length === 0 && (
                    <p className="col-span-full text-sm text-body/60">No products in this category yet.</p>
                  )}
                </AsyncState>
              </div>
            </div>
          </div>

          <div className="mt-10">
            <SectionHeader eyebrow="Community" title="Featured Producers" />
          </div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
            {featuredProducers.map((producer) => (
              <ProducerCard key={producer.name} producer={producer} />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
