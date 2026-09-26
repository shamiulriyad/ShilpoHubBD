import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SearchBar, SectionHeader, AsyncState } from '../../components/ui';
import { ProductCard, EntityCard } from '../../components/cards';
import { useCategories } from '../../hooks/useCategories';
import { useFeaturedProducts, useProducts } from '../../hooks/useProducts';
import { useProducerDirectory } from '../../hooks/useProducerDirectory';
import { toProductCardItem, toCategoryCardItem } from '../../utils/productAdapters';

export default function MarketplaceHome() {
  const categoriesQuery = useCategories();
  const featuredQuery = useFeaturedProducts(8);

  // Top up featured picks with top-rated approved products so the section is never sparse.
  const topRatedQuery = useProducts({ page: 1, pageSize: 8, sortBy: 'TopRated' });
  const directoryQuery = useProducerDirectory();
  // Latest approved products, used to give each category card a real product photo.
  const allProductsQuery = useProducts({ page: 1, pageSize: 50 });
  const imageByCategory = new Map();
  (allProductsQuery.data?.items || []).forEach((p) => {
    if (p.categoryId && p.primaryImageUrl && !imageByCategory.has(p.categoryId)) imageByCategory.set(p.categoryId, p.primaryImageUrl);
  });
  const categories = [...(categoriesQuery.data || [])]
    .filter((c) => c.productCount > 0)
    .sort((a, b) => b.productCount - a.productCount)
    .slice(0, 12);
  const featured = [
    ...new Map(
      [...(featuredQuery.data || []), ...(topRatedQuery.data?.items || [])].map((p) => [p.id, p]),
    ).values(),
  ].slice(0, 8);
  const producers = (directoryQuery.data || []).slice(0, 6).map((p) => ({
    id: p.id,
    name: p.name,
    craft: p.crafts.map((c) => c.name).slice(0, 2).join(', '),
    district: p.districts.map((d) => d.name).slice(0, 2).join(', '),
    productCount: p.productCount,
  }));

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
          {categories.map((category) => {
            const item = toCategoryCardItem(category);
            return (
              <EntityCard
                key={item.id}
                title={item.name}
                subtitle={`${item.itemCount} ${item.itemCount === 1 ? 'product' : 'products'}`}
                image={imageByCategory.get(category.id) || category.imageUrl}
                to={`${routePaths.marketplaceProducts}?categoryId=${category.id}`}
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
        <AsyncState isLoading={featuredQuery.isLoading || topRatedQuery.isLoading} isError={featuredQuery.isError && topRatedQuery.isError} error={featuredQuery.error}>
          {featured.map((product) => (
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
          <EntityCard
            key={producer.id}
            title={producer.name}
            subtitle={producer.craft}
            meta={`${producer.district} · ${producer.productCount} products`}
            to={routePaths.exploreProducers}
          />
        ))}
      </div>
    </div>
  );
}
