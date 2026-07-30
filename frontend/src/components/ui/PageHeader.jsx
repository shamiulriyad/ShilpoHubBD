import Breadcrumbs from '../layout/Breadcrumbs';

export default function PageHeader({ breadcrumbs, title, description, action }) {
  return (
    <div className="mb-8">
      {breadcrumbs && <Breadcrumbs items={breadcrumbs} />}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-heading sm:text-3xl">{title}</h1>
          {description && <p className="mt-2 max-w-2xl text-sm text-body/80">{description}</p>}
        </div>
        {action && <div className="shrink-0">{action}</div>}
      </div>
    </div>
  );
}
