import { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import SafeImage from '../media/SafeImage';
import { SectionHeader } from '../ui';
import { useHeritageGallery } from '../../hooks/useHeritageGallery';

const arrowBtn = 'flex h-10 w-10 shrink-0 items-center justify-center rounded-full border border-border bg-surface text-lg text-heading transition hover:border-primary hover:text-primary disabled:cursor-not-allowed disabled:opacity-30';
const card = 'group relative aspect-[4/5] w-[78%] shrink-0 snap-start overflow-hidden rounded-2xl border border-border bg-surface transition duration-300 hover:shadow-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-4 sm:w-[46%] lg:w-[31%] xl:w-[23%]';

export default function HeritageGallery() {
  const { items } = useHeritageGallery(8);
  const trackRef = useRef(null);
  const [atStart, setAtStart] = useState(true);
  const [atEnd, setAtEnd] = useState(true);

  const updateEdges = useCallback(() => {
    const el = trackRef.current;
    if (!el) return;
    setAtStart(el.scrollLeft <= 4);
    setAtEnd(el.scrollLeft + el.clientWidth >= el.scrollWidth - 4);
  }, []);

  useEffect(() => { updateEdges(); }, [updateEdges, items.length]);

  const scrollByCard = (direction) => {
    const el = trackRef.current;
    if (!el) return;
    const firstCard = el.querySelector('[data-heritage-card]');
    const step = firstCard ? firstCard.getBoundingClientRect().width + 20 : el.clientWidth * 0.8;
    el.scrollBy({ left: direction * step, behavior: 'smooth' });
  };

  if (!items.length) return null;

  return (
    <section className="mx-auto max-w-7xl px-5 py-16 lg:px-8 lg:py-20">
      <SectionHeader
        eyebrow="Heritage Gallery"
        title="Landmarks that carry the country's story."
        description="Forts, temples and monasteries that shaped Bangladesh's heritage. Browse the carousel to see more."
        action={
          <div className="flex gap-2">
            <button type="button" aria-label="Previous heritage sites" className={arrowBtn} onClick={() => scrollByCard(-1)} disabled={atStart}>‹</button>
            <button type="button" aria-label="More heritage sites" className={arrowBtn} onClick={() => scrollByCard(1)} disabled={atEnd}>›</button>
          </div>
        }
      />
      <div
        ref={trackRef}
        onScroll={updateEdges}
        role="list"
        aria-label="Heritage sites"
        className="no-scrollbar flex snap-x snap-mandatory gap-5 overflow-x-auto scroll-smooth pb-2"
      >
        {items.map((item) => (
          <Link key={item.id} to={item.to} data-heritage-card role="listitem" className={card}>
            <SafeImage src={item.imageUrl} alt={item.name} loading="lazy" className="h-full w-full object-cover transition duration-500 group-hover:scale-110" />
            <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/10 to-transparent" />
            <span aria-hidden="true" className="absolute right-4 top-4 flex h-9 w-9 items-center justify-center rounded-full bg-white/90 text-heading transition group-hover:bg-primary group-hover:text-white">↗</span>
            <div className="absolute inset-x-0 bottom-0 p-5">
              {item.placeType && <p className="text-[10px] font-bold uppercase tracking-[.18em] text-white/75">{item.placeType}</p>}
              <h3 className="mt-2 text-lg font-semibold leading-tight text-white" style={{ fontFamily: 'Georgia, serif' }}>{item.name}</h3>
              <p className="mt-1 text-xs text-white/80">{item.districtName}</p>
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}
