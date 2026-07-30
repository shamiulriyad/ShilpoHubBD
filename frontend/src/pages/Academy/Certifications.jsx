import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { certifications } from '../../data/mockData';

export default function Certifications() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Academy', path: routePaths.academy },
          { label: 'Certifications' },
        ]}
        title="Certifications"
        description="Recognized certifications awarded across the ShilpoHub Academy."
      />
      <div className="grid gap-4 sm:grid-cols-3">
        {certifications.map((cert) => (
          <div key={cert.id} className="rounded-xl border border-border bg-surface p-5 text-center">
            <div className="mx-auto mb-3 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10 text-primary">
              🏅
            </div>
            <p className="text-sm font-semibold text-heading">{cert.name}</p>
            <p className="mt-1 text-xs text-body/60">{cert.issued} issued</p>
          </div>
        ))}
      </div>
    </div>
  );
}
