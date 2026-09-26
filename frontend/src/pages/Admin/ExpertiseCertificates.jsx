import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { useEligibleProducers, useIssueExpertiseCertificate } from '../../hooks/useExpertiseCertificates';

export default function AdminExpertiseCertificates() {
  const { data, isLoading, isError, error } = useEligibleProducers();
  const issue = useIssueExpertiseCertificate();
  const producers = data || [];
  return (
    <div>
      <PageHeader
        title="Expertise Certificates"
        description="Producers whose customer ratings earned a new expertise level. Issue their certificate."
      />
      <MutationFeedback mutation={issue} successMessage="Certificate issued. The producer has been notified." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {producers.map((p) => (
            <article key={p.producerId} className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{p.producerName} <span className="font-normal text-body/60">· {p.expertise || 'no expertise on profile'}</span></p>
                <p className="text-xs text-body/60">{Number(p.averageRating).toFixed(2)} average from {p.ratingCount} ratings{p.highestIssuedLevel ? ` · currently ${p.highestIssuedLevel}` : ''}</p>
                {!p.profileApproved && <p className="text-xs text-error">Profile is not approved yet - approve it first.</p>}
              </div>
              <div className="flex items-center gap-3">
                <Badge tone="success">{p.earnedLevel}</Badge>
                <Button variant="primary" disabled={issue.isPending || !p.profileApproved} onClick={() => issue.mutate(p.producerId)}>Issue {p.earnedLevel} certificate</Button>
              </div>
            </article>
          ))}
          {producers.length === 0 && <TravelEmptyState title="No one is waiting" description="Producers appear here when their ratings earn a new level." />}
        </div>
      </AsyncState>
    </div>
  );
}
