import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, AsyncState } from '../../components/ui';
import { useResearchPublications } from '../../hooks/useResearchPublications';
import { useAuth } from '../../hooks/useAuth';

const yearOf = (publication) => (publication.publishedOn ? new Date(publication.publishedOn).getFullYear() : null);

export default function Publications() {
  const { isAuthenticated } = useAuth();
  const [year, setYear] = useState('');
  const query = useResearchPublications({ pageSize: 50, year: year ? Number(year) : undefined }, isAuthenticated);
  const publications = query.data?.items || [];

  const availableYears = [...new Set(publications.map(yearOf).filter(Boolean))].sort((a, b) => b - a);
  const currentYear = new Date().getFullYear();
  const yearOptions = availableYears.length > 0
    ? availableYears
    : Array.from({ length: 8 }, (_, index) => currentYear - index);

  const filterGroups = [
    { key: 'year', label: 'Year', options: yearOptions.map((value) => ({ label: String(value), value: String(value) })) },
  ];

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

      {!isAuthenticated ? (
        <div className="rounded-xl border border-border bg-surface p-6 text-sm text-body/70">
          Sign in to browse the authenticated publication repository.
        </div>
      ) : (
        <div className="grid gap-6 lg:grid-cols-[260px_minmax(0,1fr)]">
          <FilterPanel
            groups={filterGroups}
            values={{ year }}
            onChange={(_key, value, checked) => setYear(checked ? value : '')}
            onClear={() => setYear('')}
          />
          <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>
            <div className="space-y-3">
              {publications.map((publication) => (
                <div key={publication.id} className="rounded-xl border border-border bg-surface p-4">
                  <p className="text-sm font-semibold text-heading">{publication.title}</p>
                  <p className="mt-1 text-xs text-body/60">
                    {publication.authors}
                    {yearOf(publication) ? ` · ${yearOf(publication)}` : ''}
                    {publication.venue ? ` · ${publication.venue}` : ''}
                  </p>
                </div>
              ))}
              {publications.length === 0 && <p className="text-sm text-body/60">No publications match this filter.</p>}
            </div>
          </AsyncState>
        </div>
      )}
    </div>
  );
}
