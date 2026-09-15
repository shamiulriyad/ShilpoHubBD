import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, Pagination, SearchBar, AsyncState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { useProducts } from '../../hooks/useProducts';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';
import { toProductCardItem } from '../../utils/productAdapters';

export default function ProductListing() {
  const [searchParams, setSearchParams] = useSearchParams();
  const categoryId = searchParams.get('categoryId') || '';
  const districtId = searchParams.get('districtId') || '';
  const initialSearch = searchParams.get('search') || '';
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState(initialSearch);

  const categoriesQuery = useCategories();
  const districtsQuery = useDistricts();
  const productsQuery = useProducts({
    page,
    pageSize: 12,
    categoryId: categoryId || undefined,
    districtId: districtId || undefined,
    search: search || undefined,
  });

  const filterGroups = [
    {
      key: 'categoryId',
      label: 'Category',
      options: (categoriesQuery.data || []).map((category) => ({ label: category.name, value: category.id })),
    },
    {
      key: 'districtId',
      label: 'District',
      options: (districtsQuery.data || []).map((district) => ({ label: district.name, value: district.id })),
    },
  ];

  const updateFilter = (key, value, checked) => {
    const next = new URLSearchParams(searchParams);
    if (checked) next.set(key, value);
    else next.delete(key);
    setSearchParams(next, { replace: true });
    setPage(1);
  };

  const clearFilters = () => {
    const next = new URLSearchParams(searchParams);
    next.delete('categoryId');
    next.delete('districtId');
    setSearchParams(next, { replace: true });
    setPage(1);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Marketplace', path: routePaths.marketplace },
          { label: 'Products' },
        ]}
        title="All Products"
        description={
          productsQuery.data ? `${productsQuery.data.totalCount} heritage products found` : 'Loading products…'
        }
      />

      <div className="mb-6">
        <SearchBar
          placeholder="Search products…"
          value={search}
          onChange={(event) => {
            setSearch(event.target.value);
            setPage(1);
          }}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-[260px_minmax(0,1fr)]">
        <FilterPanel
          className="workspace-scroll lg:sticky lg:top-[6rem] lg:max-h-[calc(100dvh-7.5rem)] lg:overflow-y-auto lg:overscroll-contain"
          groups={filterGroups}
          values={{ categoryId, districtId }}
          onChange={updateFilter}
          onClear={clearFilters}
        />
        <div className="min-w-0">
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
            <AsyncState
              isLoading={productsQuery.isLoading}
              isError={productsQuery.isError}
              error={productsQuery.error}
            >
              {(productsQuery.data?.items || []).map((product) => (
                <ProductCard
                  key={product.id}
                  product={toProductCardItem(product)}
                  to={routePaths.marketplaceProductDetails.replace(':productId', product.id)}
                />
              ))}
              {productsQuery.data?.items?.length === 0 && (
                <p className="col-span-full text-sm text-body/60">No products match your filters.</p>
              )}
            </AsyncState>
          </div>
          {productsQuery.data?.totalPages > 1 && (
            <div className="mt-8">
              <Pagination currentPage={page} totalPages={productsQuery.data.totalPages} onPageChange={setPage} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
