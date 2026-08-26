import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { VillageCard, StatCard } from '../../components/cards';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { toVillageCardItem } from '../../utils/villageAdapters';

export default function DistrictDetails() {
  const { districtId } = useParams();
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();

  const district = (districtsQuery.data || []).find((d) => d.id === districtId);
  const districtVillages = (villagesQuery.data || []).filter((v) => v.districtId === districtId);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={districtsQuery.isLoading} isError={districtsQuery.isError} error={districtsQuery.error}>
        {district && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Explore', path: routePaths.explore },
                { label: 'Districts', path: routePaths.exploreDistricts },
                { label: district.name },
              ]}
              title={district.name}
              description={`${district.division} Division`}
            />

            <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
              <StatCard label="Heritage Villages" value={districtVillages.length} />
              <StatCard label="Division" value={district.division} />
            </div>

            <p className="mb-3 text-sm font-semibold text-heading">Villages in {district.name}</p>
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {districtVillages.map((village) => (
                <VillageCard
                  key={village.id}
                  village={toVillageCardItem(village)}
                  to={routePaths.exploreVillageDetails.replace(':villageId', village.id)}
                />
              ))}
              {districtVillages.length === 0 && (
                <p className="col-span-full text-sm text-body/60">No villages recorded for this district yet.</p>
              )}
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
