import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { SectionHeader } from '../../components/ui';
import { HELPLINE_NUMBER, HELPLINE_TEL } from '../../config/support';
import { useSiteContent } from '../../hooks/useSiteContent';

const shell = 'mx-auto max-w-6xl px-5 lg:px-8';

// Stats, mission/vision, the landscape timeline, stakeholders and capability cards are managed by the
// Super Admin (Admin › CMS › Site Content); an empty group simply hides its section.
export default function AboutPage() {
  const { group } = useSiteContent();
  const stats = group('about-stat').map(item => [item.title, item.subtitle]);
  const flipCards = group('about-purpose').map(item => ({ title: item.title, front: item.body, back: item.extra }));
  const timeline = group('about-landscape').map(item => ({ title: item.title, description: item.body, highlight: item.extra === 'highlight' }));
  const stakeholders = group('about-stakeholder').map(item => item.title);
  const capabilities = group('about-capability').map(item => ({ title: item.title, description: item.body, to: item.linkUrl || '/' }));
  return (
    <div className="overflow-hidden">
      <section className="bg-heading py-20 text-center lg:py-28">
        <div className={shell}>
          <p className="text-xs font-bold uppercase tracking-[.24em] text-[#e8bd98]">About ShilpoHub</p>
          <h1 className="mx-auto mt-6 max-w-3xl text-4xl leading-tight text-background sm:text-5xl lg:text-6xl" style={{ fontFamily: 'Georgia, serif' }}>
            One ecosystem for Bangladesh's living heritage.
          </h1>
          <p className="mx-auto mt-6 max-w-xl text-base leading-8 text-background/70">
            ShilpoHub connects the people who create, sustain, learn, research and celebrate Bangladesh's craft
            traditions — in one place, for the first time.
          </p>
          {stats.length > 0 && (
          <div className="mx-auto mt-12 grid max-w-lg grid-cols-3 gap-6 border-t border-background/15 pt-8">
            {stats.map(([value, label]) => (
              <div key={label}>
                <p className="text-3xl font-bold text-background sm:text-4xl" style={{ fontFamily: 'Georgia, serif' }}>{value}</p>
                <p className="mt-1 text-xs leading-5 text-background/60">{label}</p>
              </div>
            ))}
          </div>
          )}
        </div>
      </section>

      {flipCards.length > 0 && (
      <section className={`${shell} py-16 lg:py-20`}>
        <SectionHeader eyebrow="What drives us" title="Mission & vision" description="Hover a card to see it from the other side." />
        <div className="grid gap-6 sm:grid-cols-2">
          {flipCards.map((card) => (
            <div key={card.title} className="group h-56 [perspective:1200px]">
              <div className="relative h-full w-full transition-transform duration-700 [transform-style:preserve-3d] group-hover:[transform:rotateY(180deg)]">
                <div className="absolute inset-0 flex flex-col justify-center rounded-2xl border border-border bg-surface p-8 [backface-visibility:hidden]">
                  <p className="text-xs font-bold uppercase tracking-[.2em] text-primary">{card.title}</p>
                  <p className="mt-3 text-xl leading-snug text-heading" style={{ fontFamily: 'Georgia, serif' }}>{card.front}</p>
                </div>
                <div className="absolute inset-0 flex flex-col justify-center rounded-2xl bg-primary p-8 text-white [backface-visibility:hidden] [transform:rotateY(180deg)]">
                  <p className="text-sm leading-7">{card.back}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>
      )}

      {timeline.length > 0 && (
      <section className="border-y border-border bg-surface py-16 lg:py-20">
        <div className={shell}>
          <SectionHeader eyebrow="Why ShilpoHub" title="Nothing like it exists — yet." description="Pieces of this already exist. No one has connected them." />
          <div className="ml-3 border-l-2 border-primary/25 pl-8">
            {timeline.map((item, i) => (
              <div key={item.title} className="relative pb-8 last:pb-0">
                <span className={`absolute -left-[2.6rem] flex h-7 w-7 items-center justify-center rounded-full text-[11px] font-bold ${item.highlight ? 'bg-primary text-white' : 'bg-background text-heading border border-border'}`}>{i + 1}</span>
                <h3 className={`font-semibold ${item.highlight ? 'text-primary' : 'text-heading'}`}>{item.title}</h3>
                <p className="mt-1 max-w-xl text-sm leading-6 text-muted">{item.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>
      )}

      {stakeholders.length > 0 && (
      <section className={`${shell} py-16 lg:py-20`}>
        <SectionHeader eyebrow="Who it serves" title="A multi-role ecosystem." description="Every part of the heritage economy has a place here." />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {stakeholders.map((stakeholder, i) => (
            <div key={stakeholder} className="[perspective:800px]">
              <div className="rounded-xl border border-border bg-surface p-5 text-center transition duration-300 will-change-transform hover:-translate-y-1 hover:[transform:rotateX(6deg)_rotateY(-6deg)] hover:shadow-lg">
                <p className="text-2xl font-bold text-primary/25" style={{ fontFamily: 'Georgia, serif' }}>{String(i + 1).padStart(2, '0')}</p>
                <p className="mt-2 text-sm font-semibold leading-5 text-heading">{stakeholder}</p>
              </div>
            </div>
          ))}
        </div>
      </section>
      )}

      {capabilities.length > 0 && (
      <section className="border-y border-border bg-surface py-16 lg:py-20">
        <div className={shell}>
          <SectionHeader eyebrow="Platform" title="What you can explore." description="Drag sideways for a quick tour of what's open to everyone." />
        </div>
        <div className="flex gap-5 overflow-x-auto px-5 pb-4 [scrollbar-width:thin] lg:px-8">
          {capabilities.map((capability) => (
            <Link key={capability.title} to={capability.to} className="group flex w-64 shrink-0 flex-col rounded-2xl border border-border bg-background p-6 transition hover:border-primary/40 hover:shadow-sm">
              <span className="h-1 w-10 rounded-full bg-primary transition group-hover:w-16" />
              <p className="mt-5 font-semibold text-heading">{capability.title} <span aria-hidden="true">↗</span></p>
              <p className="mt-2 text-sm leading-6 text-muted">{capability.description}</p>
            </Link>
          ))}
        </div>
      </section>
      )}

      <section className="border-b border-border bg-background py-12 text-center">
        <div className={shell}>
          <p className="text-xs font-bold uppercase tracking-[.2em] text-muted">Need help?</p>
          <p className="mt-3 text-lg text-heading">Our 24/7 helpline is always open for every member.</p>
          <a href={HELPLINE_TEL} className="mt-2 inline-block text-2xl font-semibold text-primary hover:underline">{HELPLINE_NUMBER}</a>
        </div>
      </section>

      <section className="bg-primary py-16 text-center lg:py-20">
        <div className={shell}>
          <p className="text-xs font-bold uppercase tracking-[.2em] text-white/70">Be part of the story</p>
          <h2 className="mx-auto mt-4 max-w-xl text-3xl leading-tight text-white sm:text-4xl" style={{ fontFamily: 'Georgia, serif' }}>A place for makers, explorers and you.</h2>
          <p className="mx-auto mt-3 max-w-md text-sm leading-6 text-white/80">Join the community and discover what you can create together.</p>
          <Link className="mt-7 inline-flex items-center justify-center rounded-full bg-background px-6 py-3 text-sm font-semibold text-primary transition hover:bg-background/90" to={routePaths.register}>Join ShilpoHub →</Link>
        </div>
      </section>
    </div>
  );
}
