import { useEffect, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useProduct } from '../../hooks/useProducts';
import { useInteriorPreview } from '../../hooks/useAiShopping';
import { resolveMediaUrl } from '../../components/media/CardMedia';
import { getApiErrorMessage } from '../../utils/apiError';
import SafeImage from '../../components/media/SafeImage';

const roomTypes = ['Living Room', 'Bedroom', 'Dining Room', 'Office'];

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_BYTES = 20 * 1024 * 1024;

export default function AIInteriorPreview() {
  const { productId } = useParams();
  const productQuery = useProduct(productId);
  const interiorPreview = useInteriorPreview();
  const [roomType, setRoomType] = useState(roomTypes[0]);
  const [roomImageFile, setRoomImageFile] = useState(null);
  const [roomImagePreview, setRoomImagePreview] = useState('');
  const [fileError, setFileError] = useState('');
  const fileInput = useRef(null);
  const product = productQuery.data;

  useEffect(() => {
    if (!roomImageFile) {
      setRoomImagePreview('');
      return undefined;
    }
    const objectUrl = URL.createObjectURL(roomImageFile);
    setRoomImagePreview(objectUrl);
    return () => URL.revokeObjectURL(objectUrl);
  }, [roomImageFile]);

  const chooseFile = (event) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    if (!file) return;

    if (!ALLOWED_TYPES.includes(file.type)) {
      setFileError('Choose a JPG, PNG or WebP image.');
      return;
    }
    if (file.size > MAX_BYTES) {
      setFileError('Choose an image smaller than 20 MB.');
      return;
    }

    setFileError('');
    interiorPreview.reset();
    setRoomImageFile(file);
  };

  const handleGenerate = () => {
    if (!product || !roomImageFile) return;

    const formData = new FormData();
    formData.append('productId', product.id);
    formData.append('roomImage', roomImageFile);
    formData.append('style', roomType);

    interiorPreview.mutate(formData);
  };

  const result = interiorPreview.data;
  const errorMessage = interiorPreview.isError ? getApiErrorMessage(interiorPreview.error) : '';
  const previewImageUrl = result ? resolveMediaUrl(result.previewImageUrl) : undefined;

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

              <p className="text-sm font-semibold text-heading">2. Upload a photo of your room</p>
              <div className="flex items-center gap-3 rounded-lg border border-border bg-background p-3">
                {roomImagePreview ? (
                  <SafeImage src={roomImagePreview} alt="Selected room" className="h-12 w-12 shrink-0 rounded-md object-cover" />
                ) : (
                  <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-md bg-surface text-[10px] text-body/40">
                    Room
                  </span>
                )}
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-medium text-heading">{roomImageFile ? roomImageFile.name : 'No image selected'}</p>
                  <p className="text-xs text-body/60">JPG, PNG or WebP, up to 20 MB.</p>
                </div>
                <input
                  ref={fileInput}
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  className="sr-only"
                  onChange={chooseFile}
                  aria-label="Choose a room photo"
                  tabIndex={-1}
                />
                <Button type="button" variant="secondary" onClick={() => fileInput.current?.click()}>
                  {roomImageFile ? 'Change' : 'Choose photo'}
                </Button>
              </div>
              {fileError && <p role="alert" className="text-sm text-error">{fileError}</p>}

              <p className="text-sm font-semibold text-heading">3. Product to preview</p>
              <div className="flex items-center gap-3 rounded-lg border border-border bg-background p-3">
                <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-md bg-surface text-[10px] text-body/40">
                  Item
                </span>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-medium text-heading">{product.name}</p>
                  <p className="text-xs text-body/60">{product.categoryName}</p>
                </div>
              </div>

              <Button
                variant="primary"
                className="w-full"
                onClick={handleGenerate}
                disabled={!roomImageFile || interiorPreview.isPending}
              >
                {interiorPreview.isPending ? 'Generating…' : 'Generate Preview'}
              </Button>
              {!roomImageFile && <p className="text-xs text-body/50">Select a room photo to continue.</p>}
              {errorMessage && <p role="alert" className="text-sm text-error">{errorMessage}</p>}
            </div>

            <div className="rounded-xl border border-border bg-surface p-6">
              <p className="mb-4 text-sm font-semibold text-heading">Preview</p>
              <div className="flex aspect-video items-center justify-center rounded-lg border border-dashed border-border bg-background/40 text-center text-xs text-body/40">
                {previewImageUrl ? (
                  <SafeImage src={previewImageUrl} alt="AI room preview" className="h-full w-full rounded-lg object-cover" />
                ) : (
                  'Your AI-generated room preview will appear here'
                )}
              </div>
              {result?.description && previewImageUrl && (
                <p className="mt-3 text-xs text-body/50">{result.description}</p>
              )}
            </div>
          </div>
        )}
      </AsyncState>
    </div>
  );
}
