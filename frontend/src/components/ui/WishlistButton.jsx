export default function WishlistButton({ active = false, onChange, className = '' }) {
  if (!onChange) return null;

  return (
    <button
      type="button"
      aria-label={active ? 'Remove from wishlist' : 'Add to wishlist'}
      aria-pressed={active}
      onClick={(event) => {
        event.preventDefault();
        event.stopPropagation();
        onChange(!active);
      }}
      className={`flex h-8 w-8 items-center justify-center rounded-full border text-sm transition ${
        active ? 'border-primary bg-primary text-surface' : 'border-border bg-surface text-body hover:text-primary'
      } ${className}`}
    >
      {active ? '♥' : '♡'}
    </button>
  );
}
