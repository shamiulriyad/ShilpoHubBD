import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { newsItems } from './NewsList';

export default function NewsDetails() {
  const { newsId } = useParams();
  const item = newsItems.find((n) => n.id === newsId) || newsItems[0];

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'News', path: routePaths.news },
          { label: item.title },
        ]}
        title={item.title}
      />
      <div className="mb-6 flex items-center gap-3">
        <Badge tone="secondary">{item.category}</Badge>
        <span className="text-xs text-body/50">{item.date}</span>
      </div>
      <div className="mb-8 flex aspect-video items-center justify-center rounded-2xl border border-border bg-surface text-sm text-body/40">
        Article Image Placeholder
      </div>
      {/* TODO(backend): no News API yet — full article body will come from a content endpoint. */}
      <p className="text-sm leading-relaxed text-body/80">
        Full article content will be served from the content API once available.
      </p>
    </div>
  );
}
