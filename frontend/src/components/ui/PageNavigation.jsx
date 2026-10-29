import Pagination from './Pagination';

export default function PageNavigation({ data, page, onPageChange, pageSize = 10 }) {
  const count = data?.totalCount ?? 0;
  const pages = data?.totalPages ?? Math.max(1, Math.ceil(count / (data?.pageSize || pageSize)));
  if (pages <= 1) return null;
  return <div className="mt-5 flex flex-wrap items-center justify-between gap-4 border-t border-border pt-4">
    <p className="text-xs text-muted">Page {page} of {pages} · {count.toLocaleString()} records</p>
    <Pagination currentPage={page} totalPages={pages} onPageChange={onPageChange} />
  </div>;
}
