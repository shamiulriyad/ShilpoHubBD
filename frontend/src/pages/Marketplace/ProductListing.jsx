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
  const [searchParams] = useSearchParams();
  const categoryId = searchParams.get('categoryId') || undefined;
  const districtId = searchParams.get('districtId') || undefined;
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');

  const categoriesQuery = useCategories();
  const districtsQuery = useDistricts();
  const productsQuery = useProducts({ page, pageSize: 12, categoryId, districtId, search: search || undefined });

  const filterGroups = [
    { label: 'Category', options: (categoriesQuery.data || []).map((c) => c.name) },
    { label: 'District', options: (districtsQuery.data || []).slice(0, 8).map((d) => d.name) },
  ];

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
          productsQuery.data ? `${productsQuery.data.totalCount}+ heritage products across categories` : 'Loading products…'
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

      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
            <AsyncState
              isLoading={productsQuery.isLoading}
              isError={productsQuery.isError}
              error={productsQuery.error}
            >
              {productsQuery.data?.items.map((product) => (
                <ProductCard
                  key={product.id}
                  product={toProductCardItem(product)}
                  to={routePaths.marketplaceProductDetails.replace(':productId', product.id)}
                />
              ))}
              {productsQuery.data?.items.length === 0 && (
                <p className="col-span-full text-sm text-body/60">No products match your filters.</p>
              )}
            </AsyncState>
          </div>
          {productsQuery.data?.totalPages > 1 && (
            <div className="mt-8">
              <Pagination
                currentPage={page}
                totalPages={productsQuery.data.totalPages}
                onPageChange={setPage}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
