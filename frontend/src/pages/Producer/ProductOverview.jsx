import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import SafeImage from '../../components/media/SafeImage';
import { resolveMediaUrl } from '../../components/media/CardMedia';
import { routePaths } from '../../routes/routePaths';
import { useProduct } from '../../hooks/useProducts';
import { useProductAttributes } from '../../hooks/useProductAttributes';
import { useProductReviews } from '../../hooks/useReviews';

const STATUS = { Approved: ['success', 'Live on the marketplace'], Pending: ['secondary', 'Waiting for admin review'], Rejected: ['neutral', 'Needs changes'] };
const money = (v) => `৳ ${Number(v).toLocaleString('en-BD')}`;

function Section({ title, action, children }) {
  return (
    <section className="rounded-xl border border-border bg-surface p-5">
      <div className="mb-3 flex items-center justify-between gap-3"><h2 className="text-base font-semibold text-heading">{title}</h2>{action}</div>
      {children}
    </section>
  );
}
const Chips = ({ items }) => items?.length ? <div className="flex flex-wrap gap-2">{items.map((t) => <span key={t} className="rounded-full bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary">{t}</span>)}</div> : <span className="text-sm text-body/50">Not set</span>;

// The producer's own view of one product: everything a shopper sees, plus review status, stock and how it can be found.
export default function ProductOverview() {
  const { productId } = useParams();
  const query = useProduct(productId);
  const attributes = useProductAttributes(productId);
  const reviews = useProductReviews(productId, { pageSize: 5 });
  const [active, setActive] = useState(0);
  const p = query.data;
  const a = attributes.data;
  const images = p?.imageUrls || [];
  const low = p && p.stock > 0 && p.lowStockThreshold != null && p.stock <= p.lowStockThreshold;
  const [tone, statusText] = STATUS[p?.approvalStatus] || ['neutral', p?.approvalStatus];

  return (
    <div>
      <PageHeader
        title={p?.name || 'Product details'}
        description="Everything about this listing in one place."
        breadcrumbs={[{ label: 'My products', path: routePaths.producerProducts }, { label: p?.name || 'Details' }]}
        action={p && (
          <div className="flex flex-wrap gap-2">
            <Link to={`${routePaths.producerProducts}?edit=${p.id}`} className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-surface hover:bg-primary-dark">Edit product</Link>
            {p.approvalStatus === 'Approved' && <Link to={routePaths.marketplaceProductDetails.replace(':productId', p.id)} className="rounded-lg border border-border px-4 py-2 text-sm font-medium text-heading hover:border-primary/40">View public page</Link>}
          </div>
        )}
      />
      <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>
        {p && (
          <div className="space-y-6">
            <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.1fr)]">
              <div>
                <div className="aspect-[4/3] overflow-hidden rounded-xl border border-border bg-background">
                  {images.length ? <SafeImage src={resolveMediaUrl(images[active])} alt={p.name} className="h-full w-full object-cover" /> : <div className="grid h-full place-items-center text-sm text-body/50">No photo yet</div>}
                </div>
                {images.length > 1 && (
                  <div className="mt-3 flex gap-2">
                    {images.map((url, i) => (
                      <button key={url} type="button" onClick={() => setActive(i)} aria-label={`Show photo ${i + 1}`} aria-pressed={i === active}
                        className={`h-16 w-20 overflow-hidden rounded-lg border-2 ${i === active ? 'border-primary' : 'border-transparent'}`}>
                        <SafeImage src={resolveMediaUrl(url)} alt="" className="h-full w-full object-cover" />
                      </button>
                    ))}
                  </div>
                )}
              </div>

              <div className="space-y-4">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge tone={tone}>{statusText}</Badge>
                  <Badge tone={p.handmadeVerificationStatus === 'Verified' ? 'success' : 'neutral'}>Handmade: {p.handmadeVerificationStatus}</Badge>
                  {p.isFeatured && <Badge tone="secondary">Featured</Badge>}
                  {!p.isActive && <Badge tone="neutral">Hidden</Badge>}
                </div>
                <p className="text-sm text-body/70">{p.categoryName} · {p.districtName}</p>
                <p className="text-3xl font-semibold text-heading">
                  {money(p.discountPrice ?? p.price)}
                  {p.discountPrice != null && <span className="ml-3 text-lg font-normal text-body/50 line-through">{money(p.price)}</span>}
                </p>
                {p.rejectionReason && <p role="alert" className="rounded-lg bg-red-50 p-3 text-sm text-red-800">Review note from the admin: {p.rejectionReason}</p>}
                {p.approvalStatus === 'Pending' && <p className="rounded-lg bg-secondary/10 p-3 text-sm text-heading">This listing (or your latest edit) is waiting for an admin. It is not visible to shoppers yet.</p>}
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                  <StatCard label="In stock" value={p.stock} trend={p.stock === 0 ? 'Out of stock' : low ? 'Running low' : undefined} />
                  <StatCard label="Sold" value={p.salesCount} />
                  <StatCard label="Views" value={p.viewCount} />
                  <StatCard label="Rating" value={p.reviewCount ? `${Number(p.averageRating).toFixed(1)} ★` : '—'} trend={`${p.reviewCount} review${p.reviewCount === 1 ? '' : 's'}`} />
                </div>
              </div>
            </div>

            <Section title="Description">
              <p className="whitespace-pre-line text-sm leading-7 text-body/80">{p.description}</p>
              {p.story && <><h3 className="mt-4 text-sm font-semibold text-heading">Story</h3><p className="mt-1 whitespace-pre-line text-sm leading-7 text-body/80">{p.story}</p></>}
            </Section>

            {p.variants?.length > 0 && (
              <Section title="Options">
                <div className="overflow-x-auto"><table className="w-full text-left text-sm">
                  <thead className="text-xs uppercase tracking-wide text-body/50"><tr><th className="py-2 pr-4">Option</th><th className="py-2 pr-4">SKU</th><th className="py-2 pr-4">Price</th><th className="py-2">Stock</th></tr></thead>
                  <tbody className="divide-y divide-border">{p.variants.map((v) => <tr key={v.id}><td className="py-2 pr-4 font-medium text-heading">{v.name}</td><td className="py-2 pr-4">{v.sku || '—'}</td><td className="py-2 pr-4">{money(v.price ?? p.price)}</td><td className="py-2">{v.stock}</td></tr>)}</tbody>
                </table></div>
              </Section>
            )}

            <Section title="How shoppers and AI search find this" action={<Link to={routePaths.producerProductAttributes.replace(':productId', p.id)} className="text-sm font-medium text-primary hover:underline">{a?.exists ? 'Edit AI search info' : 'Add AI search info'}</Link>}>
              {a?.exists ? (
                <dl className="grid gap-4 text-sm sm:grid-cols-2">
                  <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Product type</dt><dd>{a.productTypeName || <span className="text-body/50">Not set</span>}</dd></div>
                  <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Materials</dt><dd><Chips items={a.materialNames} /></dd></div>
                  <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Tags</dt><dd><Chips items={a.tags} /></dd></div>
                  <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Bangla / alternate words</dt><dd><Chips items={a.keywords} /></dd></div>
                  <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Occasions</dt><dd><Chips items={a.occasions} /></dd></div>
                  <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Colours</dt><dd><Chips items={a.colors} /></dd></div>
                  {a.dimensionsText && <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Size</dt><dd>{a.dimensionsText}</dd></div>}
                  {a.madeToOrder && <div><dt className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Made to order</dt><dd>{a.leadTimeDays ? `About ${a.leadTimeDays} days` : 'Yes'}</dd></div>}
                </dl>
              ) : <p className="text-sm text-body/70">Not filled in yet. Adding a product type, materials and keywords helps this product appear when shoppers search (including in Bangla).</p>}
            </Section>

            <Section title={`Recent reviews (${p.reviewCount})`}>
              {(reviews.data?.items || []).length ? (
                <ul className="divide-y divide-border">{reviews.data.items.map((r) => (
                  <li key={r.id} className="py-3 text-sm"><p className="font-medium text-heading">{'★'.repeat(r.rating)}<span className="text-body/30">{'★'.repeat(5 - r.rating)}</span> <span className="ml-2 font-normal text-body/60">{r.reviewerName}</span></p><p className="mt-1 text-body/80">{r.comment}</p></li>
                ))}</ul>
              ) : <p className="text-sm text-body/70">No reviews yet. Shoppers can review after they receive an order.</p>}
            </Section>
          </div>
        )}
      </AsyncState>
    </div>
  );
}
