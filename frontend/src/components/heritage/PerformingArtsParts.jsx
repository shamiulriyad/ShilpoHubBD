import { Link } from 'react-router-dom';
import SafeImage from '../media/SafeImage';
import { resolveMediaUrl } from '../media/CardMedia';
import { Badge } from '../ui';
import { routePaths } from '../../routes/routePaths';
import {
  categoryLabel, isValidMediaUrl, isWebUrl, pickLocalized, placeLabel, primaryTitle, secondaryTitle, text, videoEmbedUrl,
} from '../../utils/performingArtsAdapters';

export const detailPath = (slug) => routePaths.explorePerformingArtDetails.replace(':slug', encodeURIComponent(slug));

/** English / বাংলা switch for record content. A radio group, so screen readers announce the current choice. */
export function LanguageToggle({ lang, onChange }) {
  const options = [['en', 'English'], ['bn', 'বাংলা']];
  return (
    <div role="radiogroup" aria-label={`${text('en').language} / ${text('bn').language}`} className="inline-flex rounded-full border border-border bg-surface p-1">
      {options.map(([value, label]) => (
        <button
          key={value}
          type="button"
          role="radio"
          aria-checked={lang === value}
          lang={value}
          onClick={() => onChange(value)}
          className={`rounded-full px-4 py-1.5 text-sm font-semibold transition focus:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 ${lang === value ? 'bg-primary text-white' : 'text-body/70 hover:text-heading'}`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}

/** Renders a localized field; English fallback text is tagged lang="en" so assistive tech pronounces it correctly. */
export function LocalizedText({ record, field, lang, as: Tag = 'p', className = '', showFallbackNote = false }) {
  const { value, lang: valueLang, isFallback } = pickLocalized(record, field, lang);
  if (!value) return null;
  return (
    <>
      {showFallbackNote && isFallback && <p className="mb-2 text-xs italic text-body/60" lang="bn">{text('bn').missingTranslation}</p>}
      <Tag lang={valueLang} className={`whitespace-pre-line ${className}`}>{value}</Tag>
    </>
  );
}

export function PerformingArtCard({ record, lang }) {
  const t = text(lang);
  const title = primaryTitle(record, lang);
  const subtitle = secondaryTitle(record, lang);
  const place = placeLabel(record, lang);
  return (
    <article className="flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:border-primary/50 hover:shadow-sm">
      {record.coverImageUrl ? (
        <figure>
          <SafeImage src={resolveMediaUrl(record.coverImageUrl)} alt="" loading="lazy" className="aspect-video w-full object-cover" />
          {record.coverImageCredit && <figcaption className="px-4 pt-1 text-[11px] text-body/50">{t.credit}: {record.coverImageCredit}</figcaption>}
        </figure>
      ) : (
        <div aria-hidden="true" className="flex aspect-video items-center justify-center bg-gradient-to-br from-primary/10 to-secondary/10 text-3xl text-primary/40">♪</div>
      )}
      <div className="flex flex-1 flex-col gap-2 p-4">
        <div className="flex flex-wrap gap-2">
          <Badge tone="primary"><span lang={lang}>{categoryLabel(record.category, lang)}</span></Badge>
          {record.isFeatured && <Badge tone="secondary"><span lang={lang}>{t.featured}</span></Badge>}
        </div>
        <h3 className="text-base font-semibold text-heading">
          <Link to={detailPath(record.slug)} className="focus:outline-none focus-visible:underline">
            <span lang={lang === 'bn' && record.titleBn ? 'bn' : 'en'}>{title}</span>
          </Link>
        </h3>
        {subtitle && <p className="-mt-1 text-sm text-body/60" lang={lang === 'bn' ? 'en' : 'bn'}>{subtitle}</p>}
        {place && <p className="text-xs text-body/60" lang={lang}>{place}</p>}
        <LocalizedText record={record} field="summary" lang={lang} className="line-clamp-3 text-sm text-body/75" />
        <div className="mt-auto pt-2">
          <Link to={detailPath(record.slug)} className="text-sm font-semibold text-primary hover:underline" aria-label={`${t.readMore}: ${title}`}>
            <span lang={lang}>{t.readMore}</span> →
          </Link>
        </div>
      </div>
    </article>
  );
}

function MediaItem({ item, lang }) {
  const t = text(lang);
  const caption = pickLocalized(item, 'caption', lang);
  // Defence in depth: the server validates media URLs, and anything that fails the same rule is never rendered.
  if (!isValidMediaUrl(item.mediaType, item.url)) return null;
  const url = resolveMediaUrl(item.url);
  const label = caption.value || `${item.mediaType} — ${item.credit}`;
  let body = null;
  if (item.mediaType === 'Image') {
    body = <SafeImage src={url} alt={caption.value || ''} loading="lazy" className="max-h-96 w-full rounded-lg object-contain bg-background" />;
  } else if (item.mediaType === 'Audio') {
    body = <audio controls preload="none" src={url} className="w-full" aria-label={label}>{label}</audio>;
  } else {
    const embed = videoEmbedUrl(item.url);
    body = embed ? (
      <div className="aspect-video w-full overflow-hidden rounded-lg">
        <iframe src={embed} title={label} className="h-full w-full" loading="lazy" allow="encrypted-media; picture-in-picture" allowFullScreen referrerPolicy="strict-origin-when-cross-origin" sandbox="allow-scripts allow-same-origin allow-presentation" />
      </div>
    ) : (
      <video controls preload="none" src={url} className="w-full rounded-lg" aria-label={label}>{label}</video>
    );
  }
  return (
    <figure className="rounded-xl border border-border bg-surface p-3">
      {body}
      <figcaption className="mt-2 space-y-0.5 text-xs text-body/70">
        {caption.value && <p lang={caption.lang}>{caption.value}</p>}
        <p>
          <span lang={lang}>{t.credit}</span>: {item.credit}
          {item.license && <> · {item.license}</>}
          {item.sourceUrl && isWebUrl(item.sourceUrl) && (
            <> · <a href={item.sourceUrl} target="_blank" rel="noreferrer noopener" className="text-primary underline"><span lang={lang}>{t.openSource}</span></a></>
          )}
        </p>
      </figcaption>
    </figure>
  );
}

export function MediaGallery({ media, lang }) {
  const items = (media || []).filter((m) => isValidMediaUrl(m.mediaType, m.url));
  if (!items.length) return null;
  return (
    <section aria-labelledby="pa-media" className="mb-10">
      <h2 id="pa-media" className="mb-4 text-lg font-semibold text-heading" lang={lang}>{text(lang).media}</h2>
      <div className="grid gap-4 md:grid-cols-2">{items.map((item) => <MediaItem key={item.id || item.url} item={item} lang={lang} />)}</div>
    </section>
  );
}

const SOURCE_TYPE_LABEL = {
  OfficialInventory: 'Official inventory', Book: 'Book', Article: 'Article', Archive: 'Archive', Website: 'Website', Other: 'Other',
};

export function SourceList({ sources, lang }) {
  const t = text(lang);
  return (
    <section aria-labelledby="pa-sources" className="mb-10 rounded-xl border border-border bg-surface p-5">
      <h2 id="pa-sources" className="mb-3 text-lg font-semibold text-heading" lang={lang}>{t.sources}</h2>
      {sources?.length ? (
        <ol className="list-decimal space-y-3 pl-5 text-sm text-body/80">
          {sources.map((s) => (
            <li key={s.id || s.title} lang="en">
              {s.url && isWebUrl(s.url)
                ? <a href={s.url} target="_blank" rel="noreferrer noopener" className="font-medium text-primary underline">{s.title}</a>
                : <span className="font-medium text-heading">{s.title}</span>}
              <span className="text-body/60">
                {' '}({SOURCE_TYPE_LABEL[s.sourceType] || s.sourceType}{s.publisher ? ` · ${s.publisher}` : ''}{s.accessedOn ? ` · accessed ${String(s.accessedOn).slice(0, 10)}` : ''})
              </span>
              {s.notes && <p className="mt-0.5 text-xs text-body/60">{s.notes}</p>}
            </li>
          ))}
        </ol>
      ) : <p className="text-sm text-body/60" lang={lang}>{t.noSources}</p>}
    </section>
  );
}
