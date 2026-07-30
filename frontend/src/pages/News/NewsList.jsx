import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Pagination } from '../../components/ui';
import { newsItems } from '../../data/mockData';

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
      <div className="mt-8">
        <Pagination currentPage={1} totalPages={3} />
      </div>
    </div>
  );
}
