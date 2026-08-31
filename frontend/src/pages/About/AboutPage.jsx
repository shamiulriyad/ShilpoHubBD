import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader } from '../../components/ui';
import { StatCard } from '../../components/cards';

// TODO(backend): no platform-stats or timeline endpoint — editorial content.
const heritageStats = [
  { label: 'Registered Producers', value: '12,400+' },
  { label: 'Heritage Villages', value: '640+' },
  { label: 'Heritage Products', value: '8,900+' },
  { label: 'Districts Covered', value: '64' },
];

const timeline = [
  { year: '1971', label: 'Independence & the revival of national craft identity' },
  { year: '1985', label: 'First national craft cooperatives established' },
  { year: '2013', label: 'Jamdani recognized by UNESCO' },
  { year: '2020', label: 'Digital heritage documentation begins' },
  { year: '2026', label: 'ShilpoHub national ecosystem launches' },
];

const values = [
  { title: 'Mission', description: 'Preserve and elevate Bangladesh’s heritage crafts through a connected digital ecosystem.' },
  { title: 'Vision', description: 'A thriving national network where artisans, producers and communities prosper together.' },
];

const stakeholders = [
  'Artisans & Craftspeople',
  'Farmers & Producers',
  'Customers',
  'Tourists',
  'Business Partners',
  'Researchers',
  'Government Bodies',
  'NGOs',
];

export default function AboutPage() {
  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'About' }]}
        title="About ShilpoHub"
        description="A national heritage ecosystem connecting the people who create, sustain and celebrate Bangladesh's craft traditions."
      />

      <div className="mb-10 grid gap-4 sm:grid-cols-2">
        {values.map((value) => (
          <div key={value.title} className="rounded-xl border border-border bg-surface p-5">
            <p className="text-sm font-semibold text-heading">{value.title}</p>
            <p className="mt-2 text-sm text-body/70">{value.description}</p>
          </div>
        ))}
      </div>

      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-4">
        {heritageStats.map((stat) => (
          <StatCard key={stat.label} label={stat.label} value={stat.value} />
        ))}
      </div>

      <SectionHeader eyebrow="Who we serve" title="An Ecosystem for Everyone" />
      <div className="mb-10 flex flex-wrap gap-2">
        {stakeholders.map((s) => (
          <span key={s} className="rounded-full border border-border bg-surface px-3 py-1.5 text-xs text-body">
            {s}
          </span>
        ))}
      </div>

      <SectionHeader eyebrow="History" title="Our Journey" />
      <div className="grid gap-4 sm:grid-cols-5">
        {timeline.map((item) => (
          <div key={item.year} className="rounded-xl border border-border bg-surface p-4">
            <p className="text-lg font-bold text-primary">{item.year}</p>
            <p className="mt-1 text-xs text-body/70">{item.label}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
