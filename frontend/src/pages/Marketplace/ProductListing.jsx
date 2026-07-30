import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, Pagination, SearchBar } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { products, categories, districts } from '../../data/mockData';

const filterGroups = [
  { label: 'Category', options: categories.map((c) => c.name) },
  { label: 'District', options: districts.slice(0, 5).map((d) => d.name) },
  { label: 'Price Range', options: ['Under ৳1,000', '৳1,000 - ৳3,000', '৳3,000 - ৳6,000', 'Above ৳6,000'] },
];

export default function ProductListing() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Marketplace', path: routePaths.marketplace },
          { label: 'Products' },
        ]}
        title="All Products"
        description={`${products.length}+ heritage products across categories`}
      />

      <div className="mb-6">
        <SearchBar placeholder="Search products…" />
      </div>

      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
            {products.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                to={routePaths.marketplaceProductDetails.replace(':productId', product.id)}
              />
            ))}
          </div>
          <div className="mt-8">
            <Pagination currentPage={1} totalPages={4} />
          </div>
        </div>
      </div>
    </div>
  );
}
