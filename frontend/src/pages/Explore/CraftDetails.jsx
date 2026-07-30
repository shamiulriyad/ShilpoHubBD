import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { EntityCard, ProductCard, StatCard } from '../../components/cards';
import { crafts, producers, products } from '../../data/mockData';

export default function CraftDetails() {
  const { craftId } = useParams();
  const craft = crafts.find((c) => c.id === craftId) || crafts[0];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Crafts', path: routePaths.exploreCrafts },
          { label: craft.name },
        ]}
        title={craft.name}
        description={`Category: ${craft.category}`}
      />

      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <StatCard label="Active Producers" value={craft.producers} />
        <StatCard label="Products Listed" value={products.length * 3} />
        <StatCard label="Category" value={craft.category} />
      </div>

      <p className="mb-3 text-sm font-semibold text-heading">Producers</p>
      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {producers.map((producer) => (
          <EntityCard key={producer.id} title={producer.name} subtitle={producer.district} to={routePaths.exploreProducerDetails.replace(':producerId', producer.id)} />
        ))}
      </div>

      <p className="mb-3 text-sm font-semibold text-heading">Products</p>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {products.slice(0, 4).map((product) => (
          <ProductCard key={product.id} product={product} to={routePaths.marketplaceProductDetails.replace(':productId', product.id)} />
        ))}
      </div>
    </div>
  );
}
