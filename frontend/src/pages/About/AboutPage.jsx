import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader } from '../../components/ui';

const values = [
  { title: 'Mission', description: 'Preserve and elevate Bangladesh’s heritage crafts through a connected digital ecosystem.' },
  { title: 'Vision', description: 'A thriving network where artisans, producers, communities and heritage learners can participate in the digital economy.' },
];

const stakeholders = [
  'Artisans & Producers',
  'Customers',
  'Tourists',
  'Business Partners',
  'Academy Members',
  'Researchers',
  'Government & NGOs',
  'Logistics Partners',
];

const capabilities = [
  { title: 'Explore Heritage', description: 'Browse districts, villages, crafts and cultural collections.', to: routePaths.explore },
  { title: 'Marketplace', description: 'Discover heritage products, auctions and producer stories.', to: routePaths.marketplace },
  { title: 'Heritage Tourism', description: 'Find festivals, routes, cuisine and tourist services.', to: routePaths.tourism },
  { title: 'Academy', description: 'Browse courses, mentors, live classes and certifications.', to: routePaths.academy },
  { title: 'Innovation Hub', description: 'Access the research landing area and authenticated research tools.', to: routePaths.research },
];

export default function AboutPage() {
  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'About' }]}
        title="About ShilpoHub"
        description="A digital heritage ecosystem connecting the people who create, sustain, learn, research and celebrate Bangladesh's craft traditions."
      />

      <div className="mb-10 grid gap-4 sm:grid-cols-2">
        {values.map((value) => (
          <div key={value.title} className="rounded-xl border border-border bg-surface p-5">
            <p className="text-sm font-semibold text-heading">{value.title}</p>
            <p className="mt-2 text-sm leading-6 text-body/70">{value.description}</p>
          </div>
        ))}
      </div>

      <SectionHeader eyebrow="Who it serves" title="A Multi-role Ecosystem" />
      <div className="mb-10 flex flex-wrap gap-2">
        {stakeholders.map((stakeholder) => (
          <span key={stakeholder} className="rounded-full border border-border bg-surface px-3 py-1.5 text-xs text-body">
            {stakeholder}
          </span>
        ))}
      </div>

      <SectionHeader eyebrow="Platform" title="Explore What ShilpoHub Supports" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {capabilities.map((capability) => (
          <Link
            key={capability.title}
            to={capability.to}
            className="rounded-xl border border-border bg-surface p-5 transition hover:-translate-y-0.5 hover:border-primary/30 hover:shadow-sm"
          >
            <p className="text-sm font-semibold text-heading">{capability.title}</p>
            <p className="mt-2 text-sm leading-6 text-body/65">{capability.description}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
