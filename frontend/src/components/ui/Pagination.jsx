export default function Pagination({ currentPage = 1, totalPages = 5 }) {
  const pages = Array.from({ length: totalPages }, (_, i) => i + 1);

  return (
    <nav aria-label="Pagination" className="flex items-center justify-center gap-1.5">
      <button
        type="button"
        disabled={currentPage === 1}
        className="rounded-md border border-border px-3 py-1.5 text-sm text-body disabled:opacity-40"
      >
        Prev
      </button>
      {pages.map((page) => (
        <button
          type="button"
          key={page}
          className={`h-8 w-8 rounded-md text-sm ${
            page === currentPage ? 'bg-primary text-surface' : 'border border-border text-body hover:bg-background'
          }`}
        >
          {page}
        </button>
      ))}
      <button
        type="button"
        disabled={currentPage === totalPages}
        className="rounded-md border border-border px-3 py-1.5 text-sm text-body disabled:opacity-40"
      >
        Next
      </button>
    </nav>
  );
}
