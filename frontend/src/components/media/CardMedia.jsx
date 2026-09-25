import SafeImage from './SafeImage';
import { API_BASE_URL } from '../../config/runtime';

export function resolveMediaUrl(value) {
  if (typeof value !== 'string' || !value.trim()) return undefined;
  try {
    const api = new URL(API_BASE_URL, window.location.origin);
    const source = value.trim();
    const url = new URL(source, source.startsWith('/images/') ? window.location.origin : `${api.origin}/`);
    return ['http:', 'https:'].includes(url.protocol) ? url.href : undefined;
  } catch {
    return undefined;
  }
}

function Illustration({ name, kind }) {
  return <div className="absolute inset-0 flex flex-col items-center justify-center bg-background px-6 text-center">
    <div className="flex h-14 w-14 items-center justify-center rounded-xl border border-border bg-surface text-primary/60"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.25" className="h-6 w-6" aria-hidden="true"><rect x="3" y="3" width="18" height="18" rx="3"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="m4 17 5-5 4 4 3-3 5 5"/></svg></div>
    <span className="mt-3 text-xs font-medium text-muted">{kind==='product' ? 'Product photo unavailable' : 'Photo unavailable'}</span>
    {kind==='product' && <span className="mt-1 text-xs text-muted">View details from the maker</span>}
  </div>;
}

export default function CardMedia({ src, name = '', kind = 'product', category = '' }) {
  return (
    <div className="relative aspect-[4/3] overflow-hidden bg-background">
      <SafeImage src={resolveMediaUrl(src)} alt={name} loading="lazy" className="h-full w-full object-cover transition duration-500 group-hover:scale-[1.03]" fallbackLabel={<Illustration name={`${name} ${category}`} kind={kind} />} />
    </div>
  );
}
