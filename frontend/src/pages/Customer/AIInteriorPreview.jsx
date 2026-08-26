import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useProduct } from '../../hooks/useProducts';
import { useInteriorPreview } from '../../hooks/useAiShopping';

const roomTypes = ['Living Room', 'Bedroom', 'Dining Room', 'Office'];

export default function AIInteriorPreview() {
  const { productId } = useParams();
  const productQuery = useProduct(productId);
  const interiorPreview = useInteriorPreview();
  const [roomType, setRoomType] = useState(roomTypes[0]);
  const product = productQuery.data;

  const handleGenerate = () => {
    if (!product) return;
    interiorPreview.mutate({ productName: product.name, roomType });
  };

  const result = interiorPreview.data;

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

      <AsyncState isLoading={productQuery.isLoading} isError={productQuery.isError} error={productQuery.error}>
        {product && (
          <div className="grid gap-8 lg:grid-cols-2">
            <div className="space-y-4 rounded-xl border border-border bg-surface p-6">
              <p className="text-sm font-semibold text-heading">1. Choose a room type</p>
              <div className="flex flex-wrap gap-2">
                {roomTypes.map((type) => (
                  <button
                    key={type}
                    type="button"
                    onClick={() => setRoomType(type)}
                    className={`rounded-full border px-3 py-1.5 text-sm ${
                      roomType === type ? 'border-primary bg-primary text-surface' : 'border-border bg-background text-body'
                    }`}
                  >
                    {type}
                  </button>
                ))}
              </div>

              <p className="text-sm font-semibold text-heading">2. Product to preview</p>
              <div className="flex items-center gap-3 rounded-lg border border-border bg-background p-3">
                <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-md bg-surface text-[10px] text-body/40">
                  Item
                </span>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-medium text-heading">{product.name}</p>
                  <p className="text-xs text-body/60">{product.categoryName}</p>
                </div>
              </div>

              <Button variant="primary" className="w-full" onClick={handleGenerate} disabled={interiorPreview.isPending}>
                {interiorPreview.isPending ? 'Generating…' : 'Generate Preview'}
              </Button>
            </div>

            <div className="rounded-xl border border-border bg-surface p-6">
              <p className="mb-4 text-sm font-semibold text-heading">Preview</p>
              <div className="flex aspect-video items-center justify-center rounded-lg border border-dashed border-border bg-background/40 text-center text-xs text-body/40">
                {result?.previewImageUrl?.startsWith('http') ? (
                  <img src={result.previewImageUrl} alt="AI room preview" className="h-full w-full rounded-lg object-cover" />
                ) : result ? (
                  result.description
                ) : (
                  'Your AI-generated room preview will appear here'
                )}
              </div>
              {result?.description && result.previewImageUrl?.startsWith('http') && (
                <p className="mt-3 text-xs text-body/50">{result.description}</p>
              )}
            </div>
          </div>
        )}
      </AsyncState>
    </div>
  );
}
