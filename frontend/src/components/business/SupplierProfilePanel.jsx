import { Badge } from '../ui';

const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;

// Full producer detail for a business partner: workshop, craft, rating, certifications and the products
// they can supply (with prices), so a partner can decide without opening another page.
export default function SupplierProfilePanel({ profile, isLoading }) {
  if (isLoading) return <p className="text-sm text-body/60">Loading producer details…</p>;
  if (!profile) return <p className="text-sm text-body/60">Select a producer to view their full profile and products.</p>;

  return (
    <div className="space-y-3">
      <div>
        <p className="text-sm font-semibold text-heading">{profile.producerName}</p>
        <p className="text-xs text-body/60">{[profile.workshopName, profile.primaryCraft, profile.districtName].filter(Boolean).join(' · ') || 'No workshop details yet'}</p>
      </div>
      {profile.workshopDescription && <p className="text-sm text-body/70">{profile.workshopDescription}</p>}
      <p className="text-xs text-body/60">
        ★ {Number(profile.averageRating ?? 0).toFixed(1)} ({profile.totalReviewCount ?? 0} reviews) · {profile.productCount} products
        {profile.estimatedProductionCapacity ? ` · ${profile.estimatedProductionCapacity} units/mo capacity` : ''}
      </p>
      {profile.yearsOfExperience != null && <p className="text-xs text-body/60">{profile.yearsOfExperience} years of experience</p>}
      {profile.certifications?.length > 0 && (
        <div className="flex flex-wrap gap-1">
          {profile.certifications.map((c, i) => <Badge key={i} tone="secondary">{c.name}</Badge>)}
        </div>
      )}
      <div>
        <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Products</p>
        <ul className="space-y-2">
          {(profile.products || []).map((p) => (
            <li key={p.id} className="flex items-center justify-between gap-2 rounded-lg border border-border px-3 py-2 text-sm">
              <span>
                <span className="block font-medium text-heading">{p.name}</span>
                <span className="block text-xs text-body/60">{p.categoryName} · ★ {Number(p.averageRating ?? 0).toFixed(1)} ({p.reviewCount})</span>
              </span>
              <span className="shrink-0 text-right">
                <span className="block font-semibold text-primary">{money(p.discountPrice ?? p.price)}</span>
                {p.discountPrice != null && <span className="block text-xs text-body/50 line-through">{money(p.price)}</span>}
              </span>
            </li>
          ))}
          {(profile.products || []).length === 0 && <li className="text-xs text-body/60">This producer has no listed products yet.</li>}
        </ul>
      </div>
    </div>
  );
}
