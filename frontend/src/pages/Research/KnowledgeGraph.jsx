import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useKnowledgeNodes, useKnowledgeNeighbors, useKnowledgePath, useKnowledgeNetwork, useKnowledgeGraphMutations } from '../../hooks/useKnowledgeGraph';

import { confirmAction } from '../../lib/confirm';
const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const nodeTypes = ['Producer', 'Village', 'Product', 'Craft', 'Material', 'Culture', 'Family', 'HeritagePlace', 'Custom'];
const relationshipTypes = [
  'PractisesCraft', 'LocatedInVillage', 'BelongsToFamily', 'DescendedFrom', 'ProducesProduct', 'CraftUsesMaterial',
  'SuppliesMaterialTo', 'VillageHasCulture', 'MentoredBy', 'CollaboratesWith', 'AssociatedWith', 'RelatedTo',
];
const networks = [
  { key: 'ProducerRelationships', label: 'Producer Relationships' },
  { key: 'VillageConnections', label: 'Village Connections' },
  { key: 'MaterialNetwork', label: 'Material Network' },
  { key: 'CulturalNetwork', label: 'Cultural Network' },
  { key: 'FamilyTree', label: 'Heritage Family Tree' },
];

function NetworkExplorer() {
  const [network, setNetwork] = useState('ProducerRelationships');
  const networkQuery = useKnowledgeNetwork(network, { depth: 2, maxNodes: 100 });
  const graph = networkQuery.data;

  return (
    <div className="mb-6 rounded-xl border border-border bg-surface p-4">
      <p className="mb-2 text-sm font-semibold text-heading">Heritage networks</p>
      <div className="mb-3 flex flex-wrap gap-2">
        {networks.map((n) => (
          <button
            key={n.key}
            type="button"
            aria-pressed={network === n.key}
            onClick={() => setNetwork(n.key)}
            className={`rounded-full border px-3 py-1 text-xs font-medium ${network === n.key ? 'border-primary bg-primary/10 text-primary' : 'border-border text-body/70'}`}
          >
            {n.label}
          </button>
        ))}
      </div>
      <AsyncState isLoading={networkQuery.isLoading} isError={networkQuery.isError} error={networkQuery.error}>
        {graph && (
          <div className="space-y-1 text-xs text-body/70">
            <p>{graph.nodes.length} nodes · {graph.relationships.length} relationships{graph.truncated ? ' (truncated)' : ''}</p>
            {graph.relationships.slice(0, 40).map((r) => (
              <div key={r.id} className="rounded-lg border border-border px-3 py-2">
                {r.sourceLabel} —[{r.relationshipType}]→ {r.targetLabel}
              </div>
            ))}
            {graph.relationships.length === 0 && <p>No relationships in this network yet.</p>}
          </div>
        )}
      </AsyncState>
    </div>
  );
}

function NodeNeighbors({ id }) {
  const neighborsQuery = useKnowledgeNeighbors(id);
  const { removeRelationship } = useKnowledgeGraphMutations();
  const graph = neighborsQuery.data;

  if (neighborsQuery.isLoading) return <p className="py-2 text-xs text-body/60">Loading neighbors…</p>;
  if (neighborsQuery.isError) return <p role="alert" className="py-2 text-xs text-danger">Unable to load relationships for this node.</p>;
  if (!graph) return <p className="py-2 text-xs text-body/60">Relationship data is unavailable.</p>;

  return (
    <div className="mt-3 space-y-1 text-xs text-body/70">
      {graph.relationships.map((r) => (
        <div key={r.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2">
          <span>{r.sourceLabel} —[{r.relationshipType}]→ {r.targetLabel}{r.weight != null ? ` (w=${r.weight})` : ''}</span>
          <button type="button" onClick={async () => { if (await confirmAction('Remove this? This cannot be undone.', { confirmLabel: 'Yes, remove' })) removeRelationship.mutate(r.id); }} className="text-danger hover:underline">Remove</button>
        </div>
      ))}
      {graph.relationships.length === 0 && <p>No relationships yet.</p>}
    </div>
  );
}

export default function KnowledgeGraph() {
  const [filters, setFilters] = useState({ nodeType: '', search: '' });
  const { data, isLoading, isError, error } = useKnowledgeNodes({ pageSize: 50, nodeType: filters.nodeType || undefined, search: filters.search || undefined });
  const { createNode, createRelationship } = useKnowledgeGraphMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [nodeForm, setNodeForm] = useState({ nodeType: 'Producer', label: '', description: '' });
  const [relForm, setRelForm] = useState({ sourceNodeId: '', targetNodeId: '', relationshipType: '' });
  const [pathForm, setPathForm] = useState({ sourceNodeId: '', targetNodeId: '' });
  const [pathQuery, setPathQuery] = useState(null);

  const nodes = data?.items || [];
  const pathResult = useKnowledgePath(pathQuery);

  const handleCreateNode = (event) => {
    event.preventDefault();
    if (!nodeForm.label) return;
    createNode.mutate(nodeForm, { onSuccess: () => { setShowForm(false); setNodeForm({ nodeType: 'Producer', label: '', description: '' }); } });
  };

  const handleCreateRelationship = (event) => {
    event.preventDefault();
    if (!relForm.sourceNodeId || !relForm.targetNodeId || !relForm.relationshipType) return;
    createRelationship.mutate(relForm, { onSuccess: () => setRelForm({ sourceNodeId: '', targetNodeId: '', relationshipType: '' }) });
  };

  return (
    <div>
      <PageHeader title="Knowledge Graph" description="Curate heritage knowledge nodes and relationships, and explore connections." />

      <div className="mb-4 flex flex-wrap items-center gap-2">
        <select aria-label="Node Type" value={filters.nodeType} onChange={(e) => setFilters((p) => ({ ...p, nodeType: e.target.value }))} className={inputClass}>
          <option value="">All types</option>
          {nodeTypes.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <input aria-label="Search" placeholder="Search" value={filters.search} onChange={(e) => setFilters((p) => ({ ...p, search: e.target.value }))} className={inputClass} />
        <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Node'}</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreateNode} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select aria-label="Node Type" value={nodeForm.nodeType} onChange={(e) => setNodeForm((p) => ({ ...p, nodeType: e.target.value }))} className={inputClass}>
            {nodeTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input aria-label="Label" required placeholder="Label" value={nodeForm.label} onChange={(e) => setNodeForm((p) => ({ ...p, label: e.target.value }))} className={inputClass} />
          <textarea aria-label="Description" rows={2} placeholder="Description" value={nodeForm.description} onChange={(e) => setNodeForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createNode.isPending}>{createNode.isPending ? 'Creating…' : 'Create Node'}</Button>
        </form>
      )}

      <NetworkExplorer />

      <div className="mb-6 rounded-xl border border-border bg-surface p-4">
        <p className="mb-2 text-sm font-semibold text-heading">Link two nodes</p>
        <form onSubmit={handleCreateRelationship} className="flex flex-wrap gap-2">
          <select aria-label="Source Node Id" value={relForm.sourceNodeId} onChange={(e) => setRelForm((p) => ({ ...p, sourceNodeId: e.target.value }))} className={inputClass}>
            <option value="">Source node</option>
            {nodes.map((n) => <option key={n.id} value={n.id}>{n.label}</option>)}
          </select>
          <select aria-label="Relationship type" value={relForm.relationshipType} onChange={(e) => setRelForm((p) => ({ ...p, relationshipType: e.target.value }))} className={inputClass}>
            <option value="">Relationship type</option>
            {relationshipTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <select aria-label="Target Node Id" value={relForm.targetNodeId} onChange={(e) => setRelForm((p) => ({ ...p, targetNodeId: e.target.value }))} className={inputClass}>
            <option value="">Target node</option>
            {nodes.map((n) => <option key={n.id} value={n.id}>{n.label}</option>)}
          </select>
          <Button type="submit" variant="secondary" disabled={createRelationship.isPending}>Link</Button>
        </form>
      </div>

      <div className="mb-6 rounded-xl border border-border bg-surface p-4">
        <p className="mb-2 text-sm font-semibold text-heading">Find shortest path</p>
        <div className="flex flex-wrap gap-2">
          <select aria-label="Source Node Id" value={pathForm.sourceNodeId} onChange={(e) => setPathForm((p) => ({ ...p, sourceNodeId: e.target.value }))} className={inputClass}>
            <option value="">From</option>
            {nodes.map((n) => <option key={n.id} value={n.id}>{n.label}</option>)}
          </select>
          <select aria-label="Target Node Id" value={pathForm.targetNodeId} onChange={(e) => setPathForm((p) => ({ ...p, targetNodeId: e.target.value }))} className={inputClass}>
            <option value="">To</option>
            {nodes.map((n) => <option key={n.id} value={n.id}>{n.label}</option>)}
          </select>
          <Button variant="secondary" disabled={!pathForm.sourceNodeId || !pathForm.targetNodeId} onClick={() => setPathQuery({ ...pathForm, maxDepth: 5 })}>
            Find Path
          </Button>
        </div>
        {pathQuery && pathResult.data && (
          <p className="mt-2 text-xs text-body/70">
            {pathResult.data.found ? pathResult.data.nodes.map((n) => n.label).join(' → ') : 'No path found within depth limit.'}
          </p>
        )}
      </div>

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {nodes.map((n) => (
            <div key={n.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{n.label}</p>
                  <p className="text-xs text-body/60">{n.description || 'No description'} · {n.outgoingCount} out / {n.incomingCount} in</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge>{n.nodeType}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === n.id ? null : n.id)}>
                    {expandedId === n.id ? 'Hide' : 'Neighbors'}
                  </Button>
                </div>
              </div>
              {expandedId === n.id && <NodeNeighbors id={n.id} />}
            </div>
          ))}
          {nodes.length === 0 && <p className="text-sm text-body/60">No knowledge nodes yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
