import { useState } from 'react';
import { Button } from '../ui';
import MutationFeedback from '../ui/MutationFeedback';
import { useReviewMutations } from '../../hooks/useReviews';

// "Rate this product" for one delivered order item. Uses the same POST /api/reviews as the product page.
export default function RateProductForm({ productId, productName }) {
  const { create } = useReviewMutations(productId);
  const [open, setOpen] = useState(false);
  const [rating, setRating] = useState(5);
  const [producerRating, setProducerRating] = useState(5);
  const [comment, setComment] = useState('');

  const submit = (e) => {
    e.preventDefault();
    if (!comment.trim()) return;
    create.mutate({ productId, rating: Number(rating), producerRating: Number(producerRating), comment: comment.trim(), imageUrls: [] }, {
      onSuccess: () => { setOpen(false); setComment(''); setRating(5); setProducerRating(5); },
    });
  };

  if (!open) {
    return (
      <div className="my-1.5 flex flex-col items-end gap-1.5">
        <Button type="button" variant="primary" size="sm" onClick={() => setOpen(true)}>★ Rate this product</Button>
        <MutationFeedback mutation={create} successMessage="Thanks for rating!" />
      </div>
    );
  }

  const selectClass = 'mt-1 block rounded-md border border-border bg-surface px-3 py-2 text-sm';
  return (
    <form onSubmit={submit} className="mt-2 w-full space-y-2 rounded-lg border border-border bg-background p-3 text-left">
      <p className="text-xs font-semibold text-heading">Rate {productName}</p>
      <div className="flex flex-wrap gap-3">
        <label className="text-xs text-body/70">Product
          <select aria-label="Product rating" value={rating} onChange={(e) => setRating(e.target.value)} className={selectClass}>
            {[5, 4, 3, 2, 1].map((n) => <option key={n} value={n}>{n} star{n > 1 ? 's' : ''}</option>)}
          </select>
        </label>
        <label className="text-xs text-body/70">Producer
          <select aria-label="Producer rating" value={producerRating} onChange={(e) => setProducerRating(e.target.value)} className={selectClass}>
            {[5, 4, 3, 2, 1].map((n) => <option key={n} value={n}>{n} star{n > 1 ? 's' : ''}</option>)}
          </select>
        </label>
      </div>
      <textarea aria-label="Review" required rows={3} maxLength={2000} placeholder="Share your experience" value={comment} onChange={(e) => setComment(e.target.value)} className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm" />
      <div className="flex flex-wrap items-center gap-2">
        <Button type="submit" variant="primary" size="sm" disabled={create.isPending || !comment.trim()}>{create.isPending ? 'Posting…' : 'Post review'}</Button>
        <Button type="button" variant="secondary" size="sm" onClick={() => setOpen(false)}>Cancel</Button>
      </div>
      <MutationFeedback mutation={create} successMessage="Thanks for rating!" />
    </form>
  );
}
