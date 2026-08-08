import { useState } from 'react';

export default function Product360Viewer({ productName = 'Product' }) {
  const [angle, setAngle] = useState(0);

  return (
    <div className="space-y-3">
      <div className="flex aspect-square flex-col items-center justify-center gap-2 rounded-2xl border border-border bg-background text-sm text-body/40">
        <span>360° view of {productName}</span>
        <span className="text-xs text-body/30">{angle}°</span>
      </div>
      <input
        type="range"
        min="0"
        max="359"
        value={angle}
        onChange={(event) => setAngle(Number(event.target.value))}
        className="w-full accent-primary"
        aria-label="Rotate product"
      />
      <p className="text-center text-xs text-body/50">Drag the slider to rotate the product</p>
    </div>
  );
}
