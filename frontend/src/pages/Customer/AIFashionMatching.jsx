import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useProduct } from '../../hooks/useProducts';
import { useFashionMatches } from '../../hooks/useAiShopping';

export default function AIFashionMatching() {
  const { productId } = useParams();
  const productQuery = useProduct(productId);
  const fashionMatches = useFashionMatches();
  const product = productQuery.data;

  useEffect(() => {
    if (product) {
      fashionMatches.mutate({ itemDescription: `${product.name} (${product.categoryName})` });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [product?.id]);

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: 'AI Fashion Matching' },
        ]}
        title="AI Fashion Matching"
        description={product ? `Complete the look built around ${product.name}.` : 'Complete the look with AI-matched pieces.'}
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <AsyncState isLoading={productQuery.isLoading} isError={productQuery.isError} error={productQuery.error}>
        {product && (
          <div className="mb-8 flex items-center gap-4 rounded-xl border border-border bg-surface p-4">
            <span className="flex h-14 w-14 shrink-0 items-center justify-center rounded-lg bg-background text-[10px] text-body/40">
              Item
            </span>
            <div>
              <p className="text-sm font-semibold text-heading">{product.name}</p>
              <p className="text-xs text-body/60">
                {product.categoryName} · by {product.producerName}
              </p>
            </div>
          </div>
        )}
      </AsyncState>

      <AsyncState isLoading={fashionMatches.isPending} isError={fashionMatches.isError} error={fashionMatches.error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {(fashionMatches.data || []).map((match, i) => (
            <div key={i} className="rounded-xl border border-border bg-surface p-4">
              <Badge tone="secondary">{match.matchType}</Badge>
              <p className="mt-2 text-sm font-semibold text-heading">{match.itemName}</p>
              <p className="mt-1 text-xs text-body/60">{match.reason}</p>
            </div>
          ))}
          {fashionMatches.data?.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No matches found for this item.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
