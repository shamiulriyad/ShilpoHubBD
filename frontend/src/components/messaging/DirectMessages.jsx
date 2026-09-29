import SafeImage from '../media/SafeImage';
import { useEffect, useRef, useState } from 'react';
import { AsyncState } from '../ui';
import ImageAttachButton, { resolveUploadUrl } from './ImageAttachButton';
import { useConversations, useConversation, useMessagingMutations } from '../../hooks/useMessaging';
import { useAuth } from '../../hooks/useAuth';

function ChatIcon({ kind = 'chat', className = 'h-5 w-5' }) {
  return <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    {kind === 'search' ? <><circle cx="10.5" cy="10.5" r="6.5" /><path d="m16 16 4 4" /></> : kind === 'back' ? <path d="m14 6-6 6 6 6" /> : kind === 'send' ? <><path d="m21 3-7 18-4-7-7-4 18-7Z" /><path d="m10 14 11-11" /></> : <><path d="M21 11.5a8.5 8.5 0 0 1-8.5 8.5H4l-1 1V11.5a8.5 8.5 0 0 1 17 0Z" /><path d="M7 10h9M7 14h5" /></>}
  </svg>;
}
const initials = (name = '') => name.trim().split(/\s+/).slice(0, 2).map(part => part[0]).join('').toUpperCase() || '?';
const dayKey = value => new Date(value).toLocaleDateString();
function dateLabel(value) {
  const date = new Date(value);
  const yesterday = new Date(); yesterday.setDate(yesterday.getDate() - 1);
  if (dayKey(value) === dayKey(new Date())) return 'Today';
  if (dayKey(value) === dayKey(yesterday)) return 'Yesterday';
  return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: date.getFullYear() === new Date().getFullYear() ? undefined : 'numeric' });
}
const timeLabel = value => new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

// Shared inbox for every workspace; uses the existing messaging and upload services.
export default function DirectMessages({ className = '' }) {
  const { user } = useAuth();
  const [activeId, setActiveId] = useState(null);
  const [mobileThread, setMobileThread] = useState(false);
  const [search, setSearch] = useState('');
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [drafts, setDrafts] = useState({});
  const conversationsQuery = useConversations();
  const conversationQuery = useConversation(activeId);
  const { sendMessage, markAsRead } = useMessagingMutations();
  const threadRef = useRef(null);
  const conversations = conversationsQuery.data?.items || [];
  const active = conversations.find(c => c.id === activeId);
  const messages = conversationQuery.data?.messages || [];
  const { body: draft = '', image = '' } = drafts[activeId] || {};
  const updateDraft = changes => setDrafts(previous => ({ ...previous, [activeId]: { ...previous[activeId], ...changes } }));
  const unread = conversations.filter(c => c.unreadCount > 0).length;
  const visible = conversations.filter(c => (!unreadOnly || c.unreadCount > 0) && `${c.otherUserName} ${c.lastMessageBody || ''}`.toLowerCase().includes(search.toLowerCase()));

  useEffect(() => {
    if (!activeId && conversations.length > 0) setActiveId(conversations[0].id);
  }, [activeId, conversations]);
  useEffect(() => {
    if (activeId && (mobileThread || window.matchMedia('(min-width: 640px)').matches)) markAsRead.mutate(activeId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeId, messages.length, mobileThread]);
  useEffect(() => {
    if (threadRef.current) threadRef.current.scrollTop = threadRef.current.scrollHeight;
  }, [messages.length, activeId, mobileThread]);

  const send = event => {
    event.preventDefault();
    if (!activeId || sendMessage.isPending || (!draft.trim() && !image)) return;
    const sentId = activeId;
    sendMessage.mutate({ id: sentId, body: draft.trim(), imageUrl: image || undefined }, {
      onSuccess: () => setDrafts(previous => ({ ...previous, [sentId]: { body: '', image: '' } })),
    });
  };

  return <div className={`direct-messages grid min-h-0 grid-cols-1 overflow-hidden rounded-2xl border border-border bg-surface shadow-sm sm:grid-cols-[minmax(240px,33%)_minmax(0,1fr)] ${className}`}>
    <aside aria-label="Conversation inbox" className={`${mobileThread ? 'hidden sm:flex' : 'flex'} min-h-0 flex-col border-border sm:border-r`}>
      <div className="space-y-4 border-b border-border p-5">
        <div className="flex items-center justify-between"><h2 className="text-base font-semibold text-heading">Inbox <span className="ml-1 text-sm font-normal text-body/50">{conversations.length}</span></h2><ChatIcon className="h-5 w-5 text-primary/60" /></div>
        <label className="flex items-center gap-2 rounded-xl border border-border bg-background px-3 py-2.5 text-body/50"><ChatIcon kind="search" className="h-4 w-4 shrink-0" /><input aria-label="Search conversations" value={search} onChange={e => setSearch(e.target.value)} placeholder="Search conversations" className="min-w-0 w-full bg-transparent text-sm text-heading outline-none" /></label>
        <div className="flex rounded-lg border border-border bg-background/30 p-0.5" aria-label="Inbox filters">{[[false, 'All'], [true, `Unread${unread ? ` (${unread})` : ''}`]].map(([value, label]) => <button key={label} type="button" aria-pressed={unreadOnly === value} onClick={() => setUnreadOnly(value)} className={`flex-1 rounded-md px-3 py-2 text-sm font-medium transition ${unreadOnly === value ? 'bg-primary/10 text-primary' : 'text-body/60 hover:bg-background'}`}>{label}</button>)}</div>
      </div>
      <div className="min-h-0 flex-1 overflow-y-auto">
        <AsyncState isLoading={conversationsQuery.isLoading} isError={conversationsQuery.isError} error={conversationsQuery.error}>
          {visible.map(c => <button key={c.id} type="button" aria-current={c.id === activeId ? 'true' : undefined} onClick={() => { setActiveId(c.id); setMobileThread(true); sendMessage.reset(); }} className={`inbox-row flex w-full items-center gap-3 border-b border-l-2 px-4 py-5 text-left transition focus-visible:outline-primary ${c.id === activeId ? 'border-l-primary border-b-border/50 bg-primary/5' : 'border-l-transparent border-b-border/50 hover:bg-background'}`}>
            <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">{initials(c.otherUserName)}</span>
            <div className="min-w-0 flex-1"><div className="flex items-center justify-between gap-2"><p className="truncate text-sm font-semibold text-heading">{c.otherUserName}</p>{c.lastMessageAt && <span className="shrink-0 text-[11px] text-body/50">{dayKey(c.lastMessageAt) === dayKey(new Date()) ? timeLabel(c.lastMessageAt) : dateLabel(c.lastMessageAt)}</span>}</div><div className="mt-1 flex items-center gap-2"><p className="min-w-0 flex-1 truncate text-xs text-body/60">{c.lastMessageBody || 'Start a conversation'}</p>{c.unreadCount > 0 && <span aria-label={`${c.unreadCount} unread messages`} className="flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-white">{c.unreadCount}</span>}</div></div>
          </button>)}
          {!visible.length && <div className="px-4 py-10 text-center"><p className="text-sm font-medium text-heading">{conversations.length ? 'No matching conversations' : 'Your conversations start here'}</p><p className="mt-2 text-xs leading-5 text-body/60">{conversations.length ? 'Try another search or view all messages.' : 'Use “Message producer” on a product or profile to get in touch.'}</p></div>}
        </AsyncState>
      </div>
    </aside>
    {active ? <section aria-label={`Conversation with ${active.otherUserName}`} className={`${mobileThread ? 'flex' : 'hidden sm:flex'} min-h-0 min-w-0 flex-col`}>
      <header className="flex shrink-0 items-center gap-3 border-b border-border px-4 py-4 sm:px-6">
        <button type="button" aria-label="Back to inbox" onClick={() => setMobileThread(false)} className="rounded-lg p-2 text-primary hover:bg-background sm:hidden"><ChatIcon kind="back" /></button>
        <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">{initials(active.otherUserName)}</span>
        <div><h2 className="font-serif text-xl font-semibold text-heading">{active.otherUserName}</h2><p className="mt-0.5 text-xs text-body/60">Direct conversation</p></div>
      </header>
      <div ref={threadRef} role="log" aria-label="Message history" aria-live="polite" className="min-h-0 flex-1 overflow-y-auto overscroll-contain bg-surface px-4 py-5 sm:px-6">
        <AsyncState isLoading={conversationQuery.isLoading} isError={conversationQuery.isError} error={conversationQuery.error} loadingText="Loading conversation…">
          {messages.map((m, index) => {
            const self = m.senderId === user?.id;
            const newDay = index === 0 || dayKey(messages[index - 1].createdAt) !== dayKey(m.createdAt);
            return <div key={m.id}>
              {newDay && <div className="my-5 flex items-center gap-4"><span className="h-px flex-1 bg-border/70" /><span className="rounded-full bg-background px-5 py-1.5 text-xs font-medium text-body/60">{dateLabel(m.createdAt)}</span><span className="h-px flex-1 bg-border/70" /></div>}
              <div className={`mb-3 flex items-end gap-2 ${self ? 'justify-end' : 'justify-start'}`}>{!self && <span aria-hidden="true" className="mb-5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">{initials(active.otherUserName)}</span>}<div className="max-w-[85%] sm:max-w-[75%] lg:max-w-[65%]">
                <div className={`overflow-hidden break-words rounded-2xl px-4 py-3 text-sm leading-6 shadow-sm ${self ? 'rounded-br-md bg-primary text-white' : 'rounded-bl-md bg-[#f0eeeb] text-heading'}`}>
                  {m.imageUrl && <a href={resolveUploadUrl(m.imageUrl)} target="_blank" rel="noreferrer" aria-label="Open attached picture"><SafeImage src={resolveUploadUrl(m.imageUrl)} alt="Message attachment" className="mb-2 max-h-64 max-w-full rounded-lg object-contain" loading="lazy" /></a>}
                  {m.body && <p className="whitespace-pre-wrap [overflow-wrap:anywhere]">{m.body}</p>}
                </div><time dateTime={m.createdAt} title={new Date(m.createdAt).toLocaleString()} className={`mt-1 block px-1 text-[11px] text-body/50 ${self ? 'text-right' : ''}`}>{timeLabel(m.createdAt)}</time>
              </div></div>
            </div>;
          })}
          {!messages.length && <div className="py-14 text-center"><p className="text-sm font-medium text-heading">Say hello to {active.otherUserName}</p><p className="mt-2 text-xs text-body/60">Send a message or share a picture to start the conversation.</p></div>}
        </AsyncState>
      </div>
      <form onSubmit={send} className="shrink-0 border-t border-border bg-surface p-3 sm:p-4">
        {image && <div className="mb-3 flex items-center gap-3 rounded-xl bg-background p-2"><SafeImage src={resolveUploadUrl(image)} alt="Picture ready to send" className="h-14 w-14 rounded-lg object-cover" /><div><p className="text-xs font-medium">Picture attached</p><button type="button" disabled={sendMessage.isPending} onClick={() => updateDraft({ image: '' })} className="text-xs text-primary underline">Remove picture</button></div></div>}
        <div className="flex items-center gap-2 sm:gap-3">
          <ImageAttachButton compact onUploaded={url => setDrafts(previous => ({ ...previous, [activeId]: { ...previous[activeId], image: url } }))} disabled={sendMessage.isPending} />
          <textarea aria-label="Message" rows={1} disabled={sendMessage.isPending} value={draft} onChange={e => updateDraft({ body: e.target.value })} onKeyDown={e => { if (e.key === 'Enter' && !e.shiftKey && !e.nativeEvent.isComposing) { e.preventDefault(); e.currentTarget.form.requestSubmit(); } }} placeholder="Type a message…" maxLength={4000} className="block max-h-32 min-h-12 min-w-0 flex-1 resize-y rounded-3xl border border-border bg-surface px-5 py-3 text-sm leading-6 outline-none focus:border-primary/50 focus:ring-2 focus:ring-primary/5" />
          <button type="submit" aria-label={sendMessage.isPending ? 'Sending message' : 'Send message'} title="Send message" className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-primary text-white transition hover:bg-primary-dark focus-visible:ring-4 focus-visible:ring-primary/20 disabled:opacity-40" disabled={sendMessage.isPending || (!draft.trim() && !image)}><ChatIcon kind="send" className="h-5 w-5" /></button>
        </div>
        {sendMessage.isError && <p role="alert" className="mt-2 text-xs text-error">Could not send your message. Your draft is saved here — please try again.</p>}
      </form>
    </section> : <div className="hidden flex-col items-center justify-center gap-3 bg-background/50 p-8 text-center sm:flex"><span className="rounded-2xl bg-primary/10 p-4 text-primary"><ChatIcon className="h-7 w-7" /></span><h2 className="text-lg font-semibold text-heading">A conversation makes a connection</h2><p className="max-w-xs text-sm leading-6 text-body/60">Choose a conversation to share ideas, ask questions, and keep things moving.</p></div>}
  </div>;
}
