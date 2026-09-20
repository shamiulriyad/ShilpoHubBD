import { useState } from 'react';
import { flagViews } from './adminConfig';
import { Action, DataTable, Editor, ErrorNotice, Modal, Panel, RecordDetails, useAdminAction, useAdminQuery, inputClass } from './AdminUI';
const base = '/governance/monitoring';
export default function AdminModeration({
  view
}) {
  const config = flagViews[view];
  const [page, setPage] = useState(1),
    [status, setStatus] = useState(''),
    [severity, setSeverity] = useState(''),
    [selected, setSelected] = useState(null),
    [mode, setMode] = useState(null);
  const query = useAdminQuery(`${base}/flags`, {
    page,
    pageSize: 20,
    flagType: config.flagType,
    status: status || undefined,
    severity: severity || undefined
  });
  const detail = useAdminQuery(`${base}/flags/${selected?.id}`, {}, Boolean(selected));
  const mutation = useAdminAction();
  const [scanResult, setScanResult] = useState(null);
  const open = mode => {
    mutation.reset();
    setMode(mode);
  };
  const fields = mode === 'scan' ? [{
    key: 'since',
    label: 'Activity since',
    type: 'datetime-local'
  }, {
    key: 'minRiskScore',
    label: 'Minimum risk score (0–100)',
    type: 'number',
    min: 0,
    max: 100,
    default: 40
  }] : mode === 'status' ? [{
    key: 'status',
    label: 'Decision',
    required: true,
    options: ['Open', 'UnderReview', 'Confirmed', 'Dismissed', 'Resolved']
  }, {
    key: 'note',
    label: 'Review note',
    type: 'textarea',
    required: true,
    maxLength: 2000
  }] : mode === 'note' ? [{
    key: 'note',
    label: 'Note',
    type: 'textarea',
    required: true,
    maxLength: 2000
  }] : mode === 'assign' ? [{
    key: 'assigneeUserId',
    label: 'Reviewer user ID',
    required: true
  }, {
    key: 'note',
    label: 'Assignment note',
    type: 'textarea'
  }] : [{
    key: 'title',
    label: 'Title',
    required: true
  }, {
    key: 'description',
    label: 'Evidence / description',
    type: 'textarea',
    required: true
  }, {
    key: 'severity',
    label: 'Severity',
    required: true,
    options: ['Info', 'Low', 'Medium', 'High', 'Critical'],
    default: 'Medium'
  }, {
    key: 'subjectType',
    label: 'Subject type',
    required: true,
    options: ['Producer', 'Product', 'Order', 'Payment', 'QrCode', 'Review', 'Village', 'District', 'BlogPost', 'Other'],
    default: 'Other'
  }, {
    key: 'subjectId',
    label: 'Subject ID (optional)'
  }, {
    key: 'subjectLabel',
    label: 'Subject name',
    required: true
  }, {
    key: 'riskScore',
    label: 'Risk score',
    type: 'number',
    min: 0,
    max: 100
  }];
  const submit = body => {
    const path = mode === 'scan' ? `${base}/scans` : mode === 'create' ? `${base}/flags` : `${base}/flags/${selected.id}/${mode === 'note' ? 'notes' : mode}`;
    mutation.mutate({
      path,
      payload: {
        ...body,
        ...(mode === 'scan' ? {
          scanType: config.scanType
        } : mode === 'create' ? {
          flagType: config.flagType
        } : {})
      }
    }, {
      onSuccess: data => {
        if (mode === 'scan') setScanResult(data);
        setMode(null);
      }
    });
  };
  return <Panel><p className="mb-5 text-sm text-body/70">Review automated findings and record a human decision. Scans use the server’s risk rules; findings are signals for investigation.</p><div className="mb-5 flex flex-wrap gap-3"><select aria-label="Flag status" className={`${inputClass} max-w-48`} value={status} onChange={e => {
        setStatus(e.target.value);
        setPage(1);
      }}><option value="">All statuses</option>{['Open', 'UnderReview', 'Confirmed', 'Dismissed', 'Resolved'].map(x => <option key={x}>{x}</option>)}</select><select aria-label="Flag severity" className={`${inputClass} max-w-48`} value={severity} onChange={e => {
        setSeverity(e.target.value);
        setPage(1);
      }}><option value="">All severities</option>{['Info', 'Low', 'Medium', 'High', 'Critical'].map(x => <option key={x}>{x}</option>)}</select><Action onClick={() => open('scan')}>Run scan</Action><Action onClick={() => open('create')}>Report a finding</Action></div>
    {scanResult && <p role="status" className="mb-5 rounded-lg bg-primary/5 p-4 text-sm">Scan completed: {scanResult.candidatesEvaluated} candidates evaluated, {scanResult.flagsCreated} flags created, {scanResult.duplicatesSkipped} duplicates skipped.</p>}
    <DataTable query={query} columns={['title', 'subjectLabel', 'severity', 'status', 'riskScore', 'assignedToName']} page={page} onPage={setPage} actions={row => <Action onClick={() => setSelected(row)}>Investigate</Action>} />
    {selected && <Modal title="Review finding" onClose={() => setSelected(null)}><ErrorNotice error={detail.error} />{detail.isPending ? <p>Loading evidence…</p> : detail.isSuccess && <><RecordDetails record={detail.data} />{detail.data.evidenceJson && <details className="mt-4"><summary className="cursor-pointer font-medium">Source evidence</summary><pre className="mt-2 max-h-60 overflow-auto whitespace-pre-wrap break-words rounded-lg bg-background p-3 text-xs">{detail.data.evidenceJson}</pre></details>}<div className="mt-5 flex flex-wrap gap-3"><Action onClick={() => open('status')}>Record decision</Action><Action onClick={() => open('note')}>Add note</Action><Action onClick={() => open('assign')}>Assign reviewer</Action></div></>}</Modal>}
    {mode && <Modal title={mode === 'scan' ? 'Run moderation scan' : mode === 'create' ? 'Report a finding' : 'Update investigation'} onClose={() => !mutation.isPending && setMode(null)}><Editor fields={fields} onSubmit={submit} pending={mutation.isPending} error={mutation.error} submitLabel={mode === 'scan' ? 'Run scan' : 'Confirm'} /></Modal>}
  </Panel>;
}
