import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { VillageCard, EntityCard, StatCard } from '../../components/cards';
import { districts, villages, producers } from '../../data/mockData';

export default function DistrictDetails() {
  const { districtId } = useParams();
  const district = districts.find((d) => d.id === districtId) || districts[0];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Districts', path: routePaths.exploreDistricts },
          { label: district.name },
        ]}
        title={district.name}
        description="District heritage overview."
      />

      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <StatCard label="Heritage Villages" value={district.villages} />
        <StatCard label="Craft Disciplines" value={district.crafts} />
        <StatCard label="Registered Producers" value={producers.length * 4} />
      </div>

      <p className="mb-3 text-sm font-semibold text-heading">Villages in {district.name}</p>
      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {villages.map((village) => (
          <VillageCard key={village.id} village={village} to={routePaths.exploreVillageDetails.replace(':villageId', village.id)} />
        ))}
      </div>

      <p className="mb-3 text-sm font-semibold text-heading">Producers from {district.name}</p>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {producers.map((producer) => (
          <EntityCard key={producer.id} title={producer.name} subtitle={producer.craft} to={routePaths.exploreProducerDetails.replace(':producerId', producer.id)} />
        ))}
      </div>
    </div>
  );
}
