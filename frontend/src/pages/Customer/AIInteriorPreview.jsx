import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button } from '../../components/ui';
import { products } from '../../data/mockData';

export default function AIInteriorPreview() {
  const { productId } = useParams();
  const [generated, setGenerated] = useState(false);
  const product = products.find((p) => p.id === productId) || products[0];

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: 'AI Interior Preview' },
        ]}
        title="AI Interior Preview"
        description="See how a heritage piece looks in your own space before you buy."
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="space-y-4 rounded-xl border border-border bg-surface p-6">
          <p className="text-sm font-semibold text-heading">1. Upload a photo of your room</p>
          <div className="flex aspect-video items-center justify-center rounded-lg border border-dashed border-border text-xs text-body/40">
            Upload room photo
          </div>

          <p className="text-sm font-semibold text-heading">2. Choose a product to preview</p>
          <div className="flex items-center gap-3 rounded-lg border border-border bg-background p-3">
            <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-md bg-surface text-[10px] text-body/40">
              Item
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-heading">{product.name}</p>
              <p className="text-xs text-body/60">{product.category}</p>
            </div>
          </div>

          <Button variant="primary" className="w-full" onClick={() => setGenerated(true)}>
            Generate Preview
          </Button>
        </div>

        <div className="rounded-xl border border-border bg-surface p-6">
          <p className="mb-4 text-sm font-semibold text-heading">Preview</p>
          <div className="flex aspect-video items-center justify-center rounded-lg border border-dashed border-border bg-background/40 text-center text-xs text-body/40">
            {generated
              ? `AI-generated preview of “${product.name}” placed in your room`
              : 'Your AI-generated room preview will appear here'}
          </div>
          {generated && (
            <p className="mt-3 text-xs text-body/50">
              Preview placeholder — AI rendering not implemented.
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
