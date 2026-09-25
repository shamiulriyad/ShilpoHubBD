import { Link } from 'react-router-dom';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useMyExpertise } from '../../hooks/useExpertiseCertificates';
import { routePaths } from '../../routes/routePaths';

const LEVEL_TONE = { Bronze: 'secondary', Silver: 'primary', Gold: 'success' };

export default function ProducerExpertise() {
  const { data, isLoading, isError, error } = useMyExpertise();
  return (
    <div>
      <PageHeader
        title="Expertise Certificates"
        description="Customer ratings earn you an expertise level. When you reach one, an admin issues your certificate."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        {data && (
          <div className="space-y-6">
            {!data.profileApproved && (
              <p role="status" className="rounded-md border border-secondary/40 bg-secondary/10 px-3 py-2 text-sm">
                Your profile must be approved before a certificate can be issued. <Link to={routePaths.dashboardProfile} className="font-semibold text-primary hover:underline">Open profile</Link>
              </p>
            )}
            <div className="grid gap-3 sm:grid-cols-4">
              <div className="rounded-xl border border-border bg-surface p-4 text-center"><p className="text-2xl font-semibold text-primary">{Number(data.averageRating).toFixed(2)}</p><p className="text-xs text-body/60">Average rating</p></div>
              <div className="rounded-xl border border-border bg-surface p-4 text-center"><p className="text-2xl font-semibold text-primary">{data.ratingCount}</p><p className="text-xs text-body/60">Ratings</p></div>
              <div className="rounded-xl border border-border bg-surface p-4 text-center"><p className="text-2xl font-semibold text-primary">{data.earnedLevel || '—'}</p><p className="text-xs text-body/60">Level earned</p></div>
              <div className="rounded-xl border border-border bg-surface p-4 text-center"><p className="text-2xl font-semibold text-primary">{data.highestIssuedLevel || '—'}</p><p className="text-xs text-body/60">Certificate issued</p></div>
            </div>
            {data.awaitingAdmin && <p className="text-sm text-success">You reached {data.earnedLevel}. An admin will issue your certificate.</p>}

            <div>
              <p className="mb-2 text-sm font-semibold text-heading">How levels work</p>
              <ul className="space-y-1 text-sm text-body/80">
                {data.rules.map((r) => <li key={r.level}><span className="font-medium text-heading">{r.level}</span>: at least {r.minRatings} ratings averaging {r.minAverage} or more</li>)}
              </ul>
              {data.nextLevel && <p className="mt-2 text-xs text-body/60">Next level to reach: {data.nextLevel}</p>}
            </div>

            <div>
              <p className="mb-2 text-sm font-semibold text-heading">Your certificates</p>
              <div className="space-y-2">
                {data.certificates.map((c) => (
                  <div key={c.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
                    <div>
                      <p className="text-sm font-semibold text-heading">{c.expertise} · {c.certificateNumber}</p>
                      <p className="text-xs text-body/60">Issued {new Date(c.issuedAt).toLocaleDateString()} · {Number(c.averageRating).toFixed(2)} average from {c.ratingCount} ratings</p>
                    </div>
                    <Badge tone={LEVEL_TONE[c.level] || 'neutral'}>{c.level}{c.isRevoked ? ' (revoked)' : ''}</Badge>
                  </div>
                ))}
                {data.certificates.length === 0 && <p className="text-sm text-body/60">No certificate yet.</p>}
              </div>
            </div>
          </div>
        )}
      </AsyncState>
    </div>
  );
}
