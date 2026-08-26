import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge } from '../../components/ui';
import { useVerifyTrainingCertificate } from '../../hooks/useTrainingCertificates';

export default function Certifications() {
  const [certificateNumber, setCertificateNumber] = useState('');
  const verify = useVerifyTrainingCertificate();

  const handleSubmit = (event) => {
    event.preventDefault();
    verify.mutate(certificateNumber);
  };

  const result = verify.data;

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Academy', path: routePaths.academy },
          { label: 'Verify a Certificate' },
        ]}
        title="Verify a Certificate"
        description="Check the authenticity of any ShilpoHub Academy training certificate."
      />

      <form onSubmit={handleSubmit} className="mb-6 flex gap-2">
        <input
          required
          placeholder="Enter certificate number"
          value={certificateNumber}
          onChange={(event) => setCertificateNumber(event.target.value)}
          className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <Button type="submit" variant="primary" disabled={verify.isPending}>
          {verify.isPending ? 'Verifying…' : 'Verify'}
        </Button>
      </form>

      {result && (
        <div className="rounded-xl border border-border bg-surface p-6 text-center">
          <Badge tone={result.isValid ? 'success' : 'neutral'}>{result.isValid ? 'Valid Certificate' : 'Not Valid'}</Badge>
          {result.isValid && (
            <div className="mt-4 space-y-1 text-sm text-body/70">
              <p className="text-base font-semibold text-heading">{result.courseTitle}</p>
              <p>Awarded to {result.apprenticeName}</p>
              <p>Mentored by {result.mentorName}</p>
              <p className="text-xs text-body/50">Issued {new Date(result.issuedAt).toLocaleDateString()}</p>
            </div>
          )}
          <p className="mt-3 text-sm text-body/60">{result.message}</p>
        </div>
      )}
    </div>
  );
}
