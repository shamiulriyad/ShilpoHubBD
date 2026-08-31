import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, AsyncState } from '../../components/ui';
import { useResearchPublications } from '../../hooks/useResearchPublications';
import { useAuth } from '../../hooks/useAuth';

const yearOf = (pub) => (pub.publishedOn ? new Date(pub.publishedOn).getFullYear() : null);

export default function Publications() {
  const { isAuthenticated } = useAuth();
  const { data, isLoading, isError, error } = useResearchPublications({ pageSize: 50 }, isAuthenticated);
  const publications = data?.items || [];

  const years = [...new Set(publications.map(yearOf).filter(Boolean))].sort((a, b) => b - a);
  const filterGroups = [{ label: 'Year', options: years.map(String) }];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Innovation Hub', path: routePaths.research },
          { label: 'Publications' },
        ]}
        title="Publications"
        description="Research papers, reports and case studies from the ShilpoHub network."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          <div className="space-y-3">
            {publications.map((pub) => (
              <div key={pub.id} className="rounded-xl border border-border bg-surface p-4">
                <p className="text-sm font-semibold text-heading">{pub.title}</p>
                <p className="mt-1 text-xs text-body/60">
                  {pub.authors}
                  {yearOf(pub) ? ` · ${yearOf(pub)}` : ''}
                  {pub.venue ? ` · ${pub.venue}` : ''}
                </p>
              </div>
            ))}
            {publications.length === 0 && (
              <p className="text-sm text-body/60">
                {isAuthenticated ? 'No publications available yet.' : 'Sign in to browse the publication repository.'}
              </p>
            )}
          </div>
        </AsyncState>
      </div>
    </div>
  );
}
