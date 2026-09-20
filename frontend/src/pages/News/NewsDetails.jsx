import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../../services/apiClient';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import CardMedia from '../../components/media/CardMedia';

export default function NewsDetails() {
  const { newsId } = useParams();
  const query = useQuery({ queryKey:['cms','news',newsId], queryFn:() => apiClient.get(`/cms/news/${newsId}`).then(response => response.data), enabled:Boolean(newsId) });
  const item = query.data;
  return <main className="mx-auto max-w-3xl px-4 py-10 lg:px-8"><AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>{item && <><PageHeader breadcrumbs={[{label:'Home',path:'/'},{label:'News',path:'/news'},{label:item.title}]} title={item.title}/><div className="mb-6 flex items-center gap-3"><Badge tone="secondary">News</Badge><span className="text-xs text-body/50">{new Date(item.publishedAt || item.createdAt).toLocaleDateString()}</span></div>{item.imageUrl && <div className="mb-8 overflow-hidden rounded-2xl"><CardMedia src={item.imageUrl} name={item.title}/></div>}<p className="whitespace-pre-line text-base leading-8 text-body/80">{item.content || item.summary}</p></>}</AsyncState></main>;
}
