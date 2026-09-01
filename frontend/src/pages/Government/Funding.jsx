import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useFundingPrograms, useFundingApplications, useFundingApplication, useFundingMutations } from '../../hooks/useFunding';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const programTypes = ['Grant', 'Loan', 'Scholarship', 'EquipmentSupport', 'VillageSponsorship', 'ProducerSponsorship'];
const programStatusTone = { Draft: 'neutral', Open: 'success', Closed: 'neutral', Archived: 'neutral' };
const applicationStatusTone = { Submitted: 'neutral', UnderReview: 'secondary', Approved: 'success', Rejected: 'neutral', Withdrawn: 'neutral' };

function ProgramsTab() {
  const { data, isLoading, isError, error } = useFundingPrograms({ pageSize: 50 });
  const { createProgram, updateProgram } = useFundingMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ name: '', type: 'Grant', description: '', totalBudget: '' });

  const programs = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    createProgram.mutate(
      { ...form, totalBudget: Number(form.totalBudget) || 0 },
      { onSuccess: () => { setShowForm(false); setForm({ name: '', type: 'Grant', description: '', totalBudget: '' }); } },
    );
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Program'}</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Program name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select value={form.type} onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))} className={inputClass}>
            {programTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input required type="number" min="0" placeholder="Total budget (৳)" value={form.totalBudget} onChange={(e) => setForm((p) => ({ ...p, totalBudget: e.target.value }))} className={inputClass} />
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createProgram.isPending}>Create Program</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {programs.map((p) => (
            <div key={p.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{p.name}</p>
                <p className="text-xs text-body/60">
                  {p.type} · ৳{p.allocatedAmount.toLocaleString()}/{p.totalBudget.toLocaleString()} allocated · {p.applicationCount} application(s)
                </p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={programStatusTone[p.status] || 'neutral'}>{p.status}</Badge>
                {p.status === 'Draft' && (
                  <button type="button" onClick={() => updateProgram.mutate({ id: p.id, payload: { status: 'Open' } })} className="text-xs text-primary hover:underline">Open</button>
                )}
                {p.status === 'Open' && (
                  <button type="button" onClick={() => updateProgram.mutate({ id: p.id, payload: { status: 'Closed' } })} className="text-xs text-danger hover:underline">Close</button>
                )}
              </div>
            </div>
          ))}
          {programs.length === 0 && <p className="text-sm text-body/60">No funding programs yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function ApplicationDetail({ id }) {
  const detailQuery = useFundingApplication(id);
  const { submitReview, decideApplication, scheduleDisbursement } = useFundingMutations();
  const [approvedAmount, setApprovedAmount] = useState('');
  const [disbursement, setDisbursement] = useState({ amount: '', scheduledFor: '' });

  const app = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-2 text-xs text-body/60">Loading…</p>;
  if (!app) return null;

  return (
    <div className="mt-3 space-y-3 border-t border-border pt-3 text-xs">
      <p className="text-body/60"><strong>Purpose:</strong> {app.purpose}</p>

      {app.status === 'Submitted' && (
        <div className="flex flex-wrap gap-2">
          <Button size="sm" variant="secondary" disabled={submitReview.isPending} onClick={() => submitReview.mutate({ id, payload: { decision: 'Approve' } })}>Recommend Approve</Button>
          <Button size="sm" variant="secondary" disabled={submitReview.isPending} onClick={() => submitReview.mutate({ id, payload: { decision: 'Reject' } })}>Recommend Reject</Button>
        </div>
      )}

      {['Submitted', 'UnderReview'].includes(app.status) && (
        <div className="flex flex-wrap items-end gap-2">
          <input type="number" min="0" placeholder="Approved amount (৳)" value={approvedAmount} onChange={(e) => setApprovedAmount(e.target.value)} className={`${inputClass} w-40`} />
          <Button size="sm" variant="primary" disabled={decideApplication.isPending} onClick={() => decideApplication.mutate({ id, payload: { outcome: 'Approved', approvedAmount: Number(approvedAmount) || app.requestedAmount } })}>
            Approve
          </Button>
          <Button size="sm" variant="secondary" disabled={decideApplication.isPending} onClick={() => decideApplication.mutate({ id, payload: { outcome: 'Rejected' } })}>
            Reject
          </Button>
        </div>
      )}

      {app.status === 'Approved' && (
        <div className="flex flex-wrap items-end gap-2">
          <input type="number" min="0" placeholder="Disbursement amount" value={disbursement.amount} onChange={(e) => setDisbursement((p) => ({ ...p, amount: e.target.value }))} className={`${inputClass} w-40`} />
          <input type="date" value={disbursement.scheduledFor} onChange={(e) => setDisbursement((p) => ({ ...p, scheduledFor: e.target.value }))} className={inputClass} />
          <Button
            size="sm"
            variant="secondary"
            disabled={!disbursement.amount || !disbursement.scheduledFor || scheduleDisbursement.isPending}
            onClick={() => scheduleDisbursement.mutate({ id, payload: { amount: Number(disbursement.amount), scheduledFor: new Date(disbursement.scheduledFor).toISOString(), method: 'BankTransfer' } })}
          >
            Schedule Disbursement
          </Button>
        </div>
      )}

      <div>
        <p className="mb-1 font-medium text-heading">Disbursements ({app.disbursements.length})</p>
        {app.disbursements.map((d) => (
          <p key={d.id} className="text-body/60">৳{d.amount.toLocaleString()} · {d.method} · {d.status}</p>
        ))}
        {app.disbursements.length === 0 && <p className="text-body/50">None scheduled yet.</p>}
      </div>
    </div>
  );
}

function ApplicationsTab() {
  const { data, isLoading, isError, error } = useFundingApplications({ pageSize: 50 });
  const [expandedId, setExpandedId] = useState(null);

  const applications = data?.items || [];

  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      <div className="space-y-2">
        {applications.map((a) => (
          <div key={a.id} className="rounded-xl border border-border bg-surface p-4">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="text-sm font-semibold text-heading">{a.applicantLabel} <span className="font-normal text-body/50">({a.referenceCode})</span></p>
                <p className="text-xs text-body/60">{a.programName} · ৳{a.requestedAmount.toLocaleString()} requested</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={applicationStatusTone[a.status] || 'neutral'}>{a.status}</Badge>
                <Button variant="secondary" onClick={() => setExpandedId(expandedId === a.id ? null : a.id)}>
                  {expandedId === a.id ? 'Hide' : 'Manage'}
                </Button>
              </div>
            </div>
            {expandedId === a.id && <ApplicationDetail id={a.id} />}
          </div>
        ))}
        {applications.length === 0 && <p className="text-sm text-body/60">No funding applications yet.</p>}
      </div>
    </AsyncState>
  );
}

export default function Funding() {
  const [tab, setTab] = useState('programs');

  return (
    <div>
      <PageHeader title="Funding & Grants" description="Manage funding programs and review, approve and disburse applications." />

      <div className="mb-4 flex gap-2 border-b border-border">
        <button type="button" onClick={() => setTab('programs')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'programs' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Programs</button>
        <button type="button" onClick={() => setTab('applications')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'applications' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Applications</button>
      </div>

      {tab === 'programs' ? <ProgramsTab /> : <ApplicationsTab />}
    </div>
  );
}
