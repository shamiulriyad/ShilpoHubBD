import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { producers, products } from '../../data/mockData';

export default function ProducerDetails() {
  const { producerId } = useParams();
  const producer = producers.find((p) => p.id === producerId) || producers[0];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Producers', path: routePaths.exploreProducers },
          { label: producer.name },
        ]}
        title={producer.name}
        description={`${producer.craft} · ${producer.district}`}
        action={<Button variant="primary">Contact Producer</Button>}
      />

      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <StatCard label="Rating" value={`★ ${producer.rating}`} />
        <StatCard label="Products Listed" value="24" />
        <StatCard label="Years Active" value="12" />
      </div>

      <p className="mb-3 text-sm font-semibold text-heading">Products by {producer.name}</p>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {products.slice(0, 4).map((product) => (
          <ProductCard key={product.id} product={product} to={routePaths.marketplaceProductDetails.replace(':productId', product.id)} />
        ))}
      </div>
    </div>
  );
}
