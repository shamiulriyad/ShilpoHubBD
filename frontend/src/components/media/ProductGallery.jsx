import { useState } from 'react';
import SafeImage from './SafeImage';
import CardMedia, { resolveMediaUrl } from './CardMedia';
export default function ProductGallery({ productName = 'Product', images = [] }) {
  const [active, setActive] = useState(0);
  const photos = [...new Set((images || []).map(resolveMediaUrl).filter(Boolean))];
  const selected = Math.min(active, Math.max(0, photos.length - 1));
  return <div className="space-y-3">
    <div className="overflow-hidden rounded-2xl border border-border"><CardMedia src={photos[selected]} name={productName}/></div>
    {photos.length > 1 && <div className="grid grid-cols-4 gap-3">{photos.map((src,i) => <button key={src} type="button" aria-label={`View product photo ${i+1}`} aria-pressed={selected === i} onClick={() => setActive(i)} className={`aspect-square overflow-hidden rounded-lg border-2 ${selected === i ? 'border-primary' : 'border-border'}`}><SafeImage src={src} alt={`${productName}, photo ${i+1}`} className="h-full w-full object-cover"/></button>)}</div>}
  </div>;
}
