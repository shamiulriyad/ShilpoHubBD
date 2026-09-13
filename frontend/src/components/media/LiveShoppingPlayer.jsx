import ProductCard from '../cards/ProductCard';
import Button from '../ui/Button';

export default function LiveShoppingPlayer({ event, products = [], getProductLink, onBuy, isBuying = false }) {
  const isLive = event.status === 'live';

  return (
    <div className="space-y-4">
      <div className="flex min-h-52 flex-col justify-end rounded-2xl border border-border bg-background p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-primary">
          {isLive ? 'Live commerce active' : event.status === 'ended' ? 'Event ended' : 'Scheduled event'}
        </p>
        <h2 className="mt-2 text-xl font-semibold text-heading">{event.title}</h2>
        <p className="mt-2 max-w-2xl text-sm leading-6 text-body/65">
          {isLive
            ? 'Chat and live-event purchasing are active for this event.'
            : event.status === 'ended'
              ? 'Browse the featured product and event discussion from this completed event.'
              : 'This event has not started yet. Live chat and event purchasing will unlock when the producer starts it.'}
        </p>
      </div>

      <p className="text-sm font-semibold text-heading">Featured product</p>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {products.map((product) => (
          <div key={product.id} className="rounded-xl border border-border bg-surface p-3">
            <ProductCard product={product} to={getProductLink?.(product)} />
            {isLive && onBuy && (
              <Button
                type="button"
                variant="primary"
                className="mt-3 w-full"
                disabled={isBuying}
                onClick={() => onBuy(product)}
              >
                {isBuying ? 'Adding…' : 'Add to cart from live event'}
              </Button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}