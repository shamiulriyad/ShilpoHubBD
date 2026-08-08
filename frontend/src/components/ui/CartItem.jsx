export default function CartItem({ item, onIncrement, onDecrement, onRemove }) {
  return (
    <div className="flex items-center gap-4 p-4">
      <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-lg bg-background text-[10px] text-body/40">
        Image
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-heading">{item.name}</p>
        <p className="text-xs text-body/60">{item.producer}</p>
        {onRemove && (
          <button type="button" onClick={onRemove} className="mt-1 text-xs text-link hover:underline">
            Remove
          </button>
        )}
      </div>
      <div className="flex items-center gap-2 text-sm">
        <button type="button" onClick={onDecrement} className="h-7 w-7 rounded-md border border-border">
          −
        </button>
        <span className="w-6 text-center">{item.qty}</span>
        <button type="button" onClick={onIncrement} className="h-7 w-7 rounded-md border border-border">
          +
        </button>
      </div>
      <p className="w-24 shrink-0 text-right text-sm font-semibold text-primary">
        ৳ {(item.price * item.qty).toLocaleString()}
      </p>
    </div>
  );
}
