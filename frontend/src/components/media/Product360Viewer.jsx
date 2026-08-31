import { useState } from 'react';

export default function Product360Viewer({ productName = 'Product', images = [] }) {
  const frames = Array.isArray(images) ? images : [];
  const hasFrames = frames.length > 0;
  const [frame, setFrame] = useState(0);
  const [angle, setAngle] = useState(0);

  const onScrub = (event) => {
    const value = Number(event.target.value);
    setAngle(value);
    if (hasFrames) {
      setFrame(Math.min(frames.length - 1, Math.floor((value / 360) * frames.length)));
    }
  };

  return (
    <div className="space-y-3">
      <div className="flex aspect-square flex-col items-center justify-center gap-2 overflow-hidden rounded-2xl border border-border bg-background text-sm text-body/40">
        {hasFrames ? (
          <img src={frames[frame]} alt={`${productName} 360° frame ${frame + 1}`} className="h-full w-full object-cover" />
        ) : (
          <>
            <span>360° view of {productName}</span>
            <span className="text-xs text-body/30">{angle}°</span>
          </>
        )}
      </div>
      <input
        type="range"
        min="0"
        max="360"
        value={angle}
        onChange={onScrub}
        className="w-full accent-primary"
        aria-label={`Rotate ${productName}`}
      />
      <p className="text-center text-xs text-body/40">
        {hasFrames ? `Drag to rotate · frame ${frame + 1}/${frames.length}` : 'Drag to rotate'}
      </p>
    </div>
  );
}
