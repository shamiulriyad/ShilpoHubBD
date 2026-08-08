import { useState } from 'react';

export default function WishlistButton({ active, onChange, className = '' }) {
  const [internalActive, setInternalActive] = useState(false);
  const isActive = active != null ? active : internalActive;

  return (
    <button
      type="button"
      aria-label={isActive ? 'Remove from wishlist' : 'Add to wishlist'}
      aria-pressed={isActive}
      onClick={(event) => {
        event.preventDefault();
        event.stopPropagation();
        if (onChange) {
          onChange(!isActive);
        } else {
          setInternalActive((prev) => !prev);
        }
      }}
      className={`flex h-8 w-8 items-center justify-center rounded-full border text-sm transition ${
        isActive ? 'border-primary bg-primary text-surface' : 'border-border bg-surface text-body hover:text-primary'
      } ${className}`}
    >
      {isActive ? '♥' : '♡'}
    </button>
  );
}
