import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { PageHeader, Button, Badge, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import ChipInput from '../../components/ui/ChipInput';
import { routePaths } from '../../routes/routePaths';
import { useMyProducts } from '../../hooks/useProducts';
import { useAttributeSuggestion, useProductAttributeMutations, useProductAttributes, useProductLookups } from '../../hooks/useProductAttributes';
import { getApiErrorMessage } from '../../utils/apiError';

const inputClass = 'w-full rounded-md border border-border bg-background px-3 py-2 text-sm';
const METHODS = ['Handmade', 'Handloom', 'Hand-finished'];
const OCCASION_HINTS = 'wedding, eid, pohela boishakh, gift, puja, festival, home decor';

const EMPTY = {
  productTypeId: '', materialIds: [], tags: [], keywords: [], occasions: [], colors: [], craftTechnique: '', productionMethod: '',
  dimensionsText: '', lengthCm: '', widthCm: '', heightCm: '', weightGrams: '', madeToOrder: false, leadTimeDays: '', careInstructions: '',
};

const asText = (value) => (value == null ? '' : String(value));
const asNumber = (value) => (value === '' || value == null || Number.isNaN(Number(value)) ? null : Number(value));

const fromDto = (d) => ({
  ...EMPTY,
  productTypeId: d.productTypeId || '', materialIds: d.materialIds || [], tags: d.tags || [], keywords: d.keywords || [],
  occasions: d.occasions || [], colors: d.colors || [], craftTechnique: asText(d.craftTechnique), productionMethod: asText(d.productionMethod),
  dimensionsText: asText(d.dimensionsText), lengthCm: asText(d.lengthCm), widthCm: asText(d.widthCm), heightCm: asText(d.heightCm),
  weightGrams: asText(d.weightGrams), madeToOrder: Boolean(d.madeToOrder), leadTimeDays: asText(d.leadTimeDays), careInstructions: asText(d.careInstructions),
});

const toPayload = (f) => ({
  productTypeId: f.productTypeId || null, materialIds: f.materialIds, tags: f.tags, keywords: f.keywords, occasions: f.occasions, colors: f.colors,
  craftTechnique: f.craftTechnique.trim() || null, productionMethod: f.productionMethod || null, dimensionsText: f.dimensionsText.trim() || null,
  lengthCm: asNumber(f.lengthCm), widthCm: asNumber(f.widthCm), heightCm: asNumber(f.heightCm), weightGrams: asNumber(f.weightGrams),
  madeToOrder: f.madeToOrder, leadTimeDays: f.madeToOrder ? asNumber(f.leadTimeDays) : null, careInstructions: f.careInstructions.trim() || null,
});

const union = (a, b) => [...a, ...b.filter((x) => !a.some((y) => y.toLowerCase() === x.toLowerCase()))];

function Field({ id, label, hint, children }) {
  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-sm font-medium text-heading">{label}</label>
      {children}
      {hint && <p className="mt-1 text-xs text-body/60">{hint}</p>}
    </div>
  );
}

// What the AI proposed, in plain words, so the producer can judge it before using any of it.
function SuggestionSummary({ suggested, types, materials }) {
  const typeName = types.find((t) => t.slug === suggested.productTypeSlug)?.name;
  const materialNames = (suggested.materialSlugs || []).map((slug) => materials.find((m) => m.slug === slug)?.name).filter(Boolean);
  const rows = [
    ['Product type', typeName], ['Materials', materialNames.join(', ')], ['Tags', (suggested.tags || []).join(', ')],
    ['Bangla / alternate words', (suggested.keywords || []).join(', ')], ['Occasions', (suggested.occasions || []).join(', ')],
    ['Colours', (suggested.colors || []).join(', ')], ['Technique', suggested.craftTechnique], ['Production method', suggested.productionMethod],
    ['Care', suggested.careInstructions],
  ].filter(([, value]) => value);
  if (!rows.length) return <p className="text-sm text-body/70">The AI could not find anything to suggest from the current description. Adding more detail to the product description helps.</p>;
  return (
    <dl className="grid gap-x-6 gap-y-2 text-sm sm:grid-cols-2">
      {rows.map(([label, value]) => <div key={label}><dt className="text-xs font-semibold uppercase tracking-wide text-body/50">{label}</dt><dd className="mt-0.5 text-heading">{value}</dd></div>)}
    </dl>
  );
}

export default function ProductAttributes() {
  const { productId } = useParams();
  const products = useMyProducts();
  const product = (products.data || []).find((p) => p.id === productId);
  const attributes = useProductAttributes(productId);
  const suggestionQuery = useAttributeSuggestion(productId);
  const { types, materials } = useProductLookups();
  const { save, generate, confirm, dismiss } = useProductAttributeMutations(productId);

  const [form, setForm] = useState(EMPTY);
  const [dirty, setDirty] = useState(false);
  const [appliedSuggestionId, setAppliedSuggestionId] = useState(null);

  // Load the saved values into the form until the producer starts editing.
  useEffect(() => { if (attributes.data && !dirty) setForm(fromDto(attributes.data)); }, [attributes.data, dirty]);

  const typeOptions = types.data || [];
  const materialOptions = materials.data || [];
  const suggestion = suggestionQuery.data;
  const change = (patch) => { setForm((old) => ({ ...old, ...patch })); setDirty(true); };
  const busy = save.isPending || confirm.isPending;

  const applySuggestion = () => {
    const s = suggestion.suggested || {};
    const typeId = typeOptions.find((t) => t.slug === s.productTypeSlug)?.id;
    const materialIds = (s.materialSlugs || []).map((slug) => materialOptions.find((m) => m.slug === slug)?.id).filter(Boolean);
    setForm((old) => ({
      ...old,
      productTypeId: typeId || old.productTypeId,
      materialIds: materialIds.length ? union(old.materialIds, materialIds) : old.materialIds,
      tags: union(old.tags, s.tags || []), keywords: union(old.keywords, s.keywords || []),
      occasions: union(old.occasions, s.occasions || []), colors: union(old.colors, s.colors || []),
      craftTechnique: s.craftTechnique || old.craftTechnique, productionMethod: s.productionMethod || old.productionMethod,
      careInstructions: s.careInstructions || old.careInstructions,
    }));
    setAppliedSuggestionId(suggestion.id);
    setDirty(true);
  };

  const onSaved = () => { setDirty(false); setAppliedSuggestionId(null); };
  const submit = (event) => {
    event.preventDefault();
    const payload = toPayload(form);
    // AI values only become final here, with the producer's reviewed form; confirming also closes the suggestion.
    if (appliedSuggestionId && suggestion?.id === appliedSuggestionId) confirm.mutate({ suggestionId: appliedSuggestionId, attributes: payload }, { onSuccess: onSaved });
    else save.mutate(payload, { onSuccess: onSaved });
  };

  const materialSet = useMemo(() => new Set(form.materialIds), [form.materialIds]);
  const toggleMaterial = (id) => change({ materialIds: materialSet.has(id) ? form.materialIds.filter((m) => m !== id) : [...form.materialIds, id] });

  return (
    <div>
      <PageHeader
        title="Search details"
        description="Help shoppers (and the AI search) find this product: what it is, what it is made of, and when it is used."
        breadcrumbs={[{ label: 'My products', path: routePaths.producerProducts }, { label: product?.name || 'Product' }]}
      />

      <AsyncState isLoading={attributes.isLoading} isError={attributes.isError} error={attributes.error}>
        {product && (
          <div className="mb-6 flex flex-wrap items-center gap-3 rounded-xl border border-border bg-surface p-4">
            <div className="min-w-0 flex-1">
              <p className="truncate font-semibold text-heading">{product.name}</p>
              <p className="text-sm text-body/70">{product.categoryName} · {product.districtName}</p>
            </div>
            <Badge tone={product.approvalStatus === 'Approved' ? 'success' : 'secondary'}>{product.approvalStatus}</Badge>
          </div>
        )}

        <section aria-label="AI suggestion" className="mb-6 rounded-xl border border-primary/25 bg-primary-soft/40 p-5">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h2 className="font-semibold text-heading">✦ Suggest with AI</h2>
              <p className="text-sm text-body/70">The AI reads your product name and description and proposes details. Nothing is saved until you review it and confirm.</p>
            </div>
            <Button type="button" variant="secondary" disabled={generate.isPending} onClick={() => generate.mutate()}>
              {generate.isPending ? 'Thinking…' : suggestion ? 'Suggest again' : 'Suggest details'}
            </Button>
          </div>
          {generate.isError && <p role="alert" className="mt-3 text-sm text-error">{getApiErrorMessage(generate.error, 'AI suggestions are unavailable right now.')}</p>}

          {suggestion && (
            <div className="mt-4 rounded-lg border border-border bg-surface p-4">
              <div className="mb-3 flex flex-wrap items-center gap-2">
                <Badge tone="secondary">Suggestion — not saved yet</Badge>
                <span className="text-xs text-body/60">Model: {suggestion.model}</span>
              </div>
              <SuggestionSummary suggested={suggestion.suggested || {}} types={typeOptions} materials={materialOptions} />
              {suggestion.suggested?.notes && <p className="mt-3 text-xs italic text-body/70">{suggestion.suggested.notes}</p>}
              <div className="mt-4 flex flex-wrap gap-2">
                <Button type="button" variant="primary" onClick={applySuggestion} disabled={appliedSuggestionId === suggestion.id}>
                  {appliedSuggestionId === suggestion.id ? 'Added to the form below' : 'Add to the form to review'}
                </Button>
                <Button type="button" variant="secondary" disabled={dismiss.isPending} onClick={() => dismiss.mutate(suggestion.id, { onSuccess: () => setAppliedSuggestionId(null) })}>Dismiss</Button>
              </div>
              {appliedSuggestionId === suggestion.id && <p className="mt-3 text-sm text-heading">Review and edit the form below, then press <strong>Confirm &amp; save</strong>. Remove anything that is not true for your product.</p>}
            </div>
          )}
        </section>

        <form onSubmit={submit} className="space-y-6" aria-label="Product search details">
          <div className="grid gap-4 rounded-xl border border-border bg-surface p-5 sm:grid-cols-2">
            <Field id="attr-type" label="Product type" hint="What kind of object it is — the main way shoppers filter.">
              <select id="attr-type" value={form.productTypeId} onChange={(e) => change({ productTypeId: e.target.value })} className={inputClass}>
                <option value="">Not set</option>
                {typeOptions.map((t) => <option key={t.id} value={t.id}>{t.name}{t.nameBn ? ` (${t.nameBn})` : ''}</option>)}
              </select>
            </Field>
            <Field id="attr-method" label="How it is made">
              <select id="attr-method" value={form.productionMethod} onChange={(e) => change({ productionMethod: e.target.value })} className={inputClass}>
                <option value="">Not set</option>
                {METHODS.map((m) => <option key={m} value={m}>{m}</option>)}
              </select>
            </Field>
            <fieldset className="sm:col-span-2">
              <legend className="mb-1.5 text-sm font-medium text-heading">Materials</legend>
              <div className="grid gap-x-4 gap-y-2 sm:grid-cols-3 lg:grid-cols-4">
                {materialOptions.map((m) => (
                  <label key={m.id} className="flex items-center gap-2 text-sm">
                    <input type="checkbox" checked={materialSet.has(m.id)} onChange={() => toggleMaterial(m.id)} />
                    <span>{m.name}{m.nameBn ? <span className="text-body/50"> · {m.nameBn}</span> : null}</span>
                  </label>
                ))}
              </div>
            </fieldset>
            <Field id="attr-technique" label="Technique" hint="For example: hand-stitched running stitch, jala loom weaving.">
              <input id="attr-technique" maxLength={200} value={form.craftTechnique} onChange={(e) => change({ craftTechnique: e.target.value })} className={inputClass} />
            </Field>
            <Field id="attr-care" label="Care instructions">
              <input id="attr-care" maxLength={500} value={form.careInstructions} onChange={(e) => change({ careInstructions: e.target.value })} className={inputClass} />
            </Field>
          </div>

          <div className="grid gap-4 rounded-xl border border-border bg-surface p-5 sm:grid-cols-2">
            <ChipInput id="attr-tags" label="Tags" values={form.tags} onChange={(tags) => change({ tags })} placeholder="jamdani, cotton, handwoven" hint="Short search words. Press Enter or comma after each." />
            <ChipInput id="attr-keywords" label="Bangla & alternate spellings" values={form.keywords} onChange={(keywords) => change({ keywords })} placeholder="জামদানি, shari, শাড়ি" hint="Words shoppers may type in Bangla or romanised Bangla." />
            <ChipInput id="attr-occasions" label="Occasions" values={form.occasions} onChange={(occasions) => change({ occasions })} placeholder="wedding, gift" hint={`For example: ${OCCASION_HINTS}.`} />
            <ChipInput id="attr-colors" label="Colours" values={form.colors} onChange={(colors) => change({ colors })} placeholder="indigo, red" />
          </div>

          <div className="grid gap-4 rounded-xl border border-border bg-surface p-5 sm:grid-cols-2 lg:grid-cols-4">
            <div className="sm:col-span-2 lg:col-span-4">
              <Field id="attr-size" label="Size (text)" hint="Only real measurements, e.g. 6.0 m × 1.15 m. The AI never fills this in.">
                <input id="attr-size" maxLength={120} value={form.dimensionsText} onChange={(e) => change({ dimensionsText: e.target.value })} className={inputClass} />
              </Field>
            </div>
            {[['lengthCm', 'Length (cm)'], ['widthCm', 'Width (cm)'], ['heightCm', 'Height (cm)'], ['weightGrams', 'Weight (g)']].map(([key, label]) => (
              <Field key={key} id={`attr-${key}`} label={label}>
                <input id={`attr-${key}`} type="number" min="0" step="any" value={form[key]} onChange={(e) => change({ [key]: e.target.value })} className={inputClass} />
              </Field>
            ))}
          </div>

          <div className="grid gap-4 rounded-xl border border-border bg-surface p-5 sm:grid-cols-2">
            <label className="flex items-center gap-2 text-sm font-medium text-heading">
              <input type="checkbox" checked={form.madeToOrder} onChange={(e) => change({ madeToOrder: e.target.checked })} />
              Made to order
            </label>
            <Field id="attr-lead" label="Lead time (days)" hint="How long a made-to-order piece takes.">
              <input id="attr-lead" type="number" min="0" max="365" disabled={!form.madeToOrder} value={form.leadTimeDays} onChange={(e) => change({ leadTimeDays: e.target.value })} className={inputClass} />
            </Field>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <Button type="submit" variant="primary" disabled={busy || !dirty}>
              {busy ? 'Saving…' : appliedSuggestionId ? 'Confirm & save' : 'Save details'}
            </Button>
            <Link to={routePaths.producerProducts} className="text-sm font-medium text-primary hover:underline">Back to my products</Link>
            {!dirty && !busy && (save.isSuccess || confirm.isSuccess) && <span role="status" className="text-sm text-success">Saved. Search will update shortly.</span>}
          </div>
          <MutationFeedback mutation={save} />
          <MutationFeedback mutation={confirm} />
        </form>
      </AsyncState>
    </div>
  );
}
