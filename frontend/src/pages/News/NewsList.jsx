import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';

// TODO(backend): no News API/controller yet. Editorial placeholder list.
export const newsItems = [
  { id: 'news-1', title: 'ShilpoHub Launches Digital Museum Initiative', date: '2026-01-12', category: 'Platform' },
  { id: 'news-2', title: 'Jamdani Village Receives UNESCO Recognition', date: '2026-02-11', category: 'Heritage' },
  { id: 'news-3', title: 'New Academy Cohort Begins This Month', date: '2026-03-13', category: 'Academy' },
  { id: 'news-4', title: 'Innovation Hub Publishes Heritage Dataset', date: '2026-04-14', category: 'Research' },
  { id: 'news-5', title: 'Marketplace Crosses 10,000 Products', date: '2026-05-15', category: 'Marketplace' },
  { id: 'news-6', title: 'Tourism Board Partners with ShilpoHub', date: '2026-06-16', category: 'Tourism' },
];

export default function NewsList() {
  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'News' }]}
        title="News"
        description="Updates from across the ShilpoHub ecosystem."
      />
      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {newsItems.map((item) => (
          <Link
            key={item.id}
            to={routePaths.newsDetails.replace(':newsId', item.id)}
            className="flex flex-col gap-2 p-5 hover:bg-background sm:flex-row sm:items-center sm:justify-between"
          >
            <div>
              <Badge tone="secondary" className="mb-2">
                {item.category}
              </Badge>
              <p className="text-sm font-semibold text-heading">{item.title}</p>
            </div>
            <p className="shrink-0 text-xs text-body/50">{item.date}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
