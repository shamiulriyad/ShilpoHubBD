import { routePaths } from '../../routes/routePaths';
import { PageHeader, ChartPlaceholder, SectionHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useResearchPublications } from '../../hooks/useResearchPublications';
import { useAuth } from '../../hooks/useAuth';

const links = [
  { title: 'Research Workspace', description: 'Ongoing research projects', to: routePaths.researchWorkspace },
  { title: 'Publications', description: 'Papers, reports & case studies', to: routePaths.researchPublications },
  { title: 'Heritage Database', description: 'Open heritage datasets', to: routePaths.researchHeritageDatabase },
  { title: 'AI Research Assistant', description: 'Insights, trends, correlations & citations', to: routePaths.researchAiAssistant },
  { title: 'Field Research', description: 'Surveys, field researchers, responses & evidence', to: routePaths.researchFieldResearch },
  { title: 'Knowledge Graph', description: 'Curate heritage knowledge nodes & relationships', to: routePaths.researchKnowledgeGraph },
  { title: 'Preservation Strategies', description: 'Objectives and action plans for heritage preservation', to: routePaths.innovationPreservationStrategies },
  { title: 'Innovation Experiments', description: 'AI/ML experiments, versions and training runs', to: routePaths.innovationExperiments },
];

export default function InnovationHubHome() {
  const { isAuthenticated } = useAuth();
  const { data, isLoading, isError, error } = useResearchPublications({ pageSize: 3 }, isAuthenticated);
  const publications = data?.items || [];

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
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {publications.map((pub) => (
            <div key={pub.id} className="rounded-xl border border-border bg-surface p-4">
              <p className="text-sm font-semibold text-heading">{pub.title}</p>
              <p className="mt-1 text-xs text-body/60">
                {pub.authors}
                {pub.publishedOn ? ` · ${new Date(pub.publishedOn).getFullYear()}` : ''}
              </p>
            </div>
          ))}
          {publications.length === 0 && <p className="text-sm text-body/60">No publications yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
