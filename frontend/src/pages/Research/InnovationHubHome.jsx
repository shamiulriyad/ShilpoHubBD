import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useResearchPublications } from '../../hooks/useResearchPublications';
import { useAuth } from '../../hooks/useAuth';

const links = [
  { title: 'Research Workspace', description: 'Ongoing research projects', to: routePaths.researchWorkspace, authenticated: true },
  { title: 'Publications', description: 'Papers, reports and case studies', to: routePaths.researchPublications, authenticated: true },
  { title: 'AI Research Assistant', description: 'Insights, trends, correlations and citations', to: routePaths.researchAiAssistant, authenticated: true },
  { title: 'Field Research', description: 'Surveys, field researchers, responses and evidence', to: routePaths.researchFieldResearch, authenticated: true },
  { title: 'Preservation Strategies', description: 'Objectives and action plans for heritage preservation', to: routePaths.innovationPreservationStrategies, authenticated: true },
  { title: 'Innovation Experiments', description: 'AI/ML experiments, versions and training runs', to: routePaths.innovationExperiments, authenticated: true },
  { title: 'Innovation Submissions', description: 'Submit heritage innovation ideas for review', to: routePaths.innovationSubmissions, authenticated: true },
  { title: 'Innovation Prototypes', description: 'Iterations, test cases and issue tracking', to: routePaths.innovationPrototypes, authenticated: true },
  {
    title: 'Heritage Database',
    description: 'Curated heritage datasets for authorized research and government teams',
    to: routePaths.researchHeritageDatabase,
    roles: ['HeritageInnovationHub', 'GovernmentNGO', 'SuperAdmin'],
  },
  {
    title: 'Knowledge Graph',
    description: 'Curate heritage knowledge nodes and relationships',
    to: routePaths.researchKnowledgeGraph,
    roles: ['HeritageInnovationHub', 'GovernmentNGO', 'SuperAdmin'],
  },
];

export default function InnovationHubHome() {
  const { isAuthenticated, hasAnyRole } = useAuth();
  const { data, isLoading, isError, error } = useResearchPublications({ pageSize: 3 }, isAuthenticated);
  const publications = data?.items || [];
  const visibleLinks = links.filter((link) => !link.roles || hasAnyRole(link.roles));

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'Innovation Hub' }]}
        title="Innovation Hub"
        description="Research workflows, publications and role-controlled heritage data for the ShilpoHub ecosystem."
      />

      {!isAuthenticated && (
        <div className="mb-8 rounded-xl border border-border bg-surface p-4 text-sm text-body/70">
          Research workspaces require an authenticated account.{' '}
          <Link to={routePaths.login} className="font-medium text-link hover:underline">Sign in to continue →</Link>
        </div>
      )}

      <div className="mb-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {visibleLinks.map((link) => (
          <EntityCard key={link.title} title={link.title} subtitle={link.description} to={link.to} />
        ))}
      </div>

      <SectionHeader eyebrow="Repository" title="Recent Publications" />
      {isAuthenticated ? (
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          <div className="space-y-3">
            {publications.map((publication) => (
              <div key={publication.id} className="rounded-xl border border-border bg-surface p-4">
                <p className="text-sm font-semibold text-heading">{publication.title}</p>
                <p className="mt-1 text-xs text-body/60">
                  {publication.authors}
                  {publication.publishedOn ? ` · ${new Date(publication.publishedOn).getFullYear()}` : ''}
                </p>
              </div>
            ))}
            {publications.length === 0 && <p className="text-sm text-body/60">No publications are available yet.</p>}
          </div>
        </AsyncState>
      ) : (
        <p className="text-sm text-body/60">Sign in to load the publication repository.</p>
      )}
    </div>
  );
}