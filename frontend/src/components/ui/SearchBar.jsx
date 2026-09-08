export default function SearchBar({ placeholder = 'Search…', size = 'md', className = '', value, onChange, onSubmit }) {
  const sizes = {
    md: 'py-2.5 text-sm',
    lg: 'py-3.5 text-base',
  };

  return (
    <form
      role="search"
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit?.(value);
      }}
      className={`flex w-full items-center gap-3 rounded-full border border-border bg-surface px-5 shadow-[0_12px_32px_rgba(23,59,53,0.09)] transition focus-within:border-primary/50 focus-within:ring-4 focus-within:ring-primary/10 ${sizes[size] || sizes.md} ${className}`}
    >
      <span aria-hidden="true" className="text-lg text-primary">⌕</span>
      <input
        type="search"
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        className="w-full bg-transparent text-body outline-none placeholder:text-body/50"
      />
      <button
        type="submit"
        className="shrink-0 rounded-full bg-title px-4 py-2 text-xs font-semibold text-surface transition hover:bg-primary"
      >
        Search
      </button>
    </form>
  );
}