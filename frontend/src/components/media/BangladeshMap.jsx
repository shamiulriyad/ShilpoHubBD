const bangladeshMapUrl = 'https://www.openstreetmap.org/export/embed.html?bbox=88.00%2C20.50%2C92.80%2C26.80&layer=mapnik&marker=23.6850%2C90.3563';
const mapPageUrl = 'https://www.openstreetmap.org/#map=7/23.685/90.356';

export default function BangladeshMap({ selectedDistrict }) {
  return (
    <div className="relative aspect-[16/10] overflow-hidden rounded-3xl border border-title/10 bg-title shadow-[0_18px_38px_rgba(23,59,53,0.16)]">
      <iframe
        title="Interactive map of Bangladesh"
        src={bangladeshMapUrl}
        loading="lazy"
        className="absolute inset-0 h-full w-full border-0"
        referrerPolicy="no-referrer"
      />
      <div className="pointer-events-none absolute inset-x-0 top-0 flex items-start justify-between bg-gradient-to-b from-title/55 to-transparent p-4">
        <span className="rounded-full bg-title/80 px-3 py-1.5 text-[11px] font-semibold text-surface shadow-sm backdrop-blur">
          {selectedDistrict ? `${selectedDistrict} selected` : 'Bangladesh heritage map'}
        </span>
        <span className="rounded-full bg-surface/90 px-3 py-1.5 text-[10px] font-bold uppercase tracking-[0.12em] text-title shadow-sm">
          Drag · zoom · explore
        </span>
      </div>
      <a
        href={mapPageUrl}
        target="_blank"
        rel="noreferrer"
        className="absolute bottom-3 right-3 rounded-full bg-surface/95 px-3 py-1.5 text-[11px] font-semibold text-title shadow-sm transition hover:bg-primary hover:text-surface"
      >
        Open full map ↗
      </a>
    </div>
  );
}