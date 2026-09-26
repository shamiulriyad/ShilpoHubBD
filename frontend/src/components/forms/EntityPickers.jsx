import { useSupplierSearch } from '../../hooks/useSupplierDiscovery';
import { useMyProducts, useProducts } from '../../hooks/useProducts';

// Pickers for records people used to have to paste in as raw GUIDs ("Producer ID", "Product ID").
// Each one loads its own options, shows a readable name, and hands the id back through onChange.

const fieldClass = 'w-full rounded-md border border-border bg-background px-3 py-2 text-sm';

function Field({ id, label, hint, children }) {
  return (
    <label htmlFor={id} className="block text-sm">
      <span className="mb-1 block font-medium text-heading">{label}</span>
      {children}
      {hint && <span className="mt-1 block text-xs text-body/60">{hint}</span>}
    </label>
  );
}

function statusOption(query, emptyText) {
  if (query.isLoading) return <option value="" disabled>Loading…</option>;
  if (query.isError) return <option value="" disabled>Could not load — try again</option>;
  return emptyText ? <option value="" disabled>{emptyText}</option> : null;
}

export function useProducerOptions() {
  const query = useSupplierSearch({ pageSize: 50 });
  const options = (query.data?.items || []).map((p) => ({
    id: p.producerId,
    label: [p.producerName, p.workshopName, p.districtName].filter(Boolean).join(' · '),
  }));
  return { query, options };
}

export function ProducerSelect({ id = 'producer-select', label = 'Producer', value, onChange, required = false, hint, placeholder = 'Choose a producer…' }) {
  const { query, options } = useProducerOptions();
  return (
    <Field id={id} label={label} hint={hint}>
      <select id={id} required={required} value={value} onChange={(e) => onChange(e.target.value)} className={fieldClass}>
        <option value="">{placeholder}</option>
        {options.map((o) => <option key={o.id} value={o.id}>{o.label}</option>)}
        {statusOption(query, options.length === 0 && !query.isLoading ? 'No producers found' : null)}
      </select>
    </Field>
  );
}

// Several producers at once (Compare Producers). Renders checkboxes, keeps `value` an array of ids.
export function ProducerMultiSelect({ value, onChange, max = 6 }) {
  const { query, options } = useProducerOptions();
  const toggle = (id) => {
    if (value.includes(id)) onChange(value.filter((v) => v !== id));
    else if (value.length < max) onChange([...value, id]);
  };
  if (query.isLoading) return <p className="text-sm text-body/60">Loading producers…</p>;
  if (query.isError) return <p role="alert" className="text-sm text-error">Could not load producers. Please try again.</p>;
  if (options.length === 0) return <p className="text-sm text-body/60">No producers found.</p>;
  return (
    <fieldset>
      <legend className="mb-2 text-sm font-medium text-heading">Choose 2 to {max} producers ({value.length} selected)</legend>
      <div className="grid gap-2 sm:grid-cols-2">
        {options.map((o) => {
          const checked = value.includes(o.id);
          const blocked = !checked && value.length >= max;
          return (
            <label key={o.id} className={`flex items-center gap-2 rounded-lg border px-3 py-2 text-sm ${checked ? 'border-primary bg-primary/5' : 'border-border bg-surface'} ${blocked ? 'opacity-50' : ''}`}>
              <input type="checkbox" checked={checked} disabled={blocked} onChange={() => toggle(o.id)} />
              <span>{o.label}</span>
            </label>
          );
        })}
      </div>
    </fieldset>
  );
}

// A producer's public products (for a business partner choosing what to procure).
export function ProducerProductSelect({ id = 'product-select', label = 'Product', producerId, value, onChange, required = false }) {
  const query = useProducts({ producerId: producerId || undefined, pageSize: 50 });
  const products = producerId ? query.data?.items || [] : [];
  return (
    <Field id={id} label={label} hint={!producerId ? 'Choose a producer first.' : undefined}>
      <select id={id} required={required} disabled={!producerId} value={value} onChange={(e) => onChange(e.target.value)} className={fieldClass}>
        <option value="">{producerId ? 'Choose a product…' : '—'}</option>
        {products.map((p) => <option key={p.id} value={p.id}>{p.name}{p.price != null ? ` — ৳ ${Number(p.price).toLocaleString('en-BD')}` : ''}</option>)}
        {producerId && statusOption(query, products.length === 0 && !query.isLoading ? 'This producer has no listed products' : null)}
      </select>
    </Field>
  );
}

// The signed-in producer's own products (AI Business Assistant tools).
export function MyProductSelect({ id = 'my-product-select', label = 'Product', value, onChange, required = false, placeholder = 'Choose one of your products…' }) {
  const query = useMyProducts();
  const products = Array.isArray(query.data) ? query.data : query.data?.items || [];
  return (
    <Field id={id} label={label}>
      <select id={id} required={required} value={value} onChange={(e) => onChange(e.target.value)} className={fieldClass}>
        <option value="">{placeholder}</option>
        {products.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
        {statusOption(query, products.length === 0 && !query.isLoading ? 'You have no products yet' : null)}
      </select>
    </Field>
  );
}
