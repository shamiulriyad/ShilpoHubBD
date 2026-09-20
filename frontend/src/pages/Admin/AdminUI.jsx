import { useEffect, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { superAdminService as api } from '../../services/superAdminService';
import { getApiErrorMessage } from '../../utils/apiError';
import { formPayload } from './adminConfig';
import { productsService } from '../../services/productsService';
import SafeImage from '../../components/media/SafeImage';
import { resolveMediaUrl } from '../../components/media/CardMedia';
export const inputClass = 'w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm text-heading focus:outline-none focus:ring-2 focus:ring-primary/30';
export function Action({
  children,
  danger,
  ...props
}) {
  return <button type="button" className={`rounded-lg border px-3 py-2 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-50 ${danger ? 'border-red-200 text-red-700 hover:bg-red-50' : 'border-border bg-surface text-heading hover:bg-primary/5'}`} {...props}>{children}</button>;
}
export function Panel({
  children,
  className = ''
}) {
  return <div className={`rounded-2xl border border-border bg-surface p-5 shadow-sm ${className}`}>{children}</div>;
}
export function ErrorNotice({
  error
}) {
  return error ? <p role="alert" className="my-3 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-800">{getApiErrorMessage(error, 'The request could not be completed. Please try again.')}</p> : null;
}
export const rowsOf = data => Array.isArray(data) ? data : data?.items || [];
export const labelOf = key => key.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase()).replace(/Id$/, 'ID');
export function display(value, key = '') {
  if (value == null || value === '') return '—';
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  if (Array.isArray(value)) return value.map(v => typeof v === 'object' ? v.name || v.title || '' : v).join(', ') || '—';
  if (typeof value === 'object') return 'Details available';
  if (/At$|Date$/.test(key) && !Number.isNaN(Date.parse(value))) return new Date(value).toLocaleString();
  return String(value);
}
export function useAdminQuery(path, params = {}, enabled = true) {
  return useQuery({
    queryKey: ['super-admin', path, params],
    queryFn: () => api.list(path, params),
    enabled,
    retry: 1
  });
}
export function useAdminAction() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: ({
      method = 'post',
      path,
      payload,
      params
    }) => api.action(method, path, payload, params),
    onSuccess: () => client.invalidateQueries()
  });
}
export function Feedback({
  mutation
}) {
  return <><ErrorNotice error={mutation.error} />{mutation.isSuccess && <p role="status" className="my-3 rounded-lg bg-primary/5 p-3 text-sm">Changes saved successfully.</p>}</>;
}
export function DataTable({
  query,
  columns,
  actions,
  onPage,
  page = 1
}) {
  const rows = rowsOf(query.data);
  const pages = query.data?.totalPages || 1;
  return <><ErrorNotice error={query.error} />{query.isError && <Action onClick={() => query.refetch()}>Retry</Action>}
    {query.isPending ? <p role="status" className="p-8 text-body/60">Loading records…</p> : !query.isError && <>
    <div className="overflow-x-auto rounded-xl border border-border"><table className="w-full text-left text-sm"><thead className="bg-background"><tr>{columns.map(c => <th scope="col" className="whitespace-nowrap px-4 py-3 font-semibold text-body/70" key={c}>{labelOf(c)}</th>)}{actions && <th scope="col" className="px-4 py-3">Actions</th>}</tr></thead><tbody className="divide-y divide-border">{rows.map((row, i) => <tr key={row.id || row.ipAddress || i} className="align-top hover:bg-background/50">{columns.map(c => <td key={c} className="max-w-xs break-words px-4 py-4">{display(row[c], c)}</td>)}{actions && <td className="px-4 py-3"><div className="flex min-w-32 flex-wrap gap-2">{actions(row)}</div></td>}</tr>)}</tbody></table>{rows.length === 0 && <p className="p-10 text-center text-body/60">No records match this view.</p>}</div>
    {onPage && !Array.isArray(query.data) && <div className="mt-4 flex flex-wrap items-center justify-between gap-3 text-sm"><span>{query.data?.totalCount ?? 0} records · Page {page} of {Math.max(pages, 1)}</span><div className="flex gap-2"><Action disabled={page <= 1 || query.isFetching} onClick={() => onPage(page - 1)}>Previous</Action><Action disabled={page >= pages || query.isFetching} onClick={() => onPage(page + 1)}>Next</Action></div></div>}</>}
  </>;
}
export function Modal({
  title,
  onClose,
  children
}) {
  const ref = useRef(null);
  useEffect(() => {
    const el = ref.current;
    el.showModal();
    return () => el.close();
  }, []);
  return <dialog aria-label={title} ref={ref} onCancel={e => {
    e.preventDefault();
    onClose();
  }} className="m-auto max-h-[90vh] w-[min(760px,94vw)] rounded-2xl border border-border bg-surface p-0 text-heading shadow-2xl backdrop:bg-slate-950/50"><div className="sticky top-0 z-10 flex items-center justify-between gap-4 border-b border-border bg-surface p-5"><h2 className="text-xl font-semibold">{title}</h2><Action aria-label="Close dialog" onClick={onClose}>✕</Action></div><div className="p-5">{children}</div></dialog>;
}
function Field({
  field,
  value,
  onChange
}) {
  const upload = useMutation({ mutationFn: productsService.uploadImage, onSuccess: result => onChange(result.url) });
  const optionsQuery = useQuery({
    queryKey: ['super-admin', 'lookup', field.lookup],
    enabled: Boolean(field.lookup),
    queryFn: async () => {
      const options = [];
      let page = 1;
      let result;
      do {
        result = await api.list(field.lookup, {
          page,
          pageSize: 100,
          ...(field.lookup === '/districts' ? {
            includeInactive: true
          } : {})
        });
        options.push(...rowsOf(result));
        page += 1;
      } while (!Array.isArray(result) && page <= result.totalPages);
      return options;
    }
  });
  const options = field.lookup ? rowsOf(optionsQuery.data).map(x => ({
    value: x.id,
    label: x.name || x.title
  })) : field.options?.map(x => typeof x === 'string' ? {
    value: x,
    label: x
  } : x);
  const common = {
    id: `admin-${field.key}`,
    name: field.key,
    required: field.required,
    disabled: field.lookup && !optionsQuery.isSuccess,
    className: inputClass,
    value: value ?? '',
    onChange: e => onChange(e.target.value),
    maxLength: field.maxLength,
    min: field.min,
    max: field.max,
    step: field.type === 'number' ? 'any' : undefined
  };
  if (field.type === 'checkbox') return <label className="flex items-center gap-3 py-2 text-sm font-medium"><input type="checkbox" checked={Boolean(value)} onChange={e => onChange(e.target.checked)} className="h-4 w-4 accent-primary" />{field.label}</label>;
  if (field.type === 'image-upload') return <div><label htmlFor={common.id} className="mb-1.5 block text-sm font-medium">{field.label}</label><input id={common.id} type="file" accept="image/jpeg,image/png,image/webp" className={inputClass} disabled={upload.isPending} onChange={event => { const file = event.target.files?.[0]; if (file) upload.mutate(file); }} /><p className="mt-1 text-xs text-body/60">JPG, PNG or WebP, up to 5 MB. {upload.isPending ? 'Uploading…' : value ? 'Image ready.' : ''}</p><ErrorNotice error={upload.error} />{value && <SafeImage src={resolveMediaUrl(value)} alt="Uploaded preview" className="mt-2 h-32 w-full rounded-lg object-cover" />}</div>;
  return <div className={field.type === 'textarea' ? 'sm:col-span-2' : ''}><label htmlFor={common.id} className="mb-1.5 block text-sm font-medium">{field.label}{field.required ? ' *' : ''}</label>{options ? <select aria-label={field.label} {...common}><option value="">{optionsQuery.isPending && field.lookup ? 'Loading…' : 'Select…'}</option>{options.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}</select> : field.type === 'textarea' ? <textarea aria-label={field.label} {...common} rows={field.key === 'content' ? 10 : 3} /> : <input aria-label={field.label} {...common} type={field.type || 'text'} />}{field.lookup && <ErrorNotice error={optionsQuery.error} />}</div>;
}
export function Editor({
  fields,
  initial = {},
  onSubmit,
  pending,
  error,
  submitLabel = 'Save changes'
}) {
  const [values, setValues] = useState(() => Object.fromEntries(fields.map(f => {
    let value = initial[f.key] ?? f.default ?? (f.type === 'checkbox' ? false : '');
    if (f.type === 'datetime-local' && value) {
      const d = new Date(value);
      value = new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
    }
    return [f.key, value];
  })));
  const [validation, setValidation] = useState('');
  return <form onSubmit={e => {
    e.preventDefault();
    if (pending) return;
    const body = formPayload(fields, values);
    const missing = fields.find(field => field.required && field.type !== 'checkbox' && (body[field.key] == null || body[field.key] === ''));
    if (missing) { setValidation(`Complete ${missing.label.toLowerCase()} before saving.`); return; }
    if (body.startDate && body.endDate && body.endDate < body.startDate || body.startsAt && body.endsAt && body.endsAt < body.startsAt || body.from && body.to && body.to < body.from) {
      setValidation('The end date must be after the start date.');
      return;
    }
    setValidation('');
    onSubmit(body);
  }}><fieldset disabled={pending} className="grid gap-4 sm:grid-cols-2">{fields.map(f => <Field key={f.key} field={f} value={values[f.key]} onChange={value => setValues(old => ({
        ...old,
        [f.key]: value
      }))} />)}</fieldset>{validation && <p role="alert" className="mt-3 text-red-700">{validation}</p>}<ErrorNotice error={error} /><button disabled={pending} type="submit" className="mt-5 rounded-lg bg-primary px-5 py-2.5 text-sm font-semibold text-white disabled:opacity-50">{pending ? 'Saving…' : submitLabel}</button></form>;
}
export function RecordDetails({
  record
}) {
  return <dl className="grid gap-4 sm:grid-cols-2">{Object.entries(record || {}).filter(([k, v]) => v != null && !['evidenceJson', 'apiKey'].includes(k)).map(([k, v]) => <div key={k} className="min-w-0"><dt className="text-xs font-semibold uppercase tracking-wide text-body/60">{labelOf(k)}</dt><dd className="mt-1 whitespace-pre-wrap break-words text-sm">{typeof v === 'object' && !Array.isArray(v) ? display(v) : Array.isArray(v) && v.some(x => typeof x === 'object') ? <div className="space-y-3">{v.map((x, i) => <RecordDetails key={i} record={x} />)}</div> : display(v, k)}{typeof v === 'string' && /ImageUrl$/.test(k) && /^https?:\/\//.test(v) && <a href={v} target="_blank" rel="noreferrer" className="ml-2 text-primary underline">Open document</a>}</dd></div>)}</dl>;
}
