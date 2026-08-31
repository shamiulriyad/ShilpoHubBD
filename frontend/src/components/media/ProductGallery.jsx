import { useState } from 'react';

export default function ProductGallery({ productName = 'Product', images = [], thumbnailCount = 4 }) {
  const [active, setActive] = useState(0);
  const hasImages = Array.isArray(images) && images.length > 0;
  const current = hasImages ? images[Math.min(active, images.length - 1)] : null;
  const thumbs = hasImages ? images : Array.from({ length: thumbnailCount });

  return (
    <div className="space-y-3">
      <div className="flex aspect-square items-center justify-center overflow-hidden rounded-2xl border border-border bg-background text-sm text-body/40">
        {current ? (
          <img src={current} alt={`${productName} ${active + 1}`} className="h-full w-full object-cover" />
        ) : (
          `${productName} image ${active + 1}`
        )}
      </div>
      <div className="grid grid-cols-4 gap-3">
        {thumbs.slice(0, hasImages ? images.length : thumbnailCount).map((thumb, i) => (
          <button
            key={hasImages ? thumb : i}
            type="button"
            onClick={() => setActive(i)}
            className={`flex aspect-square items-center justify-center overflow-hidden rounded-lg border text-xs text-body/40 ${
              active === i ? 'border-primary' : 'border-border'
            }`}
          >
            {hasImages ? (
              <img src={thumb} alt={`${productName} thumbnail ${i + 1}`} className="h-full w-full object-cover" />
            ) : (
              i + 1
            )}
          </button>
        ))}
      </div>
    </div>
  );
}
