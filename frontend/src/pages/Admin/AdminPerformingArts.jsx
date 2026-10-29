import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { Action, ErrorNotice, Modal, Panel, inputClass, display } from './AdminUI';
import { useDistricts } from '../../hooks/useDistricts';
import { useAdminPerformingArts, usePerformingArtAdminMutations } from '../../hooks/usePerformingArts';
import { performingArtsAdminService } from '../../services/performingArtsService';
import { productsService } from '../../services/productsService';
import {
  CATEGORIES, DIVISIONS, MEDIA_TYPES, SOURCE_TYPES, STATUSES, categoryLabel, emptyMedia, emptySource, slugify, toFormValues,
  toSavePayload, validateForm, workflowActions,
} from '../../utils/performingArtsAdapters';
import { detailPath } from '../../components/heritage/PerformingArtsParts';

const STATUS_STYLE = {
  Draft: 'bg-slate-100 text-slate-700',
  InReview: 'bg-amber-100 text-amber-800',
  Published: 'bg-emerald-100 text-emerald-800',
  Archived: 'bg-zinc-200 text-zinc-600',
};
const STATUS_LABEL = { Draft: 'Draft', InReview: 'In review', Published: 'Published', Archived: 'Archived' };
const ACTION_LABEL = { submit: 'Submit for review', publish: 'Publish', return: 'Return to draft', archive: 'Archive', restore: 'Restore to draft', delete: 'Delete' };
const ACTION_HELP = {
  submit: 'The record moves to the review queue. It needs at least one source.',
  publish: 'The record becomes visible on the public Performing Arts page and to the AI Heritage Assistant. Confirm that every statement matches its cited sources.',
  return: 'Explain what must change. The note is kept on the record for the editor.',
  archive: 'The record is removed from the public site and the AI assistant, and becomes read-only.',
  restore: 'The record returns to Draft so it can be edited and reviewed again.',
  delete: 'This permanently removes the record, its sources and its media references.',
};

function StatusBadge({ status }) {
  return <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${STATUS_STYLE[status] || ''}`}>{STATUS_LABEL[status] || status}</span>;
}

function Field({ id, label, required, hint, children, wide }) {
  return (
    <div className={wide ? 'sm:col-span-2' : ''}>
      <label htmlFor={id} className="mb-1.5 block text-sm font-medium">{label}{required ? ' *' : ''}</label>
      {children}
      {hint && <p className="mt-1 text-xs text-body/60">{hint}</p>}
    </div>
  );
}

function TextInput({ id, value, onChange, lang, ...props }) {
  return <input id={id} name={id} lang={lang} className={inputClass} value={value ?? ''} onChange={(e) => onChange(e.target.value)} {...props} />;
}

function TextArea({ id, value, onChange, lang, rows = 3, ...props }) {
  return <textarea id={id} name={id} lang={lang} rows={rows} className={inputClass} value={value ?? ''} onChange={(e) => onChange(e.target.value)} {...props} />;
}

function Section({ title, children }) {
  return (
    <fieldset className="mb-6 rounded-xl border border-border p-4">
      <legend className="px-1 text-sm font-semibold text-heading">{title}</legend>
      <div className="grid gap-4 sm:grid-cols-2">{children}</div>
    </fieldset>
  );
}

function MediaUpload({ index, onUploaded }) {
  const upload = useMutation({ mutationFn: productsService.uploadImage, onSuccess: (result) => onUploaded(result.url) });
  return (
    <div className="sm:col-span-2">
      <label htmlFor={`pa-media-upload-${index}`} className="mb-1 block text-xs font-medium text-body/70">Or upload an image (JPG, PNG or WebP, up to 20 MB)</label>
      <input
        id={`pa-media-upload-${index}`}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        className={inputClass}
        disabled={upload.isPending}
        onChange={(e) => { const file = e.target.files?.[0]; if (file && file.size <= 20 * 1024 * 1024) upload.mutate(file); }}
      />
      {upload.isPending && <p className="mt-1 text-xs text-body/60" role="status">Uploading…</p>}
      <ErrorNotice error={upload.error} />
    </div>
  );
}

function RecordEditor({ initial, onSaved, onCancel }) {
  const districts = useDistricts();
  const { save } = usePerformingArtAdminMutations();
  const [values, setValues] = useState(() => toFormValues(initial));
  const [slugTouched, setSlugTouched] = useState(Boolean(initial?.id));
  const [errors, setErrors] = useState([]);
  const set = (key) => (value) => setValues((old) => ({
    ...old,
    [key]: value,
    ...(key === 'titleEn' && !slugTouched ? { slug: slugify(value) } : {}),
    ...(key === 'districtId' && value ? { division: (districts.data || []).find((d) => d.id === value)?.division || old.division } : {}),
    ...(key === 'division' && old.districtId && (districts.data || []).find((d) => d.id === old.districtId)?.division !== value ? { districtId: '' } : {}),
  }));
  const setRow = (list, index, key) => (value) => setValues((old) => ({
    ...old, [list]: old[list].map((row, i) => (i === index ? { ...row, [key]: value } : row)),
  }));
  const addRow = (list, factory) => setValues((old) => ({ ...old, [list]: [...old[list], factory()] }));
  const removeRow = (list, index) => setValues((old) => ({ ...old, [list]: old[list].filter((_, i) => i !== index) }));
  const moveRow = (list, index, delta) => setValues((old) => {
    const rows = [...old[list]];
    const target = index + delta;
    if (target < 0 || target >= rows.length) return old;
    [rows[index], rows[target]] = [rows[target], rows[index]];
    return { ...old, [list]: rows };
  });

  const districtOptions = (districts.data || []).filter((d) => !values.division || d.division === values.division);
  const isArchived = initial?.status === 'Archived';

  const submit = (event) => {
    event.preventDefault();
    const problems = validateForm(values);
    setErrors(problems);
    if (problems.length) return;
    save.mutate({ id: initial?.id, payload: toSavePayload(values) }, { onSuccess: onSaved });
  };

  return (
    <form onSubmit={submit} noValidate>
      {initial?.id && (
        <div className="mb-4 flex flex-wrap items-center gap-3 rounded-lg bg-background p-3 text-sm">
          <StatusBadge status={initial.status} />
          {initial.reviewNote && <span><span className="font-semibold">Review note:</span> {initial.reviewNote}</span>}
        </div>
      )}
      {isArchived && <p role="alert" className="mb-4 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900">Archived records are read-only. Restore the record to a draft to edit it.</p>}
      <p className="mb-4 text-xs text-body/60">Only enter facts that appear in a cited source. Leave Bangla fields empty rather than adding an unreviewed translation; the public page will show the English text with a notice.</p>

      <fieldset disabled={save.isPending || isArchived}>
        <Section title="Identity">
          <Field id="pa-titleEn" label="Title (English)" required><TextInput id="pa-titleEn" value={values.titleEn} onChange={set('titleEn')} maxLength={200} required /></Field>
          <Field id="pa-titleBn" label="Title (Bangla)" hint="Bangla script only."><TextInput id="pa-titleBn" lang="bn" value={values.titleBn} onChange={set('titleBn')} maxLength={200} /></Field>
          <Field id="pa-slug" label="Slug" required hint="Lowercase letters, numbers and hyphens; used in the page URL.">
            <TextInput id="pa-slug" value={values.slug} onChange={(v) => { setSlugTouched(true); set('slug')(v); }} maxLength={120} required />
          </Field>
          <Field id="pa-category" label="Category" required>
            <select id="pa-category" className={inputClass} value={values.category} onChange={(e) => set('category')(e.target.value)}>
              {CATEGORIES.map((c) => <option key={c.value} value={c.value}>{c.en} / {c.bn}</option>)}
            </select>
          </Field>
          <Field id="pa-alt" label="Alternate names" hint="Comma separated, either language." wide><TextInput id="pa-alt" value={values.alternateNames} onChange={set('alternateNames')} maxLength={500} /></Field>
        </Section>

        <Section title="Description">
          <Field id="pa-summaryEn" label="Summary (English)" required wide><TextArea id="pa-summaryEn" value={values.summaryEn} onChange={set('summaryEn')} maxLength={2000} required /></Field>
          <Field id="pa-summaryBn" label="Summary (Bangla)" wide><TextArea id="pa-summaryBn" lang="bn" value={values.summaryBn} onChange={set('summaryBn')} maxLength={2000} /></Field>
          <Field id="pa-descEn" label="Full description (English)" wide><TextArea id="pa-descEn" rows={6} value={values.descriptionEn} onChange={set('descriptionEn')} maxLength={8000} /></Field>
          <Field id="pa-descBn" label="Full description (Bangla)" wide><TextArea id="pa-descBn" lang="bn" rows={6} value={values.descriptionBn} onChange={set('descriptionBn')} maxLength={8000} /></Field>
          <Field id="pa-ctxEn" label="Regional context (English)" wide><TextArea id="pa-ctxEn" value={values.regionalContextEn} onChange={set('regionalContextEn')} maxLength={4000} /></Field>
          <Field id="pa-ctxBn" label="Regional context (Bangla)" wide><TextArea id="pa-ctxBn" lang="bn" value={values.regionalContextBn} onChange={set('regionalContextBn')} maxLength={4000} /></Field>
        </Section>

        <Section title="Place and people">
          <Field id="pa-division" label="Division" hint="Set automatically from the district when one is chosen.">
            <select id="pa-division" className={inputClass} value={values.division} onChange={(e) => set('division')(e.target.value)}>
              <option value="">Nationwide / not specified</option>
              {DIVISIONS.map((d) => <option key={d.value} value={d.value}>{d.value}</option>)}
            </select>
          </Field>
          <Field id="pa-district" label="District">
            <select id="pa-district" className={inputClass} value={values.districtId} onChange={(e) => set('districtId')(e.target.value)} disabled={districts.isLoading}>
              <option value="">{districts.isLoading ? 'Loading…' : 'Not specified'}</option>
              {districtOptions.map((d) => <option key={d.id} value={d.id}>{d.name} ({d.division})</option>)}
            </select>
            <ErrorNotice error={districts.error} />
          </Field>
          <Field id="pa-upazila" label="Upazila"><TextInput id="pa-upazila" value={values.upazila} onChange={set('upazila')} maxLength={120} /></Field>
          <Field id="pa-communities" label="Associated communities" hint="Comma separated."><TextInput id="pa-communities" value={values.communities} onChange={set('communities')} maxLength={1000} /></Field>
          <Field id="pa-instruments" label="Instruments and associated elements" hint="Comma separated."><TextInput id="pa-instruments" value={values.instruments} onChange={set('instruments')} maxLength={1000} /></Field>
          <Field id="pa-artists" label="Artists / tradition bearers" hint="One per line, only as named in a cited source. No biographies."><TextArea id="pa-artists" value={values.artists} onChange={set('artists')} maxLength={4000} /></Field>
        </Section>

        <Section title="Record details">
          <Field id="pa-viability" label="Present condition (as recorded by the source)"><TextInput id="pa-viability" value={values.viabilityNote} onChange={set('viabilityNote')} maxLength={1000} /></Field>
          <Field id="pa-inventory" label="Inventory reference" hint="e.g. a Cultural Heritage Inventory of Bangladesh unique ID."><TextInput id="pa-inventory" value={values.inventoryReference} onChange={set('inventoryReference')} maxLength={100} /></Field>
          <Field id="pa-order" label="Display order"><TextInput id="pa-order" type="number" min={0} value={values.displayOrder} onChange={set('displayOrder')} /></Field>
          <label className="flex items-center gap-3 self-end py-2 text-sm font-medium">
            <input type="checkbox" checked={Boolean(values.isFeatured)} onChange={(e) => set('isFeatured')(e.target.checked)} className="h-4 w-4 accent-primary" />
            Feature on the Performing Arts page
          </label>
        </Section>

        <fieldset className="mb-6 rounded-xl border border-border p-4">
          <legend className="px-1 text-sm font-semibold text-heading">Sources ({values.sources.length})</legend>
          <p className="mb-3 text-xs text-body/60">At least one source is required before a record can be submitted or published. Prefer official inventories such as heritagehub.gov.bd.</p>
          {values.sources.map((s, i) => (
            <div key={i} className="mb-4 grid gap-3 rounded-lg bg-background p-3 sm:grid-cols-2">
              <Field id={`pa-src-type-${i}`} label="Type">
                <select id={`pa-src-type-${i}`} className={inputClass} value={s.sourceType} onChange={(e) => setRow('sources', i, 'sourceType')(e.target.value)}>
                  {SOURCE_TYPES.map((type) => <option key={type} value={type}>{type}</option>)}
                </select>
              </Field>
              <Field id={`pa-src-title-${i}`} label="Title / citation" required><TextInput id={`pa-src-title-${i}`} value={s.title} onChange={setRow('sources', i, 'title')} maxLength={500} /></Field>
              <Field id={`pa-src-pub-${i}`} label="Publisher"><TextInput id={`pa-src-pub-${i}`} value={s.publisher} onChange={setRow('sources', i, 'publisher')} maxLength={300} /></Field>
              <Field id={`pa-src-url-${i}`} label="URL"><TextInput id={`pa-src-url-${i}`} type="url" value={s.url} onChange={setRow('sources', i, 'url')} maxLength={1000} placeholder="https://" /></Field>
              <Field id={`pa-src-date-${i}`} label="Accessed on"><TextInput id={`pa-src-date-${i}`} type="date" value={s.accessedOn} onChange={setRow('sources', i, 'accessedOn')} /></Field>
              <Field id={`pa-src-notes-${i}`} label="Notes"><TextInput id={`pa-src-notes-${i}`} value={s.notes} onChange={setRow('sources', i, 'notes')} maxLength={1000} /></Field>
              <div className="flex gap-2 sm:col-span-2">
                <Action onClick={() => moveRow('sources', i, -1)} disabled={i === 0} aria-label={`Move source ${i + 1} up`}>↑</Action>
                <Action onClick={() => moveRow('sources', i, 1)} disabled={i === values.sources.length - 1} aria-label={`Move source ${i + 1} down`}>↓</Action>
                <Action danger onClick={() => removeRow('sources', i)}>Remove source</Action>
              </div>
            </div>
          ))}
          <Action onClick={() => addRow('sources', emptySource)}>Add source</Action>
        </fieldset>

        <fieldset className="mb-6 rounded-xl border border-border p-4">
          <legend className="px-1 text-sm font-semibold text-heading">Media ({values.media.length})</legend>
          <p className="mb-3 text-xs text-body/60">Use only media you have the right to publish, with a credit for each item. URLs must be https or an upload path, and match the type (image: jpg/png/webp/gif; audio: mp3/ogg/wav/m4a/aac; video: mp4/webm, YouTube or Vimeo).</p>
          {values.media.map((m, i) => (
            <div key={i} className="mb-4 grid gap-3 rounded-lg bg-background p-3 sm:grid-cols-2">
              <Field id={`pa-media-type-${i}`} label="Type">
                <select id={`pa-media-type-${i}`} className={inputClass} value={m.mediaType} onChange={(e) => setRow('media', i, 'mediaType')(e.target.value)}>
                  {MEDIA_TYPES.map((type) => <option key={type} value={type}>{type}</option>)}
                </select>
              </Field>
              <Field id={`pa-media-url-${i}`} label="URL" required><TextInput id={`pa-media-url-${i}`} value={m.url} onChange={setRow('media', i, 'url')} maxLength={1000} placeholder="https:// or /uploads/…" /></Field>
              {m.mediaType === 'Image' && <MediaUpload index={i} onUploaded={setRow('media', i, 'url')} />}
              <Field id={`pa-media-credit-${i}`} label="Credit" required><TextInput id={`pa-media-credit-${i}`} value={m.credit} onChange={setRow('media', i, 'credit')} maxLength={300} /></Field>
              <Field id={`pa-media-license-${i}`} label="License"><TextInput id={`pa-media-license-${i}`} value={m.license} onChange={setRow('media', i, 'license')} maxLength={200} /></Field>
              <Field id={`pa-media-capEn-${i}`} label="Caption (English)"><TextInput id={`pa-media-capEn-${i}`} value={m.captionEn} onChange={setRow('media', i, 'captionEn')} maxLength={500} /></Field>
              <Field id={`pa-media-capBn-${i}`} label="Caption (Bangla)"><TextInput id={`pa-media-capBn-${i}`} lang="bn" value={m.captionBn} onChange={setRow('media', i, 'captionBn')} maxLength={500} /></Field>
              <Field id={`pa-media-src-${i}`} label="Original source page" wide><TextInput id={`pa-media-src-${i}`} type="url" value={m.sourceUrl} onChange={setRow('media', i, 'sourceUrl')} maxLength={1000} placeholder="https://" /></Field>
              <div className="flex gap-2 sm:col-span-2">
                <Action onClick={() => moveRow('media', i, -1)} disabled={i === 0} aria-label={`Move media ${i + 1} up`}>↑</Action>
                <Action onClick={() => moveRow('media', i, 1)} disabled={i === values.media.length - 1} aria-label={`Move media ${i + 1} down`}>↓</Action>
                <Action danger onClick={() => removeRow('media', i)}>Remove media</Action>
              </div>
            </div>
          ))}
          <Action onClick={() => addRow('media', emptyMedia)}>Add media</Action>
        </fieldset>
      </fieldset>

      {errors.length > 0 && (
        <div role="alert" className="mb-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          <p className="font-semibold">Fix these before saving:</p>
          <ul className="mt-1 list-disc pl-5">{errors.map((e) => <li key={e}>{e}</li>)}</ul>
        </div>
      )}
      <ErrorNotice error={save.error} />
      <div className="flex gap-3">
        <button type="submit" disabled={save.isPending || isArchived} className="rounded-lg bg-primary px-5 py-2.5 text-sm font-semibold text-white disabled:opacity-50">
          {save.isPending ? 'Saving…' : initial?.id ? 'Save changes' : 'Create draft'}
        </button>
        <Action onClick={onCancel} disabled={save.isPending}>Cancel</Action>
      </div>
    </form>
  );
}

function WorkflowDialog({ record, action, onClose }) {
  const { transition, remove } = usePerformingArtAdminMutations();
  const [note, setNote] = useState('');
  const mutation = action === 'delete' ? remove : transition;
  const noteRequired = action === 'return';
  const run = () => {
    if (action === 'delete') remove.mutate(record.id, { onSuccess: onClose });
    else transition.mutate({ id: record.id, action, note }, { onSuccess: onClose });
  };
  return (
    <Modal title={`${ACTION_LABEL[action]}: ${record.titleEn}`} onClose={() => !mutation.isPending && onClose()}>
      <p className="mb-4 text-sm text-body/70">{ACTION_HELP[action]}</p>
      {action !== 'delete' && (
        <div className="mb-4">
          <label htmlFor="pa-workflow-note" className="mb-1.5 block text-sm font-medium">Note{noteRequired ? ' *' : ' (optional)'}</label>
          <textarea id="pa-workflow-note" className={inputClass} rows={3} maxLength={1000} value={note} onChange={(e) => setNote(e.target.value)} />
        </div>
      )}
      <ErrorNotice error={mutation.error} />
      <Action danger={['archive', 'delete'].includes(action)} disabled={mutation.isPending || (noteRequired && !note.trim())} onClick={run}>
        {mutation.isPending ? 'Working…' : `Confirm: ${ACTION_LABEL[action].toLowerCase()}`}
      </Action>
    </Modal>
  );
}

export default function AdminPerformingArts() {
  const [status, setStatus] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [editing, setEditing] = useState(null);
  const [workflow, setWorkflow] = useState(null);
  const query = useAdminPerformingArts({ status: status || undefined, search: search.trim() || undefined, page, pageSize: 20 });
  const open = useMutation({ mutationFn: (id) => performingArtsAdminService.get(id), onSuccess: setEditing });
  const rows = query.data?.items || [];
  const pages = query.data?.totalPages || 1;

  return (
    <Panel>
      <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
        <p className="max-w-xl text-sm text-body/60">Create, review, publish and archive performing-arts heritage records. Only Published records appear on the public page and in AI Heritage Assistant answers.</p>
        <div className="flex flex-wrap gap-2">
          <select aria-label="Filter by status" className={inputClass} value={status} onChange={(e) => { setStatus(e.target.value); setPage(1); }}>
            <option value="">All statuses</option>
            {STATUSES.map((s) => <option key={s} value={s}>{STATUS_LABEL[s]}</option>)}
          </select>
          <input aria-label="Search records" type="search" placeholder="Search…" className={inputClass} value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }} />
          <Action onClick={() => setEditing({})}>Create new</Action>
          <Action onClick={() => query.refetch()} disabled={query.isFetching}>Refresh</Action>
        </div>
      </div>

      <ErrorNotice error={query.error || open.error} />
      {query.isError && <Action onClick={() => query.refetch()}>Retry</Action>}
      {query.isPending ? <p role="status" className="p-8 text-body/60">Loading records…</p> : !query.isError && (
        <>
          <div className="overflow-x-auto rounded-xl border border-border">
            <table className="w-full text-left text-sm">
              <thead className="bg-background">
                <tr>{['Title', 'Category', 'Place', 'Status', 'Sources', 'Updated', 'Actions'].map((h) => <th key={h} scope="col" className="whitespace-nowrap px-4 py-3 font-semibold text-body/70">{h}</th>)}</tr>
              </thead>
              <tbody className="divide-y divide-border">
                {rows.map((row) => (
                  <tr key={row.id} className="align-top hover:bg-background/50">
                    <td className="px-4 py-4">
                      <p className="font-medium text-heading">{row.titleEn}{row.isFeatured && <span className="ml-2 text-xs text-primary">★ Featured</span>}</p>
                      {row.titleBn && <p className="text-body/60" lang="bn">{row.titleBn}</p>}
                      {row.status === 'Published' && <a href={detailPath(row.slug)} target="_blank" rel="noreferrer" className="text-xs text-primary underline">View public page</a>}
                    </td>
                    <td className="px-4 py-4">{categoryLabel(row.category)}</td>
                    <td className="px-4 py-4">{[row.districtName, row.division].filter(Boolean).join(', ') || '—'}</td>
                    <td className="px-4 py-4"><StatusBadge status={row.status} /></td>
                    <td className={`px-4 py-4 ${row.sourceCount === 0 ? 'font-semibold text-red-700' : ''}`}>{row.sourceCount}</td>
                    <td className="whitespace-nowrap px-4 py-4">{display(row.updatedAt, 'updatedAt')}</td>
                    <td className="px-4 py-3">
                      <div className="flex min-w-40 flex-wrap gap-2">
                        <Action disabled={open.isPending} onClick={() => open.mutate(row.id)}>{row.status === 'Archived' ? 'View' : 'Edit'}</Action>
                        {workflowActions(row.status).map((action) => (
                          <Action key={action} danger={['archive', 'delete'].includes(action)} onClick={() => setWorkflow({ record: row, action })}>{ACTION_LABEL[action]}</Action>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {rows.length === 0 && <p className="p-10 text-center text-body/60">No records match this view.</p>}
          </div>
          <div className="mt-4 flex flex-wrap items-center justify-between gap-3 text-sm">
            <span>{query.data?.totalCount ?? 0} records · Page {page} of {Math.max(pages, 1)}</span>
            <div className="flex gap-2">
              <Action disabled={page <= 1 || query.isFetching} onClick={() => setPage(page - 1)}>Previous</Action>
              <Action disabled={page >= pages || query.isFetching} onClick={() => setPage(page + 1)}>Next</Action>
            </div>
          </div>
        </>
      )}

      {editing && (
        <Modal title={editing.id ? `Edit: ${editing.titleEn}` : 'New performing-arts record'} onClose={() => setEditing(null)}>
          <RecordEditor initial={editing} onSaved={() => setEditing(null)} onCancel={() => setEditing(null)} />
        </Modal>
      )}
      {workflow && <WorkflowDialog record={workflow.record} action={workflow.action} onClose={() => setWorkflow(null)} />}
    </Panel>
  );
}
