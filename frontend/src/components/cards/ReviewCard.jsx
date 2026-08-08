export default function ReviewCard({ review }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-4">
      <div className="flex items-center justify-between">
        <p className="text-sm font-medium text-heading">{review.author}</p>
        <span className="text-xs text-secondary">{'★'.repeat(review.rating)}</span>
      </div>
      <p className="mt-2 text-sm text-body/70">{review.comment}</p>
      <p className="mt-1 text-xs text-body/50">{review.date}</p>
    </div>
  );
}
