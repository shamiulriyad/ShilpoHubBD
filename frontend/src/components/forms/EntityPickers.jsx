import { useSupplierSearch } from '../../hooks/useSupplierDiscovery';
import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { supplierDiscoveryService } from '../../services/supplierDiscoveryService';
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

// Search the directory in bounded pages; retain the shortlist across searches.
export function ProducerMultiSelect({ value, onChange, max = 6 }) {
  const [search, setSearch] = useState('');
  const [term, setTerm] = useState('');
  const [page, setPage] = useState(1);
  const [selectedNames, setSelectedNames] = useState({});
  useEffect(() => {
    const timer = setTimeout(() => { setTerm(search.trim()); setPage(1); }, 300);
    return () => clearTimeout(timer);
  }, [search]);
  const query = useQuery({
    queryKey: ['producer-picker', term, page],
    queryFn: () => supplierDiscoveryService.search({ search: term || undefined, page, pageSize: 8 }),
  });
  const options = query.data?.items || [];
  const total = query.data?.totalCount || 0;
  const totalPages = Math.max(1, Math.ceil(total / 8));
  const add = (producer) => {
    if (value.includes(producer.producerId) || value.length >= max) return;
    setSelectedNames((names) => ({ ...names, [producer.producerId]: producer.producerName }));
    onChange([...value, producer.producerId]);
  };
  return (
    <fieldset className="producer-picker">
      <legend className="sr-only">Choose 2 to {max} producers</legend>
      <div className="producer-shortlist">
        <div className="supplier-filter-heading"><h2>Comparison shortlist</h2><span aria-live="polite">{value.length} / {max} selected</span></div>
        <p className="text-sm text-body/70">Find producers below and add 2 to {max} to compare.</p>
        <div className="producer-shortlist-chips">
          {value.map((id) => <button key={id} type="button" onClick={() => onChange(value.filter((item) => item !== id))} aria-label={`Remove ${selectedNames[id] || 'producer'}`}><span>{selectedNames[id] || 'Selected producer'}</span><span aria-hidden="true">×</span></button>)}
          {value.length === 0 && <span className="text-sm text-body/60">Your shortlist is empty.</span>}
        </div>
        {value.length >= max && <p role="status" className="text-xs text-primary">Shortlist full. Remove a producer to add another.</p>}
      </div>
      <div className="producer-directory">
        <label htmlFor="comparison-producer-search" className="mb-2 block text-sm font-semibold text-heading">Search the producer directory</label>
        <input id="comparison-producer-search" type="search" placeholder="Search by producer or workshop name" value={search} onChange={(event) => setSearch(event.target.value)} className={fieldClass} />
        <div className="producer-directory-results" aria-busy={query.isFetching}>
          {query.isLoading ? <p role="status">Finding producers…</p> : query.isError ? <div role="alert">Could not load producers. <button type="button" onClick={() => query.refetch()} className="text-primary underline">Try again</button></div> : options.length === 0 ? <p>No producers found. Try another name.</p> : options.map((producer) => {
            const selected = value.includes(producer.producerId);
            return <div key={producer.producerId} className="producer-directory-row">
              <span className="supplier-avatar" aria-hidden="true">{(producer.producerName || 'P').slice(0, 1).toUpperCase()}</span>
              <div className="min-w-0 flex-1"><p className="font-semibold text-heading">{producer.producerName}</p><p className="text-xs text-body/70">{[producer.workshopName, producer.primaryCraft, producer.districtName].filter(Boolean).join(' · ') || `${producer.productCount ?? 0} listed products`}</p></div>
              <button type="button" disabled={selected || value.length >= max || query.isFetching} onClick={() => add(producer)}>{selected ? 'Added' : '+ Add'}</button>
            </div>;
          })}
        </div>
        <div className="producer-directory-pagination">
          <span>{query.isLoading ? 'Loading…' : `Page ${page} of ${totalPages} · ${total.toLocaleString()} producers`}</span>
          <div><button type="button" disabled={page <= 1 || query.isFetching} onClick={() => setPage(page - 1)}>Previous</button><button type="button" disabled={page >= totalPages || query.isFetching} onClick={() => setPage(page + 1)}>Next</button></div>
        </div>
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
