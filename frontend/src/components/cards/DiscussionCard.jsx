import { Link } from 'react-router-dom';
import Badge from '../ui/Badge';

export default function DiscussionCard({ thread, to }) {
  return (
    <Link
      to={to || '#'}
      className="group flex flex-col gap-2 rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
    >
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-semibold text-heading group-hover:text-primary">{thread.title}</p>
        <Badge tone="secondary">{thread.category}</Badge>
      </div>
      <p className="text-xs text-body/60">by {thread.author}</p>
      <div className="flex gap-4 text-xs text-body/50">
        <span>{thread.replies} replies</span>
        <span>{thread.views} views</span>
        <span>{thread.lastActivity}</span>
      </div>
    </Link>
  );
}
