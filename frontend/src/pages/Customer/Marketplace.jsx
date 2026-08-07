import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SearchBar, SectionHeader, Badge, CategoryFilter, MarketplaceFilter } from '../../components/ui';
import { ProductCard, EntityCard, ProducerCard } from '../../components/cards';
import { products, categories, producers, workshops } from '../../data/mockData';

export default function Marketplace() {
  const liveWorkshops = workshops.filter((w) => w.status === 'live');
  const [activeCategory, setActiveCategory] = useState('All');
  const filteredProducts =
    activeCategory === 'All' ? products : products.filter((p) => p.category === activeCategory);

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Marketplace' }]}
        title="Marketplace"
        description="Authentic heritage products, direct from verified producers across Bangladesh."
      />

      <div className="mb-10 max-w-xl">
        <SearchBar placeholder="Search products, categories, producers…" />
      </div>

      {liveWorkshops.length > 0 && (
        <div className="mb-10 flex flex-wrap items-center gap-3 rounded-xl border border-primary/20 bg-primary/5 p-4">
          <Badge tone="success">Live Now</Badge>
          <p className="text-sm text-body/80">
            {liveWorkshops[0].producer} is streaming {liveWorkshops[0].title.toLowerCase()}.
          </p>
          <Link
            to={routePaths.customerLiveShopping.replace(':workshopId', liveWorkshops[0].id)}
            className="ml-auto text-sm font-medium text-link hover:underline"
          >
            Watch now →
          </Link>
        </div>
      )}

      <SectionHeader eyebrow="Browse" title="Shop by Category" />
      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
        {categories.map((category) => (
          <EntityCard key={category.id} title={category.name} subtitle={`${category.itemCount} items`} to={routePaths.customerMarketplace} />
        ))}
      </div>

      <SectionHeader eyebrow="For You" title="Recommended for You" />
      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {products.slice(2, 6).map((product) => (
          <ProductCard
            key={product.id}
            product={product}
            to={routePaths.customerProductDetails.replace(':productId', product.id)}
          />
        ))}
      </div>

      <SectionHeader eyebrow="Featured" title="Featured Products" />
      <div className="grid gap-6 lg:grid-cols-[240px_1fr]">
        <MarketplaceFilter className="hidden lg:block" />
        <div>
          <CategoryFilter
            className="mb-6"
            options={['All', ...categories.map((c) => c.name)]}
            active={activeCategory}
            onChange={setActiveCategory}
          />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {filteredProducts.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                to={routePaths.customerProductDetails.replace(':productId', product.id)}
              />
            ))}
            {filteredProducts.length === 0 && (
              <p className="col-span-full text-sm text-body/60">No products in this category yet.</p>
            )}
          </div>
        </div>
      </div>

      <div className="mt-10">
        <SectionHeader eyebrow="Community" title="Featured Producers" />
      </div>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
        {producers.map((producer) => (
          <ProducerCard
            key={producer.id}
            producer={producer}
            to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
          />
        ))}
      </div>
    </div>
  );
}
