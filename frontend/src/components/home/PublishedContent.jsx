import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import apiClient from '../../services/apiClient';
import SafeImage from '../media/SafeImage';

export function safeContentLink(value) {
  return typeof value === 'string' && (/^\/(?!\/)/.test(value) || /^https?:\/\//i.test(value)) ? value : null;
}

export function isQaFixture(content) {
  return /(?:^|\s)-\s*QA\s+\d{8}-\d{6}\s*$/i.test(content?.title || '')
    || /^Persistent QA\b/i.test(content?.message || '');
}

export function selectHomepageCards(items = [], limit = 3) {
  const imageUrls = new Set();

  return items.filter(item => item.isPublished && !isQaFixture(item)).filter(item => {
    const imageUrl = (item.imageUrl || item.coverImageUrl || '').trim().toLowerCase();
    if (!imageUrl) return true;
    if (imageUrls.has(imageUrl)) return false;
    imageUrls.add(imageUrl);
    return true;
  }).slice(0, limit);
}

function usePublishedContent(resource, params) {
  return useQuery({
    queryKey: ['cms-public', resource, params],
    queryFn: () => apiClient.get(`/cms/${resource}`, { params }).then(response => response.data),
    retry: 1,
  });
}

export default function PublishedContent() {
  const sections = usePublishedContent('homepage');
  const announcements = usePublishedContent('announcements', { activeOnly: true });
  const news = usePublishedContent('news', { page: 1, pageSize: 12 });
  const blogs = usePublishedContent('blogs', { page: 1, pageSize: 12 });
  const events = usePublishedContent('events', { page: 1, pageSize: 12 });
  const activeSections = (sections.data || []).filter(section => section.isActive).sort((a, b) => a.displayOrder - b.displayOrder);
  const notices = (announcements.data || []).filter(notice => notice.isActive && !isQaFixture(notice));
  const updates = [
    [selectHomepageCards(news.data?.items), 'news', 'Latest news'],
    [selectHomepageCards(blogs.data?.items), 'blogs', 'Community stories'],
    [selectHomepageCards(events.data?.items), 'events', 'Events'],
  ];

  if (!activeSections.length && !notices.length && !updates.some(([items]) => items.length)) return null;

  return <section aria-label="Community updates" className="mx-auto max-w-7xl space-y-8 px-5 py-12 lg:px-8">
    {notices.map(notice => <aside key={notice.id} className={`rounded-xl border p-5 ${notice.severity === 'Critical' ? 'border-red-200 bg-red-50 text-red-900' : notice.severity === 'Warning' ? 'border-amber-200 bg-amber-50 text-amber-900' : 'border-border bg-surface'}`}><h2 className="text-lg font-semibold">{notice.title}</h2><p className="mt-2 whitespace-pre-wrap text-sm">{notice.message}</p></aside>)}
    {activeSections.map(section => <article key={section.id} className={`grid overflow-hidden rounded-2xl border border-border bg-surface ${section.imageUrl ? 'md:grid-cols-2' : ''}`}>{section.imageUrl && <SafeImage src={section.imageUrl} alt={section.title} className="h-72 w-full object-cover"/>}<div className="p-7"><h2 className="text-3xl">{section.title}</h2>{section.subtitle && <p className="mt-4 whitespace-pre-wrap text-body/70">{section.subtitle}</p>}{safeContentLink(section.linkUrl) && <a href={safeContentLink(section.linkUrl)} className="mt-5 inline-block font-semibold text-primary">Explore more →</a>}</div></article>)}
    {updates.filter(([items]) => items.length).map(([items, kind, title]) => <div key={kind}><h2 className="mb-5 text-2xl">{title}</h2><div className="grid gap-5 md:grid-cols-3">{items.map(item => <Link key={item.id} to={`/updates/${kind}/${item.id}`} className="overflow-hidden rounded-xl border border-border bg-surface">{(item.imageUrl || item.coverImageUrl) && <SafeImage src={item.imageUrl || item.coverImageUrl} alt={item.title} className="h-48 w-full object-cover"/>}<div className="p-5"><h3 className="text-lg">{item.title}</h3><p className="mt-2 text-sm text-body/70">{item.summary || item.location}</p>{item.startDate && <p className="mt-2 text-xs text-body/60">{new Date(item.startDate).toLocaleDateString()}</p>}<span className="mt-4 inline-block text-sm font-semibold text-primary">Read more →</span></div></Link>)}</div></div>)}
  </section>;
}
