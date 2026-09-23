import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useNotifications, useNotificationActions } from '../../hooks/useNotifications';
import { useAuth } from '../../hooks/useAuth';

export function BellIcon({ className = 'h-5 w-5' }) {
  return <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" aria-hidden="true"><path strokeLinecap="round" strokeLinejoin="round" d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4" /></svg>;
}
function dateLabel(value) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? 'Recent update' : new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' }).format(date);
}
export default function NotificationCenter({ compact = false, onNavigate }) {
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [page, setPage] = useState(1);
  const [feedback, setFeedback] = useState('');
  const query = useNotifications({ unreadOnly, page });
  const action = useNotificationActions();
  const { homePath } = useAuth();
  const items = query.data?.items ?? [];
  const count = query.data?.unreadCount ?? 0;
  const total = query.data?.totalCount ?? 0;
  const mutate = async (value, message) => {
    setFeedback('');
    try { await action.mutateAsync(value); setFeedback(message); } catch { /* Error displayed below. */ }
  };
  const target = item => {
    const path = item.targetPath;
    if (!path || !path.startsWith('/') || path.startsWith('//')) return null;
    return path.startsWith('/dashboard/') || (homePath && (path === homePath || path.startsWith(`${homePath}/`))) || (homePath === '/tourist' && path.startsWith('/tourism/')) ? path : null;
  };
  return <section aria-label="Your notifications" className="overflow-hidden rounded-2xl border border-border bg-surface">
    <div className="flex items-start justify-between gap-4 border-b border-border p-5">
      <div><h2 className="text-base font-semibold text-heading">Your inbox</h2><p className="mt-1 text-xs text-body/70">{query.isPending ? 'Checking for updates…' : query.isError ? 'Updates unavailable' : `${count} unread ${count === 1 ? 'notification' : 'notifications'}`}</p></div>
      <button type="button" className="rounded-lg px-2 py-1 text-xs font-semibold text-primary hover:bg-primary/5 disabled:opacity-40" disabled={!count || action.isPending || query.isError || !query.data?.asOf} onClick={() => mutate({ all: true, through: query.data.asOf }, 'All current notifications marked as read.')}>Mark all read</button>
    </div>
    <div className="flex items-center gap-2 border-b border-border px-5 py-3">
      {[[false, 'All updates'], [true, 'Unread']].map(([value, label]) => <button key={label} type="button" aria-pressed={unreadOnly === value} onClick={() => { setUnreadOnly(value); setPage(1); }} className={`rounded-full px-4 py-2 text-xs font-semibold transition ${unreadOnly === value ? 'bg-title text-white' : 'bg-background text-body hover:bg-primary/10'}`}>{label}</button>)}
      <button type="button" onClick={() => query.refetch()} disabled={query.isFetching} className="ml-auto rounded-lg px-2 py-2 text-xs text-primary disabled:opacity-50">{query.isFetching ? 'Updating…' : 'Refresh'}</button>
    </div>
    <p role="status" className={feedback ? 'px-5 pt-3 text-xs text-primary' : 'sr-only'}>{feedback}</p>
    {action.isError && <p role="alert" className="m-4 rounded-lg bg-red-50 p-3 text-sm text-red-800">We couldn’t update the read status. Please try again.</p>}
    {query.isError ? <div role="alert" className="p-8 text-center"><p className="font-medium text-heading">Notifications couldn’t load</p><p className="mt-2 text-sm text-body/70">Check your connection and try again.</p><button type="button" onClick={() => query.refetch()} className="mt-4 rounded-lg border border-border px-4 py-2 text-sm">Try again</button></div>
      : query.isPending ? <div className="space-y-4 p-5" aria-label="Loading notifications">{[1, 2, 3].map(i => <div key={i} className="h-20 animate-pulse rounded-xl bg-background motion-reduce:animate-none" />)}</div>
      : !items.length ? <div className="px-6 py-12 text-center"><span className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-background text-primary"><BellIcon /></span><h3 className="mt-4 font-semibold text-heading">{page > 1 ? 'No more updates' : unreadOnly ? 'You’re all caught up' : 'Your updates will appear here'}</h3><p className="mx-auto mt-2 max-w-sm text-sm leading-6 text-body/70">{unreadOnly ? 'There are no unread notifications. View all updates to revisit earlier activity.' : 'Messages, approvals and activity related to your account will appear as they happen.'}</p></div>
      : <ul className={compact ? 'max-h-[min(55vh,28rem)] divide-y divide-border overflow-y-auto overscroll-contain' : 'divide-y divide-border'}>{items.map(item => <li key={item.id} className={`flex gap-3 p-5 ${item.readAt ? '' : 'bg-primary/[0.035]'}`}>
        <span className={`mt-1 flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${item.readAt ? 'bg-background text-body/50' : 'bg-primary/10 text-primary'}`}><BellIcon className="h-4 w-4" /></span>
        <div className="min-w-0 flex-1"><div className="flex items-start gap-2"><h3 className="text-sm font-semibold leading-5 text-heading">{item.title}</h3>{!item.readAt && <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-primary" aria-label="Unread" />}</div><p className="mt-1 break-words text-sm leading-6 text-body/75">{item.body}</p><p className="mt-2 text-[11px] text-body/60">{item.category} · <time dateTime={item.createdAt}>{dateLabel(item.createdAt)}</time></p><div className="mt-3 flex flex-wrap items-center gap-4">
          {target(item) && <Link to={target(item)} onClick={() => { if (!item.readAt) action.mutate({ id: item.id, isRead: true }); onNavigate?.(); }} className="text-xs font-semibold text-primary hover:underline">View activity <span aria-hidden="true">→</span></Link>}
          <button type="button" disabled={action.isPending} onClick={() => mutate({ id: item.id, isRead: !item.readAt }, item.readAt ? 'Marked as unread.' : 'Marked as read.')} className="text-xs text-body/70 underline decoration-border underline-offset-4 hover:text-primary disabled:opacity-50">{item.readAt ? 'Mark unread' : 'Mark read'}</button>
        </div></div>
      </li>)}</ul>}
    {!query.isError && (total > 20 || page > 1) && <div className="flex items-center justify-between border-t border-border px-5 py-3 text-xs"><button type="button" disabled={page === 1 || query.isFetching} onClick={() => setPage(p => p - 1)} className="rounded-lg border border-border px-3 py-2 disabled:opacity-40">Previous</button><span>Page {page} of {Math.max(page, Math.ceil(total / 20))}</span><button type="button" disabled={page * 20 >= total || query.isFetching} onClick={() => setPage(p => p + 1)} className="rounded-lg border border-border px-3 py-2 disabled:opacity-40">Next</button></div>}
    {compact && <Link to="/dashboard/notifications" onClick={onNavigate} className="block border-t border-border p-4 text-center text-sm font-semibold text-primary hover:bg-background">Open notification center →</Link>}
  </section>;
}
