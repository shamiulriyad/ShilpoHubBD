import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
export default function TravelEmptyState({ title, description, onReset }) {
  return <div className="col-span-full rounded-2xl border border-border bg-surface px-6 py-12 text-center">
    <svg viewBox="0 0 24 24" className="mx-auto mb-4 h-10 w-10 text-primary" fill="none" stroke="currentColor" strokeWidth="1.3" aria-hidden="true"><path d="m3 5 6-2 6 2 6-2v16l-6 2-6-2-6 2ZM9 3v16m6-14v16" /></svg>
    <h2 className="text-lg font-semibold text-heading">{title}</h2><p className="mx-auto mt-2 max-w-md text-sm leading-6 text-body/70">{description}</p>
    {onReset ? <button type="button" onClick={onReset} className="mt-6 rounded-lg border border-border px-5 py-2.5 text-sm font-semibold text-heading hover:bg-background">Clear filters</button> : <Link to={routePaths.tourismMap} className="mt-6 inline-flex rounded-lg bg-title px-5 py-2.5 text-sm font-semibold text-white">Explore heritage places →</Link>}
  </div>;
}
