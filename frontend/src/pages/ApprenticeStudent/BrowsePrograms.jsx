import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { apprenticeshipProgramsService } from '../../services/apprenticeshipProgramsService';
import { useMyProgramApplications, useProgramApplicationMutations } from '../../hooks/useProgramApplications';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Submitted: 'neutral', UnderReview: 'secondary', Accepted: 'success', Rejected: 'neutral', Withdrawn: 'neutral' };

function usePublishedPrograms(params) {
  return useQuery({ queryKey: ['apprenticeship-programs', 'published', params], queryFn: () => apprenticeshipProgramsService.listPublished(params) });
}

function BrowseTab() {
  const { data, isLoading, isError, error } = usePublishedPrograms({ pageSize: 50 });
  const { apply } = useProgramApplicationMutations();
  const [message, setMessage] = useState({});

  const programs = data?.items || [];

  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      <div className="space-y-3">
        {programs.map((p) => (
          <div key={p.id} className="rounded-xl border border-border bg-surface p-4">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="text-sm font-semibold text-heading">{p.title}</p>
                <p className="text-xs text-body/60">
                  {p.type} · {p.providerName}{p.location ? ` · ${p.location}` : ''}{p.durationWeeks ? ` · ${p.durationWeeks}w` : ''}
                </p>
              </div>
              <Badge>{p.activeEnrollmentCount} enrolled</Badge>
            </div>
            <div className="mt-2 flex gap-2">
              <input placeholder="Why are you a good fit?" value={message[p.id] || ''} onChange={(e) => setMessage((m) => ({ ...m, [p.id]: e.target.value }))} className={`${inputClass} flex-1`} />
              <Button size="sm" variant="primary" disabled={apply.isPending} onClick={() => apply.mutate({ programId: p.id, message: message[p.id] || '' })}>
                Apply
              </Button>
            </div>
          </div>
        ))}
        {programs.length === 0 && <p className="text-sm text-body/60">No published programs right now.</p>}
      </div>
    </AsyncState>
  );
}

function MyApplicationsTab() {
  const { data, isLoading, isError, error } = useMyProgramApplications();
  const { withdraw } = useProgramApplicationMutations();

  const applications = data || [];

  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      <div className="space-y-2">
        {applications.map((a) => (
          <div key={a.id} className="flex items-center justify-between rounded-xl border border-border bg-surface p-4">
            <p className="text-sm font-semibold text-heading">{a.programTitle}</p>
            <div className="flex items-center gap-2">
              <Badge tone={statusTone[a.status] || 'neutral'}>{a.status}</Badge>
              {a.status === 'Submitted' && (
                <button type="button" onClick={() => withdraw.mutate(a.id)} className="text-xs text-danger hover:underline">Withdraw</button>
              )}
            </div>
          </div>
        ))}
        {applications.length === 0 && <p className="text-sm text-body/60">You haven't applied to any programs yet.</p>}
      </div>
    </AsyncState>
  );
}

export default function BrowsePrograms() {
  const [tab, setTab] = useState('browse');

  return (
    <div>
      <PageHeader title="Apprenticeship Programs" description="Browse published programs and track your applications." />

      <div className="mb-4 flex gap-2 border-b border-border">
        <button type="button" onClick={() => setTab('browse')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'browse' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Browse</button>
        <button type="button" onClick={() => setTab('applications')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'applications' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>My Applications</button>
      </div>

      {tab === 'browse' ? <BrowseTab /> : <MyApplicationsTab />}
    </div>
  );
}
