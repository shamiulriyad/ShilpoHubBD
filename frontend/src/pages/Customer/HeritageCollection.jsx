import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader, AsyncState } from '../../components/ui';
import { useOrders } from '../../hooks/useOrders';
import { useArCraftScan } from '../../hooks/useArCraftScan';

export default function HeritageCollection() {
  const deliveredQuery = useOrders({ status: 'Delivered', pageSize: 50 });
  const [code, setCode] = useState('');
  const scan = useArCraftScan();

  const orders = deliveredQuery.data?.items || [];
  const itemCount = orders.reduce((sum, order) => sum + order.itemCount, 0);

  const handleScan = (event) => {
    event.preventDefault();
    scan.mutate(code);
  };

  const result = scan.data;

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Heritage Collection' }]}
        title="Your Heritage Collection"
        description={`${itemCount} item${itemCount === 1 ? '' : 's'} from ${orders.length} delivered order${orders.length === 1 ? '' : 's'}.`}
      />

      <AsyncState isLoading={deliveredQuery.isLoading} isError={deliveredQuery.isError} error={deliveredQuery.error}>
        <div className="mb-12 divide-y divide-border rounded-xl border border-border bg-surface">
          {orders.map((order) => (
            <div key={order.id} className="flex items-center justify-between p-4 text-sm">
              <div>
                <p className="font-medium text-heading">{order.orderNumber}</p>
                <p className="text-xs text-body/60">
                  {order.itemCount} item{order.itemCount > 1 ? 's' : ''} · delivered {new Date(order.createdAt).toLocaleDateString()}
                </p>
              </div>
              <p className="font-semibold text-primary">৳ {order.total.toLocaleString()}</p>
            </div>
          ))}
          {orders.length === 0 && (
            <p className="p-6 text-center text-sm text-body/60">
              Your heritage collection is empty. Shop the marketplace and your delivered orders will show up here.
            </p>
          )}
        </div>
      </AsyncState>

      <SectionHeader
        eyebrow="Authenticity"
        title="Verify a Product"
        description="Enter the QR/scan code printed on a heritage product to see its full origin story, producer and certification."
      />

      <form onSubmit={handleScan} className="mb-6 flex flex-wrap gap-2">
        <input
          required
          placeholder="Enter product scan code"
          value={code}
          onChange={(event) => setCode(event.target.value)}
          className="min-w-[240px] flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <Button type="submit" variant="primary" disabled={scan.isPending}>
          {scan.isPending ? 'Verifying…' : 'Verify'}
        </Button>
      </form>

      {result && (
        <div className="rounded-xl border border-border bg-surface p-5">
          {result.isRecognized ? (
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <Badge tone={result.isCertified ? 'success' : 'secondary'}>
                  {result.isCertified ? 'Certified Authentic' : 'Recognized'}
                </Badge>
                {result.certificateNumber && (
                  <span className="font-mono text-xs text-body/60">{result.certificateNumber}</span>
                )}
              </div>
              {result.product && (
                <div>
                  <p className="text-sm font-semibold text-heading">{result.product.name}</p>
                  <p className="text-xs text-body/60">
                    By {result.product.producerName} · {result.product.districtName} · {result.product.categoryName}
                  </p>
                  <p className="mt-2 text-sm text-body/70">{result.product.description}</p>
                </div>
              )}
              {result.craftStory && <p className="text-sm text-body/70">{result.craftStory.summary}</p>}
              {result.traceabilitySummary && (
                <p className="text-sm text-body/70">
                  <span className="font-medium text-heading">Traceability: </span>
                  {result.traceabilitySummary}
                </p>
              )}
            </div>
          ) : (
            <p className="text-sm text-body/60">This code couldn't be verified — it may be invalid or revoked.</p>
          )}
        </div>
      )}
    </div>
  );
}
