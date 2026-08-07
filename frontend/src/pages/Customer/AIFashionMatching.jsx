import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { products } from '../../data/mockData';

export default function AIFashionMatching() {
  const { productId } = useParams();
  const product = products.find((p) => p.id === productId) || products[0];
  const matches = products
    .filter((p) => p.id !== product.id)
    .slice(0, 4)
    .map((p, i) => ({ ...p, matchScore: 92 - i * 7 }));

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: 'AI Fashion Matching' },
        ]}
        title="AI Fashion Matching"
        description={`Complete the look built around ${product.name}.`}
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <div className="mb-8 flex items-center gap-4 rounded-xl border border-border bg-surface p-4">
        <span className="flex h-14 w-14 shrink-0 items-center justify-center rounded-lg bg-background text-[10px] text-body/40">
          Item
        </span>
        <div>
          <p className="text-sm font-semibold text-heading">{product.name}</p>
          <p className="text-xs text-body/60">
            {product.category} · by {product.producer}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {matches.map((match) => (
          <div key={match.id} className="space-y-2">
            <ProductCard product={match} to={routePaths.customerProductDetails.replace(':productId', match.id)} />
            <Badge tone="success">{match.matchScore}% Match</Badge>
          </div>
        ))}
      </div>
    </div>
  );
}
