import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { useProducts, useProductAdminMutations } from '../../hooks/useProducts';
import { useAuctions } from '../../hooks/useAuctions';

const verificationTone = { Pending: 'secondary', Verified: 'success', Rejected: 'neutral' };

export default function MarketplaceMonitoring() {
  const productsQuery = useProducts({ pageSize: 100 });
  const auctionsQuery = useAuctions({ status: 'Active', pageSize: 1 });
  const { setFeatured, setHandmadeVerification } = useProductAdminMutations();

  const products = productsQuery.data?.items || [];
  const pending = products.filter((p) => p.handmadeVerificationStatus === 'Pending' || !p.handmadeVerificationStatus);

  return (
    <div>
      <PageHeader title="Marketplace Monitoring" description="Oversee listings and handmade-authenticity verification." />

      <div className="mb-8 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Total Listings" value={productsQuery.data?.totalCount ?? '—'} />
        <StatCard label="Active Auctions" value={auctionsQuery.data?.totalCount ?? '—'} />
        <StatCard label="Pending Verification" value={pending.length} />
        <StatCard label="Featured Products" value={products.filter((p) => p.isFeatured).length} />
      </div>

      <p className="mb-3 text-sm font-semibold text-heading">Handmade Verification Queue</p>
      <AsyncState isLoading={productsQuery.isLoading} isError={productsQuery.isError} error={productsQuery.error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {pending.map((product) => (
            <div key={product.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{product.name}</p>
                <p className="text-xs text-body/60">{product.categoryName} · {product.producerName} · ৳ {product.price.toLocaleString()}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={verificationTone[product.handmadeVerificationStatus] || 'secondary'}>
                  {product.handmadeVerificationStatus || 'Pending'}
                </Badge>
                <Button variant="primary" onClick={() => setHandmadeVerification.mutate({ id: product.id, payload: { status: 'Verified' } })}>
                  Verify
                </Button>
                <Button variant="secondary" onClick={() => setHandmadeVerification.mutate({ id: product.id, payload: { status: 'Rejected' } })}>
                  Reject
                </Button>
                <Button variant="secondary" onClick={() => setFeatured.mutate({ id: product.id, isFeatured: !product.isFeatured })}>
                  {product.isFeatured ? 'Unfeature' : 'Feature'}
                </Button>
              </div>
            </div>
          ))}
          {pending.length === 0 && <p className="p-6 text-center text-sm text-body/60">No products awaiting verification.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
