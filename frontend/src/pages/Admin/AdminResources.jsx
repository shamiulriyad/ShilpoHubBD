import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { superAdminService as api } from '../../services/superAdminService';
import { resources, editorFields } from './adminConfig';
import { useDistricts } from '../../hooks/useDistricts';
import { Action, DataTable, Editor, ErrorNotice, Modal, Panel, useAdminQuery, inputClass, labelOf } from './AdminUI';
export default function AdminResources({
  view
}) {
  const config = resources[view];
  const [page, setPage] = useState(1),
    [drafts, setDrafts] = useState(false),
    [editing, setEditing] = useState(null),
    [remove, setRemove] = useState(null);
  const cache = useQueryClient();
  const districts = useDistricts();
  const [syncDistrict, setSyncDistrict] = useState('');
  const sync = useMutation({
    mutationFn: () => api.action('post', '/tourism/sync', undefined, { districtId: syncDistrict }),
    onSuccess: () => cache.invalidateQueries(),
  });
  const query = useAdminQuery(config.path + (drafts ? '/drafts' : ''), {
    page,
    pageSize: 20,
    ...config.params
  });
  const detail = useMutation({
    mutationFn: row => config.noDetail ? Promise.resolve(row) : api.detail(config.path, row.id),
    onSuccess: setEditing
  });
  const save = useMutation({
    mutationFn: body => api.save(config.path, editing?.id, body),
    onSuccess: () => {
      setEditing(null);
      cache.invalidateQueries();
    }
  });
  const deletion = useMutation({
    mutationFn: () => api.action('delete', `${config.path}/${remove.id}`),
    onSuccess: () => {
      setRemove(null);
      setPage(1);
      cache.invalidateQueries();
    }
  });
  return <Panel><div className="mb-5 flex flex-wrap items-center justify-between gap-3"><p className="text-sm text-body/60">Manage {labelOf(view).toLowerCase()} and keep public information up to date.</p><div className="flex gap-2">{config.drafts && <select aria-label="Publication status" className={inputClass} value={drafts ? 'drafts' : 'published'} onChange={e => {
          setDrafts(e.target.value === 'drafts');
          setPage(1);
        }}><option value="published">Published</option><option value="drafts">Drafts</option></select>}{!config.noCreate && <Action onClick={() => {
          save.reset();
          setEditing({});
        }}>Create new</Action>}<Action onClick={() => query.refetch()} disabled={query.isFetching}>Refresh</Action></div></div>
    {view === 'locations' && <div className="mb-4 flex flex-wrap items-center gap-2 rounded-lg border border-border p-3">
      <span className="text-sm text-body/70">Import from OpenStreetMap:</span>
      <select aria-label="District to sync" className={inputClass} value={syncDistrict} onChange={e => setSyncDistrict(e.target.value)}><option value="">Choose district</option>{(districts.data ?? []).map(d => <option key={d.id} value={d.id}>{d.name}</option>)}</select>
      <Action disabled={!syncDistrict || sync.isPending} onClick={() => sync.mutate()}>{sync.isPending ? 'Syncing…' : 'Sync now'}</Action>
      {sync.data && <span className="text-xs text-body/60">{sync.data.message}</span>}
      <ErrorNotice error={sync.error} />
    </div>}
    {['categories', 'villages'].includes(view) && <p className="mb-4 text-sm text-body/60">This list contains active records. Deactivated records are excluded by the server.</p>}
    <ErrorNotice error={detail.error} /><DataTable query={query} columns={config.columns} page={page} onPage={setPage} actions={row => <><Action disabled={detail.isPending} onClick={() => {
        save.reset();
        detail.mutate(row);
      }}>Edit</Action>{!config.noDelete && <Action danger onClick={() => {
        deletion.reset();
        setRemove(row);
      }}>Delete</Action>}</>} />
    {editing && <Modal title={editing.id ? 'Edit record' : 'Create record'} onClose={() => !save.isPending && setEditing(null)}><Editor fields={editorFields(config, Boolean(editing.id))} initial={editing} onSubmit={body => save.mutate(body)} pending={save.isPending} error={save.error} /></Modal>}
    {remove && <Modal title="Delete record" onClose={() => !deletion.isPending && setRemove(null)}><p className="mb-4">Delete “{remove.title || remove.name || remove.sectionKey}”? This removes it from the available records.</p><ErrorNotice error={deletion.error} /><Action danger disabled={deletion.isPending} onClick={() => deletion.mutate()}>{deletion.isPending ? 'Deleting…' : 'Confirm delete'}</Action></Modal>}
  </Panel>;
}
