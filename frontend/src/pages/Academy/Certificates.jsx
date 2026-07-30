import { certifications } from '../../data/mockData';
import { PageHeader, Button } from '../../components/ui';

export default function Certificates() {
  return (
    <div>
      <PageHeader title="Certificates" description="Certificates you've earned through the Academy." />
      <div className="grid gap-4 sm:grid-cols-2">
        {certifications.map((cert) => (
          <div key={cert.id} className="flex items-center justify-between rounded-xl border border-border bg-surface p-5">
            <div>
              <p className="text-sm font-semibold text-heading">{cert.name}</p>
              <p className="mt-1 text-xs text-body/60">Issued to you</p>
            </div>
            <Button variant="secondary">Download</Button>
          </div>
        ))}
      </div>
    </div>
  );
}
