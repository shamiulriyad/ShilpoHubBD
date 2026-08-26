export default function CategoryFilter({ options = [], active, onChange, className = '' }) {
  return (
    <div className={`flex flex-wrap gap-2 ${className}`}>
      {options.map((option) => {
        const isObject = typeof option === 'object' && option !== null;
        const value = isObject ? option.id : option;
        const label = isObject ? option.name : option;

        return (
          <button
            key={value ?? 'all'}
            type="button"
            onClick={() => onChange?.(value)}
            className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${
              active === value
                ? 'border-primary bg-primary text-surface'
                : 'border-border bg-surface text-body hover:bg-background'
            }`}
          >
            {label}
          </button>
        );
      })}
    </div>
  );
}
