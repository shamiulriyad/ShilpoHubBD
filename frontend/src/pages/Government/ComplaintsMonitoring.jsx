import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useComplaints, useMonitoringFlags, useQrMonitoringOverview, useComplaintsMonitoringMutations } from '../../hooks/useComplaintsMonitoring';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const complaintCategories = ['ProductQuality', 'Fraud', 'Counterfeit', 'Delivery', 'Payment', 'Conduct', 'HeritageMisrepresentation', 'Other'];
const priorities = ['Low', 'Medium', 'High', 'Urgent'];
const complaintStatusTone = { Open: 'neutral', InProgress: 'secondary', Resolved: 'success', Rejected: 'neutral' };
const flagSeverityTone = { Info: 'neutral', Low: 'neutral', Medium: 'secondary', High: 'primary', Critical: 'neutral' };
const flagStatuses = ['UnderReview', 'Confirmed', 'Dismissed', 'Resolved'];

function ComplaintsTab() {
  const { data, isLoading, isError, error } = useComplaints({ pageSize: 50 });
  const { createComplaint, resolveComplaint } = useComplaintsMonitoringMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ category: 'ProductQuality', priority: 'Medium', title: '', description: '' });

  const complaints = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    createComplaint.mutate(form, { onSuccess: () => { setShowForm(false); setForm({ category: 'ProductQuality', priority: 'Medium', title: '', description: '' }); } });
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'Log Complaint'}</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select value={form.category} onChange={(e) => setForm((p) => ({ ...p, category: e.target.value }))} className={inputClass}>
            {complaintCategories.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
          <select value={form.priority} onChange={(e) => setForm((p) => ({ ...p, priority: e.target.value }))} className={inputClass}>
            {priorities.map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createComplaint.isPending}>Log Complaint</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {complaints.map((c) => (
            <div key={c.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{c.title} <span className="font-normal text-body/50">({c.referenceCode})</span></p>
                <p className="text-xs text-body/60">{c.category} · {c.priority}{c.assignedToName ? ` · ${c.assignedToName}` : ''}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={complaintStatusTone[c.status] || 'neutral'}>{c.status}</Badge>
                {!['Resolved', 'Rejected'].includes(c.status) && (
                  <button
                    type="button"
                    onClick={() => resolveComplaint.mutate({ id: c.id, payload: { resolution: 'Reviewed and closed.', outcome: 'Resolved' } })}
                    className="text-xs text-primary hover:underline"
                  >
                    Resolve
                  </button>
                )}
              </div>
            </div>
          ))}
          {complaints.length === 0 && <p className="text-sm text-body/60">No complaints logged yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function MonitoringTab() {
  const { data, isLoading, isError, error } = useMonitoringFlags({ pageSize: 50 });
  const qrQuery = useQrMonitoringOverview({});
  const { runMonitoringScan, updateMonitoringFlagStatus } = useComplaintsMonitoringMutations();

  const flags = data?.items || [];
  const qr = qrQuery.data;

  return (
    <div>
      {qr && (
        <div className="mb-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
          <div className="rounded-lg border border-border bg-surface p-3 text-center">
            <p className="text-lg font-semibold text-primary">{qr.totalScans}</p>
            <p className="text-xs text-body/60">QR scans</p>
          </div>
          <div className="rounded-lg border border-border bg-surface p-3 text-center">
            <p className="text-lg font-semibold text-primary">{qr.invalidScanRatePercent.toFixed(1)}%</p>
            <p className="text-xs text-body/60">Invalid rate</p>
          </div>
          <div className="rounded-lg border border-border bg-surface p-3 text-center">
            <p className="text-lg font-semibold text-primary">{qr.activeCodes}</p>
            <p className="text-xs text-body/60">Active codes</p>
          </div>
          <div className="rounded-lg border border-border bg-surface p-3 text-center">
            <p className="text-lg font-semibold text-primary">{qr.anomalousProducts.length}</p>
            <p className="text-xs text-body/60">Anomalous products</p>
          </div>
        </div>
      )}

      <div className="mb-4 flex justify-end">
        <Button variant="primary" disabled={runMonitoringScan.isPending} onClick={() => runMonitoringScan.mutate({ scanType: 'All' })}>
          {runMonitoringScan.isPending ? 'Scanning…' : 'Run Fraud/Anomaly Scan'}
        </Button>
      </div>

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {flags.map((f) => (
            <div key={f.id} className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-border bg-surface p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{f.title}</p>
                <p className="text-xs text-body/60">{f.flagType} · {f.subjectLabel} · risk {f.riskScore}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={flagSeverityTone[f.severity] || 'neutral'}>{f.severity}</Badge>
                {f.status === 'Open' && (
                  <select
                    onChange={(e) => e.target.value && updateMonitoringFlagStatus.mutate({ id: f.id, payload: { status: e.target.value } })}
                    className={inputClass}
                    defaultValue=""
                  >
                    <option value="" disabled>Update status…</option>
                    {flagStatuses.map((s) => <option key={s} value={s}>{s}</option>)}
                  </select>
                )}
                {f.status !== 'Open' && <Badge>{f.status}</Badge>}
              </div>
            </div>
          ))}
          {flags.length === 0 && <p className="text-sm text-body/60">No monitoring flags yet — run a scan above.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

export default function ComplaintsMonitoring() {
  const [tab, setTab] = useState('complaints');

  return (
    <div>
      <PageHeader title="Complaints & Monitoring" description="Handle marketplace complaints and review AI-detected fraud/anomaly flags." />

      <div className="mb-4 flex gap-2 border-b border-border">
        <button type="button" onClick={() => setTab('complaints')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'complaints' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Complaints</button>
        <button type="button" onClick={() => setTab('monitoring')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'monitoring' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>Monitoring</button>
      </div>

      {tab === 'complaints' ? <ComplaintsTab /> : <MonitoringTab />}
    </div>
  );
}
