import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SearchBar, SectionHeader, Badge, CategoryFilter, MarketplaceFilter, AsyncState, Pagination } from '../../components/ui';
import { priceRangeToQuery } from '../../components/ui/MarketplaceFilter';
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
  const [searchParams, setSearchParams] = useSearchParams();
  const activeCategoryId = searchParams.get('categoryId') || null;
  const [page, setPage] = useState(1);
  const [sortBy, setSortBy] = useState('Newest');
  const [filtersOpen, setFiltersOpen] = useState(false);

  const setActiveCategoryId = (value) => {
    const nextValue = value || null;
    setPage(1);
    const nextParams = new URLSearchParams(searchParams);
    if (nextValue) nextParams.set('categoryId', nextValue);
    else nextParams.delete('categoryId');
    setSearchParams(nextParams, { replace: true });
  };
  const [searchInput, setSearchInput] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [districtId, setDistrictId] = useState('');
  const [priceRange, setPriceRange] = useState('');

  const categoriesQuery = useCategories();
  const recommendedQuery = useRecommendedForMe(4, isAuthenticated);
  const productsQuery = useProducts({
    categoryId: activeCategoryId || undefined,
    districtId: districtId || undefined,
    ...priceRangeToQuery(priceRange),
    page,
    sortBy,
    pageSize: 12,
  });
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
        .map((p) => [p.producerId || p.producerName, { id: p.producerId, name: p.producerName, craft: p.categoryName, district: p.districtName, image: p.producerImageUrl }]),
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
                    image={item.image}
                    subtitle={`${item.itemCount} ${item.itemCount === 1 ? 'item' : 'items'}`}
                    to={`${routePaths.customerMarketplace}?categoryId=${item.id}`}
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
          <div className="grid items-start gap-6 lg:grid-cols-[240px_minmax(0,1fr)]">
            <div className="rounded-2xl border border-border bg-surface lg:sticky lg:top-[6rem] lg:border-0 lg:bg-transparent">
              <button type="button" onClick={() => setFiltersOpen((previous) => !previous)} aria-expanded={filtersOpen} aria-controls="marketplace-filters" className="flex w-full items-center justify-between px-5 py-4 text-sm font-semibold text-heading lg:hidden">Filters <span className="text-primary" aria-hidden="true">{filtersOpen ? '−' : '+'}</span></button>
              <div id="marketplace-filters" className={`workspace-scroll lg:block lg:max-h-[calc(100dvh-7.5rem)] lg:overflow-y-auto lg:overscroll-contain ${filtersOpen ? 'block' : 'hidden'}`}>
              <MarketplaceFilter
              values={{ categoryId: activeCategoryId, districtId, priceRange }}
              onChange={(key, value, checked) => {
                setPage(1);
                const nextValue = checked ? value : '';
                if (key === 'categoryId') setActiveCategoryId(nextValue || null);
                if (key === 'districtId') setDistrictId(nextValue);
                if (key === 'priceRange') setPriceRange(nextValue);
              }}
              onClear={() => {
                setActiveCategoryId(null);
                setDistrictId('');
                setPriceRange('');
              }}
            />
              </div>
            </div>
            <div className="min-w-0">
              <div className="mb-5 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-surface px-4 py-3">
                <p className="text-sm text-muted" role="status">{productsQuery.isFetching ? 'Updating products…' : `${productsQuery.data?.totalCount ?? 0} ${productsQuery.data?.totalCount === 1 ? 'product' : 'products'} found`}</p>
                <label className="flex items-center gap-2 text-sm text-body">Sort by
                  <select value={sortBy} onChange={(event) => { setSortBy(event.target.value); setPage(1); }} className="rounded-lg border border-border bg-background px-3 py-2 text-sm text-heading">
                    <option value="Newest">Newest first</option>
                    <option value="PriceLowToHigh">Price: low to high</option>
                    <option value="PriceHighToLow">Price: high to low</option>
                    <option value="Popular">Most popular</option>
                    <option value="TopRated">Top rated</option>
                  </select>
                </label>
              </div>
              <CategoryFilter className="mb-6" options={categoryOptions} active={activeCategoryId} onChange={setActiveCategoryId} />
              <div className="grid grid-cols-1 gap-4 min-[420px]:grid-cols-2 sm:grid-cols-3 lg:grid-cols-2 xl:grid-cols-3">
                <AsyncState isLoading={productsQuery.isLoading} isError={productsQuery.isError} error={productsQuery.error}>
                  {(productsQuery.data?.items || []).map((product) => (
                    <ProductCard
                      key={product.id}
                      product={toProductCardItem(product)}
                      to={routePaths.customerProductDetails.replace(':productId', product.id)}
                    />
                  ))}
                  {productsQuery.data?.items?.length === 0 && (
                    <p className="col-span-full rounded-xl border border-dashed border-border bg-surface px-6 py-12 text-center text-sm text-muted">No products match these filters. Try another category, price range or district.</p>
                  )}
                </AsyncState>
              </div>
              {productsQuery.data?.totalPages > 1 && <div className="mt-6"><Pagination currentPage={page} totalPages={productsQuery.data.totalPages} onPageChange={setPage} /></div>}
            </div>
          </div>

          <div className="mt-10">
            <SectionHeader eyebrow="Community" title="Featured Producers" />
          </div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
            {featuredProducers.map((producer) => (
              <ProducerCard key={producer.id || producer.name} producer={producer} to={producer.id ? routePaths.exploreProducerDetails.replace(':producerId', producer.id) : undefined} />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
