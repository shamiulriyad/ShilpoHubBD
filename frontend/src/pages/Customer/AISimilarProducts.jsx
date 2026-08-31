import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { RecommendationCard } from '../../components/cards';
import { useProduct } from '../../hooks/useProducts';
import { useSimilarProducts } from '../../hooks/useRecommendations';
import { toProductCardItem } from '../../utils/productAdapters';

export default function AISimilarProducts() {
  const { productId } = useParams();
  const productQuery = useProduct(productId);
  const similarQuery = useSimilarProducts(productId, 8);

  const product = productQuery.data;
  const similar = similarQuery.data || [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          ...(product
            ? [{ label: product.name, path: routePaths.customerProductDetails.replace(':productId', product.id) }]
            : []),
          { label: 'Similar Products' },
        ]}
        title={product ? `Similar to “${product.name}”` : 'Similar Products'}
        description="AI-matched products based on craft, category and style."
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <AsyncState isLoading={similarQuery.isLoading} isError={similarQuery.isError} error={similarQuery.error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {similar.map((item) => (
            <RecommendationCard
              key={item.id}
              product={toProductCardItem(item)}
              to={routePaths.customerProductDetails.replace(':productId', item.id)}
            />
          ))}
          {similar.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No similar products found yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
