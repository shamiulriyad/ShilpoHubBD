export default function BrandLogo({ size = 'md', showName = true, inverse = false, className = '' }) {
  const dimensions = size === 'sm' ? 'h-8 w-8' : size === 'lg' ? 'h-12 w-12' : 'h-10 w-10';

  return (
    <span className={`inline-flex items-center gap-3 ${className}`}>
      <span className={`flex shrink-0 items-center justify-center overflow-hidden rounded-xl shadow-[0_8px_20px_rgba(168,79,45,0.24)] ${dimensions}`}>
        <svg viewBox="0 0 48 48" role="img" aria-label="ShilpoHub artisan loom logo" className="h-full w-full">
          <rect width="48" height="48" rx="13" fill="#AD4F2B" />
          <path d="M11 14h26M14 14v22M34 14v22M11 35h26" stroke="#FFF8EF" strokeWidth="2.5" strokeLinecap="round" />
          <path d="M17 19h14v12H17z" fill="#F2C38E" stroke="#FFF8EF" strokeWidth="1.5" />
          <path d="M19 19v12m4-12v12m4-12v12m4-12v12M17 23h14m-14 4h14" stroke="#AD4F2B" strokeWidth="1.25" />
          <path d="M9 11l4 3m26-3-4 3" stroke="#F2C38E" strokeWidth="2" strokeLinecap="round" />
        </svg>
      </span>
      {showName && <span className={inverse ? 'text-surface' : 'text-title'}>ShilpoHub</span>}
    </span>
  );
}
