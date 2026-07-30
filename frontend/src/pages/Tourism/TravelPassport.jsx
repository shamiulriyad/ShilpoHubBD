import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { districts } from '../../data/mockData';

export default function TravelPassport() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Travel Passport' },
        ]}
        title="Travel Passport"
        description="Track the heritage sites and villages you've visited."
      />
      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <StatCard label="Sites Visited" value="6" />
        <StatCard label="Districts Explored" value="4" />
        <StatCard label="Badges Earned" value="3" />
      </div>
      <p className="mb-3 text-sm font-semibold text-heading">Visit Stamps</p>
      <div className="grid grid-cols-3 gap-3 sm:grid-cols-4 lg:grid-cols-8">
        {districts.map((district, i) => (
          <div
            key={district.id}
            className={`flex aspect-square flex-col items-center justify-center rounded-full border text-center text-[11px] ${
              i < 4 ? 'border-primary bg-primary/10 text-primary' : 'border-dashed border-border text-body/30'
            }`}
          >
            {district.name}
          </div>
        ))}
      </div>
    </div>
  );
}
