import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState, Badge } from '../../components/ui';
import { LanguageToggle, LocalizedText, MediaGallery, SourceList } from '../../components/heritage/PerformingArtsParts';
import { useContentLanguage, usePerformingArt } from '../../hooks/usePerformingArts';
import { categoryLabel, placeLabel, primaryTitle, secondaryTitle, splitList, text } from '../../utils/performingArtsAdapters';

function Facts({ record, lang }) {
  const t = text(lang);
  const rows = [
    [t.region, placeLabel(record, lang), lang],
    [t.upazila, record.upazila, 'en'],
    [t.alsoKnownAs, record.alternateNames, 'en'],
    [t.communities, record.communities, 'en'],
    [t.instruments, record.instruments, 'en'],
    [t.viability, record.viabilityNote, 'en'],
    [t.inventoryReference, record.inventoryReference, 'en'],
  ].filter(([, value]) => value);
  if (!rows.length) return null;
  return (
    <dl className="mb-10 grid gap-4 rounded-xl border border-border bg-surface p-5 sm:grid-cols-2">
      {rows.map(([label, value, valueLang]) => (
        <div key={label}>
          <dt className="text-xs font-semibold uppercase tracking-wide text-body/60">{label}</dt>
          <dd className="mt-1 text-sm text-heading" lang={valueLang}>{value}</dd>
        </div>
      ))}
    </dl>
  );
}

export default function PerformingArtDetails() {
  const { slug } = useParams();
  const [lang, setLang] = useContentLanguage();
  const t = text(lang);
  const query = usePerformingArt(slug);
  const record = query.data;
  const notFound = query.error?.response?.status === 404;
  const artists = splitList(record?.artists?.replace(/\n/g, ';'));

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8" lang={lang}>
      {notFound ? (
        <div className="rounded-xl border border-border bg-surface p-8 text-center">
          <h1 className="text-xl font-semibold text-heading">Record not found</h1>
          <p className="mt-2 text-sm text-body/60">This performing-arts record does not exist or is not published.</p>
          <Link to={routePaths.explorePerformingArts} className="mt-4 inline-block text-sm font-semibold text-primary underline">{t.back}</Link>
        </div>
      ) : (
        <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error} loadingText={t.loading}>
          {record && (
            <article>
              <PageHeader
                breadcrumbs={[
                  { label: 'Home', path: routePaths.home },
                  { label: 'Explore', path: routePaths.explore },
                  { label: 'Performing Arts', path: routePaths.explorePerformingArts },
                  { label: record.titleEn },
                ]}
                title={primaryTitle(record, lang)}
                description={secondaryTitle(record, lang) || undefined}
              />
              <div className="mb-6 flex flex-wrap items-center gap-3">
                <LanguageToggle lang={lang} onChange={setLang} />
                <Badge tone="primary">{categoryLabel(record.category, lang)}</Badge>
                {record.sources?.length > 0 && <Badge tone="success">{t.sources}: {record.sources.length}</Badge>}
              </div>

              <section aria-labelledby="pa-about" className="mb-8">
                <h2 id="pa-about" className="mb-3 text-lg font-semibold text-heading">{t.about}</h2>
                <LocalizedText record={record} field="summary" lang={lang} showFallbackNote className="mb-4 text-base leading-7 text-heading" />
                <LocalizedText record={record} field="description" lang={lang} className="text-sm leading-7 text-body/80" />
              </section>

              {(record.regionalContextEn || record.regionalContextBn) && (
                <section aria-labelledby="pa-region" className="mb-8">
                  <h2 id="pa-region" className="mb-3 text-lg font-semibold text-heading">{t.regionalContext}</h2>
                  <LocalizedText record={record} field="regionalContext" lang={lang} showFallbackNote className="text-sm leading-7 text-body/80" />
                </section>
              )}

              <Facts record={record} lang={lang} />

              {artists.length > 0 && (
                <section aria-labelledby="pa-artists" className="mb-10">
                  <h2 id="pa-artists" className="mb-3 text-lg font-semibold text-heading">{t.artists}</h2>
                  <ul className="flex flex-wrap gap-2" lang="en">
                    {artists.map((name) => <li key={name} className="rounded-full border border-border bg-surface px-3 py-1.5 text-sm">{name}</li>)}
                  </ul>
                </section>
              )}

              <MediaGallery media={record.media} lang={lang} />
              <SourceList sources={record.sources} lang={lang} />

              <Link to={routePaths.explorePerformingArts} className="text-sm font-semibold text-primary hover:underline">← {t.back}</Link>
            </article>
          )}
        </AsyncState>
      )}
    </div>
  );
}
