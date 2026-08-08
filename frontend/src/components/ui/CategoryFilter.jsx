export default function CategoryFilter({ options = [], active, onChange, className = '' }) {
  return (
    <div className={`flex flex-wrap gap-2 ${className}`}>
      {options.map((option) => (
        <button
          key={option}
          type="button"
          onClick={() => onChange?.(option)}
          className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${
            active === option
              ? 'border-primary bg-primary text-surface'
              : 'border-border bg-surface text-body hover:bg-background'
          }`}
        >
          {option}
        </button>
      ))}
    </div>
  );
}
