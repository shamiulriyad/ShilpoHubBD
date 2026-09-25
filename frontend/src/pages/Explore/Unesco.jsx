import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useUnescoRecords } from '../../hooks/useUnescoRecords';

const TYPE_LABEL = {
  CulturalHeritageSite: 'Cultural site',
  NaturalHeritageSite: 'Natural site',
  IntangibleCulturalHeritage: 'Intangible heritage',
  MemoryOfTheWorld: 'Memory of the World',
};

// Records are managed by the Super Admin (Admin › Heritage Management › UNESCO Heritage).
export default function Unesco() {
  const { data, isLoading, isError, error } = useUnescoRecords();
  const records = Array.isArray(data) ? data : data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'UNESCO Heritage' },
        ]}
        title="UNESCO Heritage"
        description="Sites and traditions of Bangladesh recognized by UNESCO."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        {records.length === 0 ? (
          <p className="rounded-xl border border-border bg-surface p-8 text-center text-sm text-body/60">No UNESCO heritage records have been published yet.</p>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {records.map((record) => (
              <div key={record.id} className="overflow-hidden rounded-xl border border-border bg-surface">
                {record.imageUrl ? (
                  <img src={record.imageUrl} alt={record.title} loading="lazy" className="aspect-video w-full object-cover" />
                ) : (
                  <div className="flex aspect-video items-center justify-center bg-background text-xs text-body/40">Heritage Image</div>
                )}
                <div className="space-y-2 p-4">
                  <div className="flex flex-wrap gap-2">
                    <Badge tone="success">Inscribed {record.inscribedYear}</Badge>
                    <Badge tone="neutral">{TYPE_LABEL[record.type] || record.type}</Badge>
                  </div>
                  <p className="text-sm font-semibold text-heading">{record.title}</p>
                  <p className="text-xs text-body/60">{record.description}</p>
                  {record.districtName && <p className="text-xs text-body/50">{record.districtName}</p>}
                  {record.officialUrl && <a href={record.officialUrl} target="_blank" rel="noreferrer" className="text-xs font-semibold text-primary underline">Official UNESCO page</a>}
                </div>
              </div>
            ))}
          </div>
        )}
      </AsyncState>
    </div>
  );
}
