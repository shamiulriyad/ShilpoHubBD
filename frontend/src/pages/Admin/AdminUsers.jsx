import { useState } from 'react';
import { Action, DataTable, Editor, ErrorNotice, Feedback, Modal, Panel, RecordDetails, rowsOf, useAdminAction, useAdminQuery, inputClass } from './AdminUI';
const roles = ['Customer', 'Producer', 'BusinessPartner', 'Tourist', 'HeritageAcademyMember', 'HeritageInnovationHub', 'GovernmentNGO', 'LogisticsPartner', 'SuperAdmin'];
export default function AdminUsers({
  view
}) {
  if (view === 'permissions') return <Permissions />;
  if (view === 'roles') return <Roles />;
  return <Directory view={view} />;
}
function Directory({
  view
}) {
  const [page, setPage] = useState(1),
    [search, setSearch] = useState(''),
    [term, setTerm] = useState(''),
    [status, setStatus] = useState(''),
    [selected, setSelected] = useState(null),
    [decision, setDecision] = useState(null);
  const identity = view === 'identity',
    verify = view === 'verification';
  const path = identity ? '/identity-verifications' : verify ? '/business-partners' : '/admin/users';
  const query = useAdminQuery(path, {
    page,
    pageSize: 20,
    search: term || undefined,
    ...(identity ? {
      status: status || undefined
    } : verify ? {
      verificationStatus: status || undefined
    } : {
      isActive: status === '' ? undefined : status === 'active'
    })
  });
  const detail = useAdminQuery(`${path}/${verify ? selected?.userId : selected?.id}`, {}, Boolean(selected));
  const mutation = useAdminAction();
  const columns = identity ? ['userFullName', 'userEmail', 'type', 'status', 'submittedAt'] : verify ? ['companyName', 'userFullName', 'userEmail', 'verificationStatus'] : ['fullName', 'email', 'roles', 'isActive', 'identityVerificationStatus'];
  const act = (label, path, payload, fields = []) => {
    mutation.reset();
    setDecision({
      label,
      path,
      payload,
      fields
    });
  };
  return <Panel><form className="mb-5 flex flex-wrap gap-3" onSubmit={e => {
      e.preventDefault();
      setTerm(search);
      setPage(1);
    }}><input aria-label="Search users" className={`${inputClass} max-w-sm`} value={search} onChange={e => setSearch(e.target.value)} placeholder="Search by name or email" /><button className="rounded-lg bg-primary px-4 py-2 text-sm text-white">Search</button><select aria-label="Filter status" className={`${inputClass} max-w-xs`} value={status} onChange={e => {
        setStatus(e.target.value);
        setPage(1);
      }}><option value="">All statuses</option>{(identity ? ['Pending', 'Approved', 'Rejected'] : verify ? ['Pending', 'Verified', 'Rejected', 'Suspended'] : ['active', 'inactive']).map(s => <option key={s}>{s}</option>)}</select></form>
    <DataTable query={query} columns={columns} page={page} onPage={setPage} actions={row => <><Action onClick={() => setSelected(row)}>Review</Action>{!identity && !verify && <Action onClick={() => act(row.isActive ? 'Deactivate user' : 'Activate user', `/admin/users/${row.id}/${row.isActive ? 'deactivate' : 'activate'}`)}>{row.isActive ? 'Deactivate' : 'Activate'}</Action>}</>} />
    {selected && <Modal title="Review account" onClose={() => setSelected(null)}><ErrorNotice error={detail.error} />{detail.isPending ? <p>Loading details…</p> : detail.isSuccess && <><RecordDetails record={detail.data} />{(identity && detail.data.status === 'Pending' || verify) && <div className="mt-6 flex gap-3"><Action disabled={mutation.isPending} onClick={() => act('Approve verification', `${path}/${verify ? selected.userId : selected.id}/${verify ? 'verify' : 'approve'}`, verify ? {
            status: 'Verified'
          } : undefined)}>Approve</Action><Action danger onClick={() => act('Reject verification', `${path}/${verify ? selected.userId : selected.id}/${verify ? 'verify' : 'reject'}`, verify ? {
            status: 'Rejected'
          } : undefined, identity ? [{
            key: 'rejectionReason',
            label: 'Reason for rejection',
            type: 'textarea',
            required: true,
            maxLength: 1000
          }] : [{
            key: 'notes',
            label: 'Reason for rejection',
            type: 'textarea',
            required: true,
            maxLength: 1000
          }])}>Reject</Action></div>}</>}</Modal>}
    {decision && <Modal title={decision.label} onClose={() => !mutation.isPending && setDecision(null)}><p className="mb-4 text-sm text-body/70">Confirm this change to the selected account.</p><Editor fields={decision.fields} pending={mutation.isPending} error={mutation.error} submitLabel={decision.label} onSubmit={body => mutation.mutate({
        path: decision.path,
        payload: decision.fields.length ? {
          ...decision.payload,
          ...body
        } : decision.payload
      }, {
        onSuccess: () => {
          setDecision(null);
          setSelected(null);
        }
      })} /></Modal>}
  </Panel>;
}
function Roles() {
  const query = useAdminQuery('/admin/roles');
  const [search, setSearch] = useState(''),
    [term, setTerm] = useState(''),
    [page, setPage] = useState(1),
    [user, setUser] = useState(null),
    [operation, setOperation] = useState('assign');
  const users = useAdminQuery('/admin/users', {
    page,
    pageSize: 20,
    search: term || undefined
  });
  const mutation = useAdminAction();
  return <div className="space-y-6"><Panel><h2 className="mb-4 text-lg font-semibold">Workspace roles</h2><DataTable query={query} columns={['name', 'userCount', 'permissionCount']} /></Panel><Panel><h2 className="mb-4 text-lg font-semibold">Manage a member’s roles</h2><form className="mb-4 flex gap-3" onSubmit={e => {
        e.preventDefault();
        setTerm(search);
        setPage(1);
      }}><input className={inputClass} aria-label="Find member" placeholder="Find a member by name or email" value={search} onChange={e => setSearch(e.target.value)} /><button className="rounded-lg bg-primary px-4 text-white">Search</button></form><DataTable query={users} columns={['fullName', 'email', 'roles']} page={page} onPage={setPage} actions={row => <Action onClick={() => {
        mutation.reset();
        setUser(row);
        setOperation('assign');
      }}>Manage roles</Action>} /></Panel>{user && <Modal title={`Roles for ${user.fullName}`} onClose={() => !mutation.isPending && setUser(null)}><p className="mb-4 text-sm">Current roles: {user.roles.join(', ')}</p><select className={`${inputClass} mb-4`} aria-label="Role operation" value={operation} onChange={e => setOperation(e.target.value)}><option value="assign">Assign a role</option><option value="remove">Remove a role</option></select><Editor key={operation} fields={[{
        key: 'role',
        label: 'Role',
        required: true,
        options: operation === 'remove' ? user.roles : roles.filter(r => !user.roles.includes(r))
      }]} onSubmit={body => mutation.mutate({
        path: `/roles/${operation}`,
        payload: {
          userId: user.id,
          ...body
        }
      }, {
        onSuccess: () => setUser(null)
      })} error={mutation.error} pending={mutation.isPending} submitLabel="Confirm role change" /></Modal>}</div>;
}
function Permissions() {
  const permissions = useAdminQuery('/admin/permissions'),
    rolesQuery = useAdminQuery('/admin/roles');
  const [roleId, setRoleId] = useState(''),
    [create, setCreate] = useState(false),
    [remove, setRemove] = useState(null);
  const roleQuery = useAdminQuery(`/admin/roles/${roleId}/permissions`, {}, Boolean(roleId));
  const mutation = useAdminAction();
  return <div className="space-y-6"><Panel><h2 className="mb-4 text-lg font-semibold">Permissions by role</h2><ErrorNotice error={rolesQuery.error} /><select className={`${inputClass} mb-4 max-w-md`} aria-label="Choose role" value={roleId} onChange={e => setRoleId(e.target.value)}><option value="">Choose a role</option>{rowsOf(rolesQuery.data).map(r => <option key={r.id} value={r.id}>{r.name}</option>)}</select><ErrorNotice error={roleQuery.error} />{roleId && roleQuery.isPending && <p>Loading permissions…</p>}{roleId && roleQuery.isSuccess && permissions.isSuccess && <PermissionSelection key={`${roleId}-${roleQuery.dataUpdatedAt}`} roleId={roleId} all={permissions.data} selected={roleQuery.data.permissionCodes} />}</Panel><Panel><div className="mb-4 flex justify-between"><h2 className="text-lg font-semibold">Permission catalogue</h2><Action onClick={() => {
          mutation.reset();
          setCreate(true);
        }}>Create permission</Action></div><DataTable query={permissions} columns={['name', 'code', 'module', 'description']} actions={row => <Action danger onClick={() => {
        mutation.reset();
        setRemove(row);
      }}>Delete</Action>} /></Panel>{create && <Modal title="Create permission" onClose={() => !mutation.isPending && setCreate(false)}><Editor fields={[{
        key: 'code',
        label: 'Permission code',
        required: true,
        maxLength: 100
      }, {
        key: 'name',
        label: 'Name',
        required: true,
        maxLength: 150
      }, {
        key: 'module',
        label: 'Module',
        required: true,
        maxLength: 60
      }, {
        key: 'description',
        label: 'Description',
        type: 'textarea',
        maxLength: 500
      }]} onSubmit={payload => mutation.mutate({
        path: '/admin/permissions',
        payload
      }, {
        onSuccess: () => setCreate(false)
      })} error={mutation.error} pending={mutation.isPending} /></Modal>}{remove && <Modal title="Delete permission" onClose={() => !mutation.isPending && setRemove(null)}><p className="mb-4">Delete {remove.name} from the permission catalogue?</p><Editor fields={[]} onSubmit={() => mutation.mutate({
        method: 'delete',
        path: `/admin/permissions/${remove.id}`
      }, {
        onSuccess: () => setRemove(null)
      })} pending={mutation.isPending} error={mutation.error} submitLabel="Confirm deletion" /></Modal>}</div>;
}
function PermissionSelection({
  roleId,
  all,
  selected
}) {
  const [codes, setCodes] = useState(selected || []),
    [confirm, setConfirm] = useState(false);
  const mutation = useAdminAction();
  return <><div className="grid max-h-96 gap-3 overflow-y-auto sm:grid-cols-2">{all.map(p => <label className="flex gap-3 rounded-lg border border-border p-3 text-sm" key={p.id}><input type="checkbox" className="accent-primary" checked={codes.includes(p.code)} onChange={e => setCodes(prev => e.target.checked ? [...prev, p.code] : prev.filter(c => c !== p.code))} /><span>{p.name}<small className="block text-body/60">{p.module} · {p.code}</small></span></label>)}</div><div className="mt-4"><Action onClick={() => {
        mutation.reset();
        setConfirm(true);
      }}>Save role permissions</Action></div><Feedback mutation={mutation} />{confirm && <Modal title="Confirm role permissions" onClose={() => !mutation.isPending && setConfirm(false)}><p className="mb-4">Replace this role’s permissions with the {codes.length} selected permissions?</p><Editor fields={[]} onSubmit={() => mutation.mutate({
        method: 'put',
        path: `/admin/roles/${roleId}/permissions`,
        payload: {
          permissionCodes: codes
        }
      }, {
        onSuccess: () => setConfirm(false)
      })} pending={mutation.isPending} error={mutation.error} submitLabel="Confirm permissions" /></Modal>}</>;
}
