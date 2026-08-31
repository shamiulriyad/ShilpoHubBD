import { PageHeader, Button, Badge, AsyncState } from '../../components/ui';
import { useMyTrainingCertificates } from '../../hooks/useTrainingCertificates';

export default function Certificates() {
  const { data, isLoading, isError, error } = useMyTrainingCertificates();
  const certificates = data || [];

  return (
    <div>
      <PageHeader title="Certificates" description="Certificates you've earned through the Academy." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid gap-4 sm:grid-cols-2">
          {certificates.map((cert) => (
            <div key={cert.id} className="flex items-center justify-between rounded-xl border border-border bg-surface p-5">
              <div>
                <p className="text-sm font-semibold text-heading">{cert.courseTitle}</p>
                <p className="mt-1 text-xs text-body/60">
                  {cert.certificateNumber} · Issued {new Date(cert.issuedAt).toLocaleDateString()}
                </p>
                {cert.isRevoked && <Badge tone="neutral">Revoked</Badge>}
              </div>
              <a href={`${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'}/training-certificates/${cert.id}/download`} target="_blank" rel="noreferrer">
                <Button variant="secondary">Download</Button>
              </a>
            </div>
          ))}
          {certificates.length === 0 && (
            <p className="col-span-full text-sm text-body/60">Complete a course to earn your first certificate.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
