export default function CraftHeritageDetails({ craft }) {
  if (!craft) return null;
  const facts = [
    ['Heritage type', craft.type], ['Region', craft.region],
    ['Materials', craft.materials], ['How it is made', craft.process], ['Products', craft.products],
  ];
  return (
    <article className="mb-10 overflow-hidden rounded-2xl border border-border bg-surface">
      <div className="border-b border-border bg-primary/5 p-6 sm:p-8">
        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-primary">Living heritage · Bangladesh</p>
        <h2 className="mt-3 text-2xl font-semibold text-heading">{craft.name}</h2>
        <p className="mt-3 max-w-3xl leading-relaxed text-body/80">{craft.summary}</p>
        {craft.aliases?.length > 0 && <p className="mt-3 text-sm text-body/65">Also known as {craft.aliases.join(' · ')}</p>}
      </div>
      <div className="grid gap-8 p-6 sm:p-8 lg:grid-cols-[1.4fr_1fr]">
        <div className="space-y-6">
          {craft.history && <section><h3 className="font-semibold text-heading">History & community</h3><p className="mt-2 text-sm leading-7 text-body/80">{craft.history}</p></section>}
          <dl className="grid gap-5 sm:grid-cols-2">
            {facts.map(([label, value]) => <div key={label}><dt className="text-xs font-semibold uppercase tracking-wide text-body/60">{label}</dt><dd className="mt-1 text-sm leading-6 text-heading">{value}</dd></div>)}
          </dl>
          {craft.story && <p className="border-l-2 border-primary pl-4 text-base leading-7 text-heading">{craft.story}</p>}
          <section><h3 className="font-semibold text-heading">Explore respectfully</h3><p className="mt-2 text-sm leading-7 text-body/80">{craft.visit || 'Arrange visits with local makers in advance. Ask before photographing people or their work, and discuss materials and techniques directly with the maker.'}</p></section>
        </div>
        <aside className="space-y-5">
          <section className="rounded-xl border border-border bg-background p-5">
            <h3 className="font-semibold text-heading">Recognition</h3>
            <p className="mt-3 text-xs font-semibold uppercase tracking-wide text-primary">Geographical indication (GI)</p>
            <p className="mt-1 text-sm leading-6">{craft.giName ? `Registered heritage: ${craft.giName}` : 'No GI recognition asserted for this broad craft category.'}</p>
            {craft.slug === 'nakshi-kantha' && <p className="mt-2 text-sm leading-6">Regional GI names, such as Jamalpur Nakshi Kantha, are distinct from this general category.</p>}
            <p className="mt-4 text-xs font-semibold uppercase tracking-wide text-primary">UNESCO intangible heritage</p>
            <p className="mt-1 text-sm leading-6">{craft.unesco || 'No UNESCO inscription asserted in this record.'}</p>
            <p className="mt-4 border-t border-border pt-3 text-xs leading-5 text-body/65">Heritage recognition does not certify individual marketplace listings. Ask sellers for evidence of origin.</p>
          </section>
          <section><h3 className="text-sm font-semibold text-heading">Sources & further reading</h3><ul className="mt-3 space-y-3">{craft.sources.map(([label, url]) => <li key={url}><a href={url} target="_blank" rel="noreferrer" className="text-sm text-link underline underline-offset-4">{label} ↗</a></li>)}</ul></section>
        </aside>
      </div>
    </article>
  );
}
