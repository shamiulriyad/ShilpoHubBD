import { useState } from 'react';
import { Action, DataTable, Editor, ErrorNotice, Modal, Panel, RecordDetails, useAdminAction, useAdminQuery, inputClass } from './AdminUI';
const base = '/admin/security';
export function SystemHealth() {
  const query = useAdminQuery(`${base}/system-health`);
  const data = query.data;
  return <Panel><div className="mb-5 flex items-center justify-between"><h2 className="text-lg font-semibold">System status</h2><Action disabled={query.isFetching} onClick={() => query.refetch()}>Refresh status</Action></div><ErrorNotice error={query.error} />{query.isPending ? <p>Checking system health…</p> : data && <><div className="mb-5 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{[['Database', data.databaseConnected ? 'Connected' : 'Unavailable'], ['Users', data.userCount], ['Products', data.productCount], ['Orders', data.orderCount]].map(([label, value]) => <div key={label} className="rounded-xl bg-background p-5"><p className="text-xs font-semibold uppercase tracking-wide text-body/60">{label}</p><p className="mt-2 text-2xl font-semibold">{value}</p></div>)}</div><dl className="grid gap-4 text-sm sm:grid-cols-2"><div><dt className="text-body/60">Uptime</dt><dd>{data.uptime}</dd></div><div><dt className="text-body/60">Memory in use</dt><dd>{(data.workingSetBytes / 1048576).toFixed(1)} MB</dd></div><div><dt className="text-body/60">Runtime</dt><dd>{data.runtimeVersion}</dd></div><div><dt className="text-body/60">Last checked</dt><dd>{new Date(data.generatedAt).toLocaleString()}</dd></div></dl></>}</Panel>;
}
export default function AdminSecurity({
  view
}) {
  if (view === 'health') return <SystemHealth />;
  if (view === 'threats') return <Threats />;
  return <SecurityList view={view} />;
}
function SecurityList({
  view
}) {
  const [page, setPage] = useState(1),
    [filters, setFilters] = useState({}),
    [mode, setMode] = useState(null),
    [selected, setSelected] = useState(null),
    [secret, setSecret] = useState(null);
  const path = `${base}/${view === 'audit' ? 'audit-logs' : view === 'keys' ? 'api-keys' : 'backups'}`;
  const query = useAdminQuery(path, {
    page,
    pageSize: 20,
    ...filters
  });
  const mutation = useAdminAction();
  const open = (mode, row) => {
    mutation.reset();
    setSelected(row);
    setMode(mode);
  };
  const fields = view === 'keys' && mode === 'create' ? [{
    key: 'name',
    label: 'Key name',
    required: true,
    maxLength: 200
  }, {
    key: 'expiresAt',
    label: 'Expires at',
    type: 'datetime-local'
  }] : [];
  const submit = payload => mutation.mutate({
    method: mode === 'delete' ? 'delete' : 'post',
    path: mode === 'revoke' ? `${path}/${selected.id}/revoke` : mode === 'delete' ? `${path}/${selected.id}` : path,
    payload: fields.length ? payload : undefined
  }, {
    onSuccess: data => {
      if (view === 'keys' && mode === 'create') setSecret(data);
      setMode(null);
    }
  });
  return <Panel><div className="mb-5 flex flex-wrap items-center justify-between gap-3"><p className="text-sm text-body/60">{view === 'audit' ? 'Track administrative changes and account activity.' : view === 'keys' ? 'Create named API keys and revoke access when it is no longer needed.' : 'Request a backup and inspect its completion status.'}</p>{view !== 'audit' && <Action onClick={() => open('create')}>{view === 'keys' ? 'Create API key' : 'Create backup'}</Action>}<Action onClick={() => query.refetch()} disabled={query.isFetching}>Refresh</Action></div>
    {view === 'audit' && <div className="mb-5"><Editor key="filters" fields={[{
        key: 'search',
        label: 'Search logs'
      }, {
        key: 'action',
        label: 'Action'
      }, {
        key: 'from',
        label: 'From',
        type: 'datetime-local'
      }, {
        key: 'to',
        label: 'To',
        type: 'datetime-local'
      }]} onSubmit={body => {
        setFilters(body);
        setPage(1);
      }} submitLabel="Apply filters" /></div>}
    <DataTable query={query} columns={view === 'audit' ? ['actorName', 'action', 'entityType', 'description', 'ipAddress', 'createdAt'] : view === 'keys' ? ['name', 'keyPrefix', 'isActive', 'lastUsedAt', 'expiresAt'] : ['requestedByName', 'status', 'startedAt', 'completedAt', 'errorMessage']} page={page} onPage={setPage} actions={view === 'audit' ? undefined : row => <>{view === 'keys' ? row.isActive && <Action danger onClick={() => open('revoke', row)}>Revoke</Action> : <><Action onClick={() => open('details', row)}>Details</Action><Action danger onClick={() => open('delete', row)}>Delete</Action></>}</>} />
    {mode && <Modal title={mode === 'details' ? 'Backup details' : mode === 'revoke' ? 'Revoke API key' : mode === 'delete' ? 'Delete backup' : view === 'keys' ? 'Create API key' : 'Create backup'} onClose={() => !mutation.isPending && setMode(null)}>{mode === 'details' ? <RecordDetails record={selected} /> : <><p className="mb-4 text-sm">{mode === 'revoke' ? `Revoke “${selected.name}”? Clients using this key will lose access.` : mode === 'delete' ? 'Permanently delete this backup record and its backup file?' : view === 'keys' ? 'The secret is shown once. Save it securely before closing the result.' : 'Submit a new backup request to the server?'}</p><Editor fields={fields} pending={mutation.isPending} error={mutation.error} onSubmit={submit} submitLabel="Confirm" /></>}</Modal>}
    {secret && <Modal title="Save your API key" onClose={() => {
      setSecret(null);
      mutation.reset();
    }}><p className="mb-3 text-sm">This secret is only shown now. Store it securely.</p><textarea readOnly aria-label="New API key" className={`${inputClass} font-mono`} value={secret.apiKey} rows={4} /><p className="mt-3 text-sm">Key name: {secret.name}</p><Action onClick={() => {
        setSecret(null);
        mutation.reset();
      }}>I have saved the key</Action></Modal>}
  </Panel>;
}
function Threats() {
  const [tab, setTab] = useState('failed-logins'),
    [page, setPage] = useState(1),
    [mode, setMode] = useState(null),
    [selected, setSelected] = useState(null);
  const query = useAdminQuery(`${base}/threats/${tab}`, {
    page,
    pageSize: 20
  });
  const mutation = useAdminAction();
  return <Panel><div className="mb-5 flex flex-wrap gap-3"><select aria-label="Threat view" className={`${inputClass} max-w-xs`} value={tab} onChange={e => {
        setTab(e.target.value);
        setPage(1);
      }}><option value="failed-logins">Failed logins</option><option value="suspicious-ips">Suspicious IP addresses</option><option value="blocked-ips">Blocked IP addresses</option></select><Action onClick={() => {
        mutation.reset();
        setSelected(null);
        setMode('block');
      }}>Block an IP</Action><Action onClick={() => query.refetch()}>Refresh</Action></div><DataTable query={query} page={page} onPage={setPage} columns={tab === 'failed-logins' ? ['email', 'ipAddress', 'succeeded', 'createdAt'] : tab === 'suspicious-ips' ? ['ipAddress', 'failedAttempts'] : ['ipAddress', 'reason', 'blockedByName', 'expiresAt']} actions={row => row.ipAddress && <Action danger onClick={() => {
      mutation.reset();
      setSelected(row);
      setMode(tab === 'blocked-ips' ? 'unblock' : 'block');
    }}>{tab === 'blocked-ips' ? 'Unblock' : 'Block'}</Action>} />{mode && <Modal title={mode === 'block' ? 'Block IP address' : 'Unblock IP address'} onClose={() => !mutation.isPending && setMode(null)}><p className="mb-4 text-sm">{mode === 'block' ? 'Requests from this address will be blocked.' : 'Restore access for ' + selected.ipAddress + '?'}</p><Editor initial={selected || {}} fields={mode === 'block' ? [{
        key: 'ipAddress',
        label: 'IP address',
        required: true
      }, {
        key: 'reason',
        label: 'Reason',
        required: true,
        type: 'textarea',
        maxLength: 500
      }, {
        key: 'expiresAt',
        label: 'Expires at',
        type: 'datetime-local'
      }] : []} onSubmit={body => mutation.mutate({
        method: mode === 'block' ? 'post' : 'delete',
        path: `${base}/threats/blocked-ips`,
        payload: mode === 'block' ? body : undefined,
        params: mode === 'unblock' ? {
          ipAddress: selected.ipAddress
        } : undefined
      }, {
        onSuccess: () => setMode(null)
      })} pending={mutation.isPending} error={mutation.error} submitLabel="Confirm" /></Modal>}</Panel>;
}
