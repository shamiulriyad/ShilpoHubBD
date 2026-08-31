import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, AsyncState } from '../../components/ui';
import { FestivalCard } from '../../components/cards';
import { useHeritageFestivals } from '../../hooks/useHeritageFestivals';
import { useDistricts } from '../../hooks/useDistricts';

export default function FestivalDirectory() {
  const festivalsQuery = useHeritageFestivals({ pageSize: 50 });
  const districtsQuery = useDistricts();

  const filterGroups = [{ label: 'District', options: (districtsQuery.data || []).slice(0, 8).map((d) => d.name) }];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Festivals' },
        ]}
        title="Festival Directory"
        description="Seasonal and regional cultural festivals."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <AsyncState isLoading={festivalsQuery.isLoading} isError={festivalsQuery.isError} error={festivalsQuery.error}>
          <div className="grid gap-4 sm:grid-cols-2">
            {festivalsQuery.data?.items.map((festival) => (
              <FestivalCard
                key={festival.id}
                festival={{ name: festival.name, date: festival.startDate, district: festival.districtName }}
              />
            ))}
            {festivalsQuery.data?.items.length === 0 && (
              <p className="col-span-full text-sm text-body/60">No festivals scheduled right now.</p>
            )}
          </div>
        </AsyncState>
      </div>
    </div>
  );
}
