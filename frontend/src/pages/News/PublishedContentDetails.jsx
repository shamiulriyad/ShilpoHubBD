import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../../services/apiClient';
import SafeImage from '../../components/media/SafeImage';
import { PageHeader } from '../../components/ui';
import { getApiErrorMessage } from '../../utils/apiError';

export default function PublishedContentDetails() {
  const {kind,id}=useParams();
  const valid=['news','blogs','events'].includes(kind);
  const query=useQuery({queryKey:['cms-public',kind,id],queryFn:()=>apiClient.get(`/cms/${kind}/${encodeURIComponent(id)}`).then(r=>r.data),enabled:valid,retry:1});
  const item=query.data;
  return <article className="mx-auto max-w-3xl px-5 py-12"><Link to="/" className="text-sm text-primary">← Back to homepage</Link>{!valid?<h1 className="mt-8 text-2xl">Page not found</h1>:query.isPending?<p role="status" className="mt-8">Loading story…</p>:query.isError?<p role="alert" className="mt-8">{getApiErrorMessage(query.error,'This story is unavailable.')}</p>:!item?.isPublished?<h1 className="mt-8 text-2xl">This content is not published.</h1>:<div className="mt-6"><PageHeader title={item.title} description={item.summary}/>{(item.imageUrl||item.coverImageUrl)&&<SafeImage src={item.imageUrl||item.coverImageUrl} alt={item.title} className="mb-6 max-h-96 w-full rounded-2xl object-cover"/>}{item.startDate&&<p className="mb-5 text-sm text-body/70">{new Date(item.startDate).toLocaleString()} – {new Date(item.endDate).toLocaleString()} · {item.location}</p>}<div className="whitespace-pre-wrap break-words text-base leading-8">{item.content||item.description}</div>{item.source&&<p className="mt-6 text-sm text-body/60">Source: {item.source}</p>}</div>}</article>;
}
