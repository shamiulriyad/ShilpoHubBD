import { routePaths } from '../../routes/routePaths';
import { PageHeader, ChartPlaceholder, SectionHeader } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { publications } from '../../data/mockData';

const links = [
  { title: 'Research Workspace', description: 'Ongoing research projects', to: routePaths.researchWorkspace },
  { title: 'Publications', description: 'Papers, reports & case studies', to: routePaths.researchPublications },
  { title: 'Heritage Database', description: 'Open heritage datasets', to: routePaths.researchHeritageDatabase },
];

export default function InnovationHubHome() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'Innovation Hub' }]}
        title="Innovation Hub"
        description="Research, publications and open heritage data powering the ShilpoHub ecosystem."
      />
      <div className="mb-10 grid gap-4 sm:grid-cols-3">
        {links.map((link) => (
          <EntityCard key={link.title} title={link.title} subtitle={link.description} to={link.to} />
        ))}
      </div>

      <SectionHeader eyebrow="Analytics" title="Heritage Analytics Preview" />
      <div className="mb-10 grid gap-4 lg:grid-cols-2">
        <ChartPlaceholder title="Craft Growth by Region" type="bar" />
        <ChartPlaceholder title="Producer Participation" type="donut" />
      </div>

      <SectionHeader eyebrow="Latest" title="Recent Publications" />
      <div className="space-y-3">
        {publications.slice(0, 3).map((pub) => (
          <div key={pub.id} className="rounded-xl border border-border bg-surface p-4">
            <p className="text-sm font-semibold text-heading">{pub.title}</p>
            <p className="mt-1 text-xs text-body/60">
              {pub.author} · {pub.year}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
