import { routePaths } from '../../routes/routePaths';
import { PageHeader, SearchBar } from '../../components/ui';
import { EntityCard } from '../../components/cards';

const sections = [
  { title: 'Districts', description: 'Heritage organized by district', to: routePaths.exploreDistricts },
  { title: 'Heritage Villages', description: 'Villages known for traditional craft', to: routePaths.exploreVillages },
  { title: 'Crafts', description: 'Traditional craft disciplines', to: routePaths.exploreCrafts },
  { title: 'Producers', description: 'Artisans, farmers & makers', to: routePaths.exploreProducers },
  { title: 'UNESCO Heritage', description: 'Nationally & internationally recognized heritage', to: routePaths.exploreUnesco },
  { title: 'Digital Museum', description: 'Curated heritage collections', to: routePaths.exploreMuseum },
];

export default function ExploreHome() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'Explore' }]}
        title="Explore Heritage"
        description="Discover the districts, villages, crafts and people behind Bangladesh's living heritage."
      />
      <div className="mb-10 max-w-xl">
        <SearchBar placeholder="Search heritage…" />
      </div>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {sections.map((section) => (
          <EntityCard key={section.title} title={section.title} subtitle={section.description} to={section.to} />
        ))}
      </div>
    </div>
  );
}
