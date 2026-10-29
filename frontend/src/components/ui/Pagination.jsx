export default function Pagination({ currentPage = 1, totalPages = 5, onPageChange }) {
  const count = Math.max(1, Number(totalPages) || 1);
  const current = Math.min(count, Math.max(1, Number(currentPage) || 1));
  const visible = new Set([1, count, current - 1, current, current + 1]);
  const numbers = [...visible].filter(page => page >= 1 && page <= count).sort((a, b) => a - b);
  const pages = numbers.flatMap((page, index) => index && page - numbers[index - 1] > 1 ? [`gap-${page}`, page] : [page]);

  return (
    <nav aria-label="Pagination" className="flex flex-wrap items-center justify-center gap-1.5">
      <button
        type="button"
        disabled={current === 1}
        onClick={() => onPageChange?.(current - 1)}
        className="rounded-md border border-border px-3 py-1.5 text-sm text-body disabled:opacity-40"
      >
        Previous
      </button>
      {pages.map((page) => typeof page === 'string' ? <span key={page} aria-hidden="true" className="px-1 text-muted">…</span> : (
        <button
          type="button"
          key={page}
          aria-label={`Page ${page}`}
          aria-current={page === current ? 'page' : undefined}
          onClick={() => onPageChange?.(page)}
          className={`h-8 w-8 rounded-md text-sm ${
            page === current ? 'bg-primary text-surface' : 'border border-border text-body hover:bg-background'
          }`}
        >
          {page}
        </button>
      ))}
      <button
        type="button"
        disabled={current === count}
        onClick={() => onPageChange?.(current + 1)}
        className="rounded-md border border-border px-3 py-1.5 text-sm text-body disabled:opacity-40"
      >
        Next
      </button>
    </nav>
  );
}
