import { Link } from 'react-router-dom';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useMyExpertise } from '../../hooks/useExpertiseCertificates';
import { routePaths } from '../../routes/routePaths';

const LEVEL_TONE = { Bronze: 'secondary', Silver: 'primary', Gold: 'success' };
const LEVEL_STYLE = {
  Bronze: 'border-amber-200 bg-amber-50 text-amber-800',
  Silver: 'border-slate-200 bg-slate-50 text-slate-700',
  Gold: 'border-yellow-200 bg-yellow-50 text-yellow-800',
};

function Icon({ type, className = 'h-5 w-5' }) {
  const paths = {
    star: <path d="m12 3 2.7 5.5 6.1.9-4.4 4.3 1 6.1-5.4-2.9-5.4 2.9 1-6.1-4.4-4.3 6.1-.9L12 3Z" />,
    users: <><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><circle cx="9" cy="7" r="4" /><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8" /></>,
    award: <><circle cx="12" cy="8" r="5" /><path d="m8.5 12-1 9 4.5-2.5 4.5 2.5-1-9" /></>,
    certificate: <><path d="M6 3h12a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Z" /><path d="M8 8h8M8 12h5M15.5 16.5l1 1 2-2" /></>,
    arrow: <><path d="M5 12h14" /><path d="m13 6 6 6-6 6" /></>,
  };
  return <svg viewBox="0 0 24 24" className={className} fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">{paths[type]}</svg>;
}

function StatCard({ icon, label, value, detail }) {
  return (
    <div className="group rounded-2xl border border-border bg-surface p-5 shadow-sm transition hover:-translate-y-0.5 hover:border-primary/20 hover:shadow-md">
      <div className="mb-5 flex items-center justify-between">
        <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-soft text-primary"><Icon type={icon} /></span>
        <span className="h-2 w-2 rounded-full bg-primary/20" />
      </div>
      <p className="text-2xl font-bold tracking-tight text-heading">{value}</p>
      <p className="mt-1 text-sm font-medium text-body/70">{label}</p>
      {detail && <p className="mt-2 text-xs text-body/50">{detail}</p>}
    </div>
  );
}

export default function ProducerExpertise() {
  const { data, isLoading, isError, error } = useMyExpertise();
  return (
    <div>
      <PageHeader title="Expertise Certificates" description="Build trust with customers, track your expertise level, and manage your earned certificates." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        {data && (
          <div className="space-y-6 pb-8">
            {!data.profileApproved && (
              <div role="status" className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-secondary/30 bg-secondary/10 px-4 py-3 text-sm text-heading">
                <span>Your profile must be approved before a certificate can be issued.</span>
                <Link to={routePaths.dashboardProfile} className="inline-flex items-center gap-2 font-semibold text-primary hover:underline">Complete profile <Icon type="arrow" className="h-4 w-4" /></Link>
              </div>
            )}

            <section aria-label="Expertise overview" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              <StatCard icon="star" label="Average rating" value={Number(data.averageRating).toFixed(2)} detail="Across all verified reviews" />
              <StatCard icon="users" label="Customer ratings" value={data.ratingCount} detail="Ratings received to date" />
              <StatCard icon="award" label="Level earned" value={data.earnedLevel || 'Not yet'} detail={data.nextLevel ? `${data.nextLevel} is your next milestone` : 'Highest level reached'} />
              <StatCard icon="certificate" label="Certificate issued" value={data.highestIssuedLevel || 'Not yet'} detail={data.certificates.length ? `${data.certificates.length} certificate${data.certificates.length === 1 ? '' : 's'} in your profile` : 'Complete a level to qualify'} />
            </section>

            {data.awaitingAdmin && (
              <div role="status" className="flex items-center gap-3 rounded-xl border border-success/25 bg-success/10 px-4 py-3 text-sm text-heading">
                <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-success text-white">✓</span>
                <span><strong>Congratulations!</strong> You reached {data.earnedLevel}. Your certificate is awaiting admin approval.</span>
              </div>
            )}

            <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
              <section className="overflow-hidden rounded-2xl border border-border bg-surface shadow-sm">
                <div className="border-b border-border px-5 py-5 sm:px-6">
                  <p className="text-base font-semibold text-heading">Your path to recognition</p>
                  <p className="mt-1 text-sm text-body/60">Every customer rating brings you closer to the next level.</p>
                </div>
                <div className="space-y-3 p-5 sm:p-6">
                  {data.rules.map((rule, index) => {
                    const earnedIndex = data.rules.findIndex((item) => item.level === data.earnedLevel);
                    const reached = earnedIndex >= index && earnedIndex !== -1;
                    return (
                      <div key={rule.level} className={`flex items-center gap-4 rounded-xl border p-4 ${reached ? 'border-primary/20 bg-primary-soft/50' : 'border-border bg-background/50'}`}>
                        <span className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-full border text-sm font-bold ${LEVEL_STYLE[rule.level] || 'border-border bg-surface text-heading'}`}>{index + 1}</span>
                        <div className="min-w-0 flex-1">
                          <div className="flex flex-wrap items-center justify-between gap-2">
                            <p className="font-semibold text-heading">{rule.level}</p>
                            {reached && <span className="rounded-full bg-primary px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-white">Reached</span>}
                          </div>
                          <p className="mt-1 text-sm text-body/65">At least <strong className="font-semibold text-heading">{rule.minRatings} ratings</strong> with a <strong className="font-semibold text-heading">{rule.minAverage}+ average</strong></p>
                        </div>
                      </div>
                    );
                  })}
                  {data.nextLevel && (
                    <div className="mt-4 flex items-center gap-3 rounded-xl bg-primary px-4 py-3 text-white">
                      <Icon type="award" className="h-6 w-6 shrink-0" />
                      <div><p className="text-xs font-medium uppercase tracking-wider text-white/70">Next milestone</p><p className="font-semibold">Keep going toward {data.nextLevel}</p></div>
                    </div>
                  )}
                </div>
              </section>

              <section className="rounded-2xl border border-border bg-surface p-5 shadow-sm sm:p-6">
                <div className="flex items-center gap-3">
                  <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary-soft text-primary"><Icon type="certificate" /></span>
                  <div><p className="font-semibold text-heading">Your certificates</p><p className="text-sm text-body/60">Verified proof of your expertise</p></div>
                </div>
                <div className="mt-5 space-y-3">
                  {data.certificates.map((certificate) => (
                    <div key={certificate.id} className="rounded-xl border border-border bg-background/50 p-4">
                      <div className="flex flex-wrap items-start justify-between gap-3">
                        <div><p className="text-sm font-semibold text-heading">{certificate.expertise}</p><p className="mt-0.5 text-xs text-body/55">Certificate #{certificate.certificateNumber}</p></div>
                        <Badge tone={LEVEL_TONE[certificate.level] || 'neutral'}>{certificate.level}{certificate.isRevoked ? ' (revoked)' : ''}</Badge>
                      </div>
                      <div className="mt-4 flex flex-wrap gap-x-5 gap-y-1 border-t border-border pt-3 text-xs text-body/60">
                        <span>Issued {new Date(certificate.issuedAt).toLocaleDateString()}</span><span>{Number(certificate.averageRating).toFixed(2)} average · {certificate.ratingCount} ratings</span>
                      </div>
                    </div>
                  ))}
                  {data.certificates.length === 0 && (
                    <div className="flex min-h-64 flex-col items-center justify-center rounded-xl border border-dashed border-border bg-background/40 px-6 py-10 text-center">
                      <span className="flex h-14 w-14 items-center justify-center rounded-full bg-primary-soft text-primary"><Icon type="award" className="h-7 w-7" /></span>
                      <p className="mt-4 font-semibold text-heading">Your first certificate awaits</p>
                      <p className="mt-1 max-w-xs text-sm leading-6 text-body/60">Earn enough customer ratings at the required average, then an approved certificate will appear here.</p>
                    </div>
                  )}
                </div>
              </section>
            </div>
          </div>
        )}
      </AsyncState>
    </div>
  );
}
