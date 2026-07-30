export default function SearchBar({ placeholder = 'Search…', size = 'md', className = '' }) {
  const sizes = {
    md: 'py-2.5 text-sm',
    lg: 'py-3.5 text-base',
  };

  return (
    <form
      role="search"
      onSubmit={(event) => event.preventDefault()}
      className={`flex w-full items-center gap-2 rounded-full border border-border bg-surface px-4 shadow-sm ${sizes[size] || sizes.md} ${className}`}
    >
      <span aria-hidden="true" className="text-body/50">⌕</span>
      <input
        type="search"
        placeholder={placeholder}
        className="w-full bg-transparent text-body outline-none placeholder:text-body/50"
      />
      <button
        type="submit"
        className="shrink-0 rounded-full bg-primary px-4 py-1.5 text-xs font-medium text-surface hover:bg-primary/90"
      >
        Search
      </button>
    </form>
  );
}
