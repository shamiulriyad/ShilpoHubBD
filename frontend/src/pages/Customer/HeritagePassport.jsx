import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { heritagePassportStamps } from '../../data/mockData';

export default function HeritagePassport() {
  const collected = heritagePassportStamps.filter((s) => s.collected).length;

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Heritage Passport' }]}
        title="Heritage Passport"
        description={`${collected} of ${heritagePassportStamps.length} stamps collected. Visit villages and festivals to fill your passport.`}
      />

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {heritagePassportStamps.map((stamp) => (
          <div
            key={stamp.id}
            className={`flex flex-col items-center gap-2 rounded-xl border p-5 text-center ${
              stamp.collected ? 'border-primary/30 bg-primary/5' : 'border-dashed border-border bg-surface opacity-60'
            }`}
          >
            <span
              className={`flex h-14 w-14 items-center justify-center rounded-full border-2 text-lg font-semibold ${
                stamp.collected ? 'border-primary text-primary' : 'border-border text-body/30'
              }`}
            >
              {stamp.collected ? '✓' : '?'}
            </span>
            <p className="text-sm font-semibold text-heading">{stamp.name}</p>
            <p className="text-xs text-body/60">
              {stamp.type} · {stamp.district}
            </p>
            <Badge tone={stamp.collected ? 'success' : 'neutral'}>
              {stamp.collected ? `Collected ${stamp.date}` : 'Not yet collected'}
            </Badge>
          </div>
        ))}
      </div>
    </div>
  );
}
