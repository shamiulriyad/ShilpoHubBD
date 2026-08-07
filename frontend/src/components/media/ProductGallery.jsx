import { useState } from 'react';

export default function ProductGallery({ productName = 'Product', thumbnailCount = 4 }) {
  const [active, setActive] = useState(0);

  return (
    <div className="space-y-3">
      <div className="flex aspect-square items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
        {productName} Image {active + 1}
      </div>
      <div className="grid grid-cols-4 gap-3">
        {Array.from({ length: thumbnailCount }).map((_, i) => (
          <button
            key={i}
            type="button"
            onClick={() => setActive(i)}
            className={`flex aspect-square items-center justify-center rounded-lg border text-[10px] transition ${
              active === i ? 'border-primary text-primary' : 'border-border text-body/30 hover:border-primary/40'
            }`}
          >
            Thumb {i + 1}
          </button>
        ))}
      </div>
    </div>
  );
}
