import { useMemo, useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SearchBar } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useCategories } from '../../hooks/useCategories';
import { useAuth } from '../../hooks/useAuth';

const sections = [
  { title: 'Districts', description: 'Heritage organized by district', to: routePaths.exploreDistricts, image: '/images/bangladesh-river.jpg' },
  { title: 'Heritage Villages', description: 'Villages and communities known for traditional craft', to: routePaths.tourismVillages, image: '/images/village-community.jpg' },
  { title: 'Crafts', description: 'Traditional craft disciplines', to: routePaths.exploreCrafts, image: '/images/loom-photo.jpg' },
  { title: 'Producers', description: 'Artisans, farmers & makers', to: routePaths.exploreProducers, image: '/images/heritage-weaver.png' },
  { title: 'UNESCO Heritage', description: 'Nationally & internationally recognized heritage', to: routePaths.exploreUnesco, image: '/images/heritage-landscape.png' },
  { title: 'Digital Museum', description: 'Curated heritage collections', to: routePaths.exploreMuseum, image: '/images/heritage-crafts.png' },
];

export default function ExploreHome() {
  const { isAuthenticated, activeRole, homePath } = useAuth();
  const inWorkspace = isAuthenticated && activeRole === 'LogisticsPartner';
  const [search, setSearch] = useState('');
  const categories = useCategories();
  const term = search.trim().toLocaleLowerCase();
  const results = useMemo(() => {
    if (!term) return [];
    return (categories.data || []).filter((category) => `${category.name} ${category.description || ''}`.toLocaleLowerCase().includes(term));
  }, [categories.data, term]);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: inWorkspace ? 'Dashboard' : 'Home', path: inWorkspace ? homePath : routePaths.home }, { label: 'Explore' }]}
        title="Explore Heritage"
        description="Discover the districts, villages, crafts and people behind Bangladesh's living heritage."
      />
      <div className="mb-10 max-w-xl">
        <SearchBar placeholder="Search every craft type…" value={search} onChange={(event) => setSearch(event.target.value)} />
      </div>
      {term && (
        <section className="mb-10" aria-live="polite">
          <div className="mb-4"><p className="text-xs font-semibold uppercase tracking-[0.14em] text-primary">Craft results</p><h2 className="text-2xl font-semibold text-heading">{results.length} matching {results.length === 1 ? 'craft' : 'crafts'}</h2></div>
          {results.length ? <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">{results.map((category) => <EntityCard key={category.id} title={category.name} subtitle={category.description} meta="Explore this craft" image={category.imageUrl} to={routePaths.exploreCraftDetails.replace(':craftId',category.id)} />)}</div> : <p className="rounded-xl border border-border bg-surface p-5 text-sm text-body/70">No craft category matches “{search}”. Try pottery, jute, bamboo, weaving, embroidery or metalwork.</p>}
        </section>
      )}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {sections.map((section) => (
          <EntityCard key={section.title} title={section.title} subtitle={section.description} to={section.to} image={section.image} />
        ))}
      </div>
    </div>
  );
}
