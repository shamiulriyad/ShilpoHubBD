import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { RecommendationCard } from '../../components/cards';
import { products } from '../../data/mockData';

export default function AISimilarProducts() {
  const { productId } = useParams();
  const product = products.find((p) => p.id === productId) || products[0];
  const similar = products
    .filter((p) => p.id !== product.id && p.category === product.category)
    .concat(products.filter((p) => p.id !== product.id && p.category !== product.category))
    .slice(0, 4)
    .map((p, i) => ({ ...p, similarity: 96 - i * 8 }));

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: product.name, path: routePaths.customerProductDetails.replace(':productId', product.id) },
          { label: 'Similar Products' },
        ]}
        title={`Similar to “${product.name}”`}
        description="AI-matched products based on craft, category and style."
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {similar.map((item) => (
          <RecommendationCard
            key={item.id}
            product={item}
            matchScore={item.similarity}
            to={routePaths.customerProductDetails.replace(':productId', item.id)}
          />
        ))}
      </div>
    </div>
  );
}
