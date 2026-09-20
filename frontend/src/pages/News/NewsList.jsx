import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../../services/apiClient';
import { PageHeader, Badge, AsyncState } from '../../components/ui';

export default function NewsList() {
  const query = useQuery({ queryKey: ['cms','news'], queryFn: () => apiClient.get('/cms/news', {params:{page:1,pageSize:20}}).then(response => response.data) });
  const items = query.data?.items || [];
  return <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8"><PageHeader breadcrumbs={[{label:'Home',path:'/'},{label:'News'}]} title="News" description="Published updates from across the ShilpoHub ecosystem."/><AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}><div className="divide-y divide-border rounded-xl border border-border bg-surface">{items.map(item => <Link key={item.id} to={`/updates/news/${item.id}`} className="flex flex-col gap-2 p-5 hover:bg-background sm:flex-row sm:items-center sm:justify-between"><div><Badge tone="secondary" className="mb-2">News</Badge><p className="text-sm font-semibold text-heading">{item.title}</p><p className="mt-1 text-sm text-body/60">{item.summary}</p></div><p className="shrink-0 text-xs text-body/50">{new Date(item.publishedAt || item.createdAt).toLocaleDateString()}</p></Link>)}{!items.length && <p className="p-8 text-center text-body/60">No published news yet.</p>}</div></AsyncState></div>;
}
