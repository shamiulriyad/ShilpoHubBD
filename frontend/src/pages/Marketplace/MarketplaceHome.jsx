import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SearchBar, SectionHeader, AsyncState } from '../../components/ui';
import { ProductCard, EntityCard } from '../../components/cards';
import { useCategories } from '../../hooks/useCategories';
import { useFeaturedProducts } from '../../hooks/useProducts';
import { toProductCardItem, toCategoryCardItem } from '../../utils/productAdapters';
// Producer directory has no backend endpoint yet (no ProducersController) — kept on mock data.
import { producers } from '../../data/mockData';

export default function MarketplaceHome() {
  const categoriesQuery = useCategories();
  const featuredQuery = useFeaturedProducts(8);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'Marketplace' }]}
        title="Marketplace"
        description="Authentic heritage products, direct from verified producers across Bangladesh."
      />

      <div className="mb-10 max-w-xl">
        <SearchBar placeholder="Search products, categories, producers…" />
      </div>

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
                to={routePaths.marketplaceCategories}
              />
            );
          })}
        </AsyncState>
      </div>

      <SectionHeader
        eyebrow="Featured"
        title="Featured Products"
        action={
          <Link to={routePaths.marketplaceProducts} className="text-sm font-medium text-link hover:underline">
            View all products →
          </Link>
        }
      />
      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        <AsyncState isLoading={featuredQuery.isLoading} isError={featuredQuery.isError} error={featuredQuery.error}>
          {featuredQuery.data?.map((product) => (
            <ProductCard
              key={product.id}
              product={toProductCardItem(product)}
              to={routePaths.marketplaceProductDetails.replace(':productId', product.id)}
            />
          ))}
        </AsyncState>
      </div>

      <SectionHeader
        eyebrow="Community"
        title="Featured Producers"
        action={
          <Link to={routePaths.exploreProducers} className="text-sm font-medium text-link hover:underline">
            View all →
          </Link>
        }
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
        {producers.map((producer) => (
          <EntityCard key={producer.id} title={producer.name} subtitle={producer.craft} meta={producer.district} to={routePaths.exploreProducers} />
        ))}
      </div>
    </div>
  );
}
