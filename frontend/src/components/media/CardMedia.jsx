import SafeImage from './SafeImage';
import { API_BASE_URL } from '../../config/runtime';

export function resolveMediaUrl(value) {
  if (typeof value !== 'string' || !value.trim()) return undefined;
  try {
    const api = new URL(API_BASE_URL, window.location.origin);
    const url = new URL(value.trim(), `${api.origin}/`);
    return ['http:', 'https:'].includes(url.protocol) ? url.href : undefined;
  } catch {
    return undefined;
  }
}

function Illustration({ name, kind }) {
  const pottery = /clay|pot|ceramic|terracotta/i.test(name);
  const textile = /kantha|jamdani|weav|textile|saree/i.test(name);
  return (
    <div className="absolute inset-0 flex flex-col items-center justify-center bg-gradient-to-br from-primary-soft via-background to-primary/10 px-4">
      <svg viewBox="0 0 320 210" className="h-[80%] w-full max-w-sm" fill="none" aria-hidden="true">
        <ellipse cx="160" cy="185" rx="86" ry="10" fill="#173b35" opacity=".07" />
        {kind === 'producer' ? <g stroke="#a84f2d" strokeWidth="3">
          <circle cx="160" cy="80" r="31" fill="#ead2bb" />
          <path d="M95 177c0-39 29-62 65-62s65 23 65 62" fill="#d7dfd2" />
          <path d="M140 123l20 25 20-25M160 148v30" />
        </g> : pottery ? <g stroke="#8c432a" strokeWidth="2">
          <path d="M119 65h82l-9 26c2 18 39 35 35 58-4 27-26 36-67 36s-63-9-67-36c-4-23 33-40 35-58z" fill="#bd7855" />
          <ellipse cx="160" cy="65" rx="42" ry="10" fill="#8c432a" />
          <ellipse cx="160" cy="64" rx="32" ry="5" fill="#4e3029" />
          <path d="M108 124q52 22 104 0M102 138q58 23 116 0M106 152q54 20 108 0" stroke="#e8b88a" />
          <path d="M132 98q-28 25-25 40" stroke="#e6ad82" strokeWidth="5" strokeLinecap="round" />
        </g> : textile ? <g>
          <path d="M91 37l147 26-27 116-148-27z" fill="#b86143" />
          <path d="M87 51l146 26M67 137l147 26" stroke="#f3d6a9" strokeWidth="7" />
          {[0, 1, 2, 3].map((row) => [0, 1, 2, 3].map((col) => <path key={`${row}-${col}`} d={`M${99 + col * 30 - row * 4} ${70 + row * 22 + col * 5}l6 9-9 6-6-9z`} fill="#f3dfb9" />))}
          <path d="M64 151l-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14m16-12-5 14" stroke="#b86143" strokeWidth="3" />
        </g> : <g stroke="#a84f2d" strokeWidth="2.5" fill="#ead2bb">
          <path d="M96 84l64-34 64 34v78l-64 30-64-30z" />
          <path d="M96 84l64 34 64-34M160 118v74M129 67l64 34v35" />
        </g>}
      </svg>
      <span className="absolute bottom-3 rounded-full bg-surface/90 px-3 py-1 text-[10px] font-medium text-muted">{kind === 'category' ? 'Craft illustration' : 'Photo unavailable'}</span>
    </div>
  );
}

export default function CardMedia({ src, name = '', kind = 'product', category = '' }) {
  return (
    <div className="relative aspect-[4/3] overflow-hidden bg-background">
      <SafeImage src={resolveMediaUrl(src)} alt={name} loading="lazy" className="h-full w-full object-cover transition duration-500 group-hover:scale-[1.03]" fallbackLabel={<Illustration name={`${name} ${category}`} kind={kind} />} />
    </div>
  );
}
