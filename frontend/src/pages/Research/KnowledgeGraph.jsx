import { useMemo, useState } from 'react';
import { PageHeader, Button, AsyncState, Badge } from '../../components/ui';
import KnowledgeGraphCanvas from '../../components/knowledge/KnowledgeGraphCanvas';
import { useAuth } from '../../hooks/useAuth';
import { useKnowledgeEntities, useKnowledgeGraphMutations, useKnowledgeNeighbors, useKnowledgeNetwork, useKnowledgeNodes, useKnowledgePath, useKnowledgeRules, useKnowledgeStats } from '../../hooks/useKnowledgeGraph';
import { confirmAction } from '../../lib/confirm';

const inputClass = 'min-h-11 rounded-lg border border-border bg-background px-3 py-2 text-sm text-heading outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/10';
const nodeTypes = ['Heritage', 'Product', 'Craft', 'Artisan', 'Producer', 'Village', 'District', 'Division', 'Material', 'Technique', 'TouristPlace', 'Food', 'Festival', 'CulturalSite', 'Community', 'CulturalTradition', 'HeritageCategory'];
const manualTypes = new Set(['Division', 'Technique', 'Community', 'CulturalTradition', 'HeritageCategory']);
const networks = [['ProducerRelationships', 'Producer Relationships'], ['VillageConnections', 'Village Connections'], ['MaterialNetwork', 'Material Network'], ['CulturalNetwork', 'Cultural Network'], ['FamilyTree', 'Heritage Family Tree']];
const humanize = (value = '') => value.replace(/([a-z])([A-Z])/g, '$1 $2');
const errorMessage = (error) => error?.response?.data?.detail || error?.response?.data?.title || error?.message || 'The request could not be completed.';

function Stat({ label, value, onClick }) {
  return <button type="button" onClick={onClick} disabled={!onClick} className="min-w-[120px] rounded-xl border border-border bg-background px-4 py-3 text-left disabled:cursor-default"><strong className="block text-xl text-heading">{value ?? '—'}</strong><span className="text-xs text-body/60">{label}</span></button>;
}
function NewNodePanel({ onClose }) {
  const [type, setType] = useState('Heritage');
  const [search, setSearch] = useState('');
  const [isOpen, setIsOpen] = useState(true);
  const [selectedEntityId, setSelectedEntityId] = useState('');
  const [selectedEntityType, setSelectedEntityType] = useState('');
  const [selectedEntityName, setSelectedEntityName] = useState('');
  const [selectedDescription, setSelectedDescription] = useState('');
  const [manual, setManual] = useState({ label: '', description: '' });
  const candidates = useKnowledgeEntities({ nodeType: type, search, take: 30 });
  const { createNode, importNode } = useKnowledgeGraphMutations();
  const usesDatabase = !manualTypes.has(type);
  const hasValidSelection = Boolean(selectedEntityId && selectedEntityType === type && selectedEntityName);
  const hasValidManualNode = Boolean(manualTypes.has(type) && manual.label.trim());
  const mutation = usesDatabase ? importNode : createNode;
  const clearSelection = () => {
    setSelectedEntityId(''); setSelectedEntityType(''); setSelectedEntityName(''); setSelectedDescription('');
  };
  const changeType = (nextType) => {
    setType(nextType); setSearch(''); clearSelection(); setManual({ label: '', description: '' }); setIsOpen(true);
  };
  const selectRecord = (record) => {
    setSelectedEntityId(record.id); setSelectedEntityType(record.entityType); setSelectedEntityName(record.name);
    setSelectedDescription(record.description || ''); setSearch(record.name); setIsOpen(false);
  };
  const submit = (event) => {
    event.preventDefault();
    if (usesDatabase && !hasValidSelection) return;
    if (!usesDatabase && !hasValidManualNode) return;
    const payload = usesDatabase
      ? { nodeType: type, entityId: selectedEntityId, entityType: selectedEntityType }
      : { nodeType: type, label: manual.label.trim(), description: manual.description.trim() };
    mutation.mutate(payload, { onSuccess: onClose });
  };
  return <div className="mb-6 rounded-2xl border border-primary/25 bg-surface p-5 shadow-sm">
    <div className="mb-4 flex items-start justify-between"><div><h2 className="font-semibold text-heading">Add knowledge node</h2><p className="text-sm text-body/60">Link an existing ShilpoHub record whenever one is available.</p></div><button type="button" onClick={onClose} aria-label="Close">×</button></div>
    <form onSubmit={submit} className="grid gap-4 lg:grid-cols-[220px_1fr]">
      <label className="grid gap-1 text-sm font-medium">Node type<select className={inputClass} value={type} onChange={(e) => changeType(e.target.value)}>{nodeTypes.map((item) => <option key={item} value={item}>{humanize(item)}</option>)}</select></label>
      {usesDatabase ? <div className="grid gap-2"><label className="grid gap-1 text-sm font-medium">Find an existing record<div role="combobox" aria-expanded={isOpen} aria-haspopup="listbox" className="relative"><input aria-autocomplete="list" aria-controls="knowledge-record-options" className={`${inputClass} w-full`} value={search} onFocus={() => setIsOpen(true)} onChange={(e) => { setSearch(e.target.value); clearSelection(); setIsOpen(true); }} placeholder={`Search ${humanize(type).toLowerCase()} records`} />{isOpen && <ul id="knowledge-record-options" role="listbox" className="absolute z-20 mt-1 max-h-56 w-full overflow-auto rounded-lg border border-border bg-surface p-1 shadow-lg">{candidates.isLoading && <li className="p-3 text-sm text-body/60">Loading records...</li>}{candidates.isError && <li role="alert" className="p-3 text-sm text-danger">Could not load existing records.</li>}{!candidates.isLoading && !candidates.isError && candidates.data?.map((item) => <li key={item.id} role="option" aria-selected={selectedEntityId === item.id}><button type="button" onClick={() => selectRecord(item)} className={`block w-full rounded-md px-3 py-2 text-left ${selectedEntityId === item.id ? 'bg-primary/10 text-primary' : 'hover:bg-muted'}`}><span className="block text-sm font-medium">{item.name}</span><span className="block text-xs text-body/60">{humanize(item.entityType)}</span></button></li>)}{!candidates.isLoading && !candidates.isError && candidates.data?.length === 0 && <li className="p-3 text-sm text-body/60">No existing records found. You can create this node manually if supported.</li>}</ul>}</div></label></div> : <div className="grid gap-3"><label className="grid gap-1 text-sm font-medium">Name<input required className={inputClass} value={manual.label} onChange={(e) => setManual((p) => ({ ...p, label: e.target.value }))} /></label><label className="grid gap-1 text-sm font-medium">Description<textarea rows="3" className={inputClass} value={manual.description} onChange={(e) => setManual((p) => ({ ...p, description: e.target.value }))} /></label></div>}
      {hasValidSelection && <div className="lg:col-start-2 rounded-lg border border-primary/30 bg-primary/5 p-3"><div className="flex items-start justify-between"><div><p className="text-xs font-semibold uppercase tracking-wide text-primary">Selected record</p><p className="font-semibold text-heading">{selectedEntityName}</p><p className="text-xs text-body/60">{humanize(selectedEntityType)} · ID {selectedEntityId}</p></div><button type="button" className="text-sm text-body/60 hover:text-heading" onClick={() => { clearSelection(); setSearch(''); setIsOpen(true); }}>Change</button></div>{selectedDescription && <p className="mt-2 text-sm text-body/60">{selectedDescription}</p>}</div>}
      {mutation.isError && <p role="alert" className="lg:col-span-2 text-sm text-danger">{errorMessage(mutation.error)}</p>}
      <div className="flex gap-2 lg:col-span-2"><Button type="submit" variant="primary" disabled={mutation.isPending || (usesDatabase ? !hasValidSelection : !hasValidManualNode)}>{mutation.isPending ? 'Adding…' : 'Add to Knowledge Graph'}</Button><Button type="button" variant="secondary" onClick={onClose}>Cancel</Button></div>
    </form>
  </div>;
}

function NodeDrawer({ node, graph, canManage, onClose }) {
  const { updateNode, removeNode } = useKnowledgeGraphMutations();
  if (!node) return null;
  const relations = (graph?.relationships || []).filter((r) => r.sourceNodeId === node.id || r.targetNodeId === node.id);
  const neighbors = relations.map((r) => graph.nodes.find((n) => n.id === (r.sourceNodeId === node.id ? r.targetNodeId : r.sourceNodeId))).filter(Boolean);
  const counts = ['Product', 'Artisan', 'Village', 'District', 'TouristPlace', 'Material'].map((type) => [type, neighbors.filter((n) => n.nodeType === type).length]).filter(([, count]) => count);
  const archive = () => updateNode.mutate({ id: node.id, payload: { label: node.label, description: node.description, metadataJson: node.metadataJson, isCurated: false } });
  const remove = async () => { const message = relations.length ? `${node.label} has ${relations.length} existing relationships. Deleting it will also remove those graph connections.` : `Delete ${node.label}?`; if (await confirmAction(message, { confirmLabel: 'Delete node' })) removeNode.mutate(node.id, { onSuccess: onClose }); };
  return <aside className="fixed inset-y-0 right-0 z-40 w-full max-w-md overflow-y-auto border-l border-border bg-surface p-6 shadow-2xl"><div className="flex items-start justify-between"><div><Badge>{humanize(node.nodeType)}</Badge><h2 className="mt-3 text-2xl font-semibold text-heading">{node.label}</h2><p className="mt-2 text-sm text-body/70">{node.description || 'No description has been added.'}</p></div><button type="button" onClick={onClose} className="text-2xl" aria-label="Close details">×</button></div><dl className="mt-6 grid grid-cols-2 gap-3"><div className="rounded-lg bg-background p-3"><dt className="text-xs text-body/60">Status</dt><dd className="font-medium">{node.status}</dd></div><div className="rounded-lg bg-background p-3"><dt className="text-xs text-body/60">Relationships</dt><dd className="font-medium">{relations.length}</dd></div>{counts.map(([type, count]) => <div key={type} className="rounded-lg bg-background p-3"><dt className="text-xs text-body/60">Connected {humanize(type)}</dt><dd className="font-medium">{count}</dd></div>)}</dl>{node.externalEntityId && <div className="mt-4 rounded-lg border border-border p-3"><p className="text-xs text-body/60">Original entity</p><code className="break-all text-xs">{node.externalEntityId}</code></div>}<h3 className="mb-2 mt-6 font-semibold text-heading">Relationships</h3><div className="space-y-2">{relations.map((r) => <div key={r.id} className="rounded-lg border border-border p-3 text-sm"><span className="font-medium">{r.sourceNodeId === node.id ? humanize(r.relationshipType) : (r.reverseLabel || `Reverse of ${humanize(r.relationshipType)}`)}</span><span className="block text-body/60">{r.sourceNodeId === node.id ? r.targetLabel : r.sourceLabel}</span></div>)}{!relations.length && <p className="text-sm text-body/60">This node is isolated.</p>}</div>{canManage && <div className="mt-6 flex gap-2"><Button variant="secondary" onClick={archive} disabled={updateNode.isPending}>Archive</Button><Button variant="secondary" onClick={remove} disabled={removeNode.isPending}>Delete</Button></div>}</aside>;
}

export default function KnowledgeGraph() {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['GovernmentNGO', 'HeritageInnovationHub', 'SuperAdmin']);
  const [filters, setFilters] = useState({ nodeType: '', search: '' });
  const [network, setNetwork] = useState('ProducerRelationships');
  const [showNew, setShowNew] = useState(false);
  const [selectedNode, setSelectedNode] = useState(null);
  const [notice, setNotice] = useState('');
  const nodesQuery = useKnowledgeNodes({ pageSize: 100, nodeType: filters.nodeType || undefined, search: filters.search || undefined });
  const catalogQuery = useKnowledgeNodes({ pageSize: 100 });
  const selectedGraph = useKnowledgeNeighbors(selectedNode?.id);
  const networkQuery = useKnowledgeNetwork(network, { depth: 4, maxNodes: 500 });
  const stats = useKnowledgeStats(); const rules = useKnowledgeRules();
  const { createRelationship } = useKnowledgeGraphMutations();
  const [link, setLink] = useState({ sourceNodeId: '', relationshipType: '', targetNodeId: '' });
  const [pathForm, setPathForm] = useState({ sourceNodeId: '', targetNodeId: '' }); const [pathQuery, setPathQuery] = useState(null); const path = useKnowledgePath(pathQuery);
  const filteredNodes = nodesQuery.data?.items || []; const allNodes = catalogQuery.data?.items || []; const source = allNodes.find((n) => n.id === link.sourceNodeId);
  const validRules = (rules.data || []).filter((r) => r.sourceType === source?.nodeType); const relationshipTypes = [...new Set(validRules.map((r) => r.relationshipType))];
  const targetTypes = new Set(validRules.filter((r) => r.relationshipType === link.relationshipType).map((r) => r.targetType)); const targetNodes = allNodes.filter((n) => targetTypes.has(n.nodeType) && n.id !== link.sourceNodeId);
  const visibleGraph = useMemo(() => { const base = networkQuery.data || { nodes: [], relationships: [] }; const term = filters.search.trim().toLowerCase(); const keep = new Set(base.nodes.filter((n) => (!filters.nodeType || n.nodeType === filters.nodeType) && (!term || n.label.toLowerCase().includes(term))).map((n) => n.id)); return { ...base, nodes: base.nodes.filter((n) => keep.has(n.id)), relationships: base.relationships.filter((r) => keep.has(r.sourceNodeId) && keep.has(r.targetNodeId)) }; }, [networkQuery.data, filters]);
  const submitLink = (event) => { event.preventDefault(); setNotice(''); createRelationship.mutate({ ...link, isDirected: true }, { onSuccess: () => { setNotice('Relationship created successfully.'); setLink({ sourceNodeId: '', relationshipType: '', targetNodeId: '' }); }, onError: (e) => setNotice(errorMessage(e)) }); };
  const highlightedNodeIds = path.data?.found ? path.data.nodes.map((n) => n.id) : []; const highlightedRelationshipIds = path.data?.found ? path.data.relationships.map((r) => r.id) : [];
  return <div className="pb-6"><PageHeader title="Knowledge Graph" description="Curate heritage knowledge nodes and relationships, and explore verified connections." />
    <div className="mb-5 flex flex-wrap gap-2"><select aria-label="Filter by node type" className={inputClass} value={filters.nodeType} onChange={(e) => setFilters((p) => ({ ...p, nodeType: e.target.value }))}><option value="">All Types</option>{nodeTypes.map((type) => <option key={type} value={type}>{humanize(type)}</option>)}</select><input aria-label="Search node names" className={`${inputClass} min-w-[260px]`} value={filters.search} onChange={(e) => setFilters((p) => ({ ...p, search: e.target.value }))} placeholder="Search node names" />{canManage && <Button variant="primary" onClick={() => setShowNew(true)}>New Node</Button>}</div>{showNew && <NewNodePanel onClose={() => setShowNew(false)} />}
    <section className="mb-6 rounded-2xl border border-border bg-surface p-5 shadow-sm"><div className="mb-4 flex flex-wrap items-start justify-between gap-4"><div><h2 className="font-semibold text-heading">Heritage networks</h2><div className="mt-3 flex flex-wrap gap-2">{networks.map(([key, label]) => <button key={key} type="button" onClick={() => setNetwork(key)} className={`rounded-full border px-3 py-1.5 text-xs font-medium ${network === key ? 'border-primary bg-primary/10 text-primary' : 'border-border text-body/70 hover:border-primary/50'}`}>{label}</button>)}</div></div><div className="flex gap-2"><Stat label="nodes" value={stats.data?.totalNodes} /><Stat label="relationships" value={stats.data?.totalRelationships} /><Stat label="isolated nodes" value={stats.data?.isolatedNodes} /></div></div><AsyncState isLoading={networkQuery.isLoading} isError={networkQuery.isError} error={networkQuery.error}><KnowledgeGraphCanvas graph={visibleGraph} selectedId={selectedNode?.id} onSelect={setSelectedNode} highlightedNodeIds={highlightedNodeIds} highlightedRelationshipIds={highlightedRelationshipIds} /></AsyncState></section>
    {canManage && <section className="mb-6 rounded-2xl border border-border bg-surface p-5"><h2 className="font-semibold text-heading">Link two nodes</h2><p className="mb-4 text-sm text-body/60">Valid relationships and targets are filtered by the selected source type.</p><form onSubmit={submitLink} className="grid gap-3 md:grid-cols-[1fr_1fr_1fr_auto]"><select aria-label="Source node" className={inputClass} value={link.sourceNodeId} onChange={(e) => setLink({ sourceNodeId: e.target.value, relationshipType: '', targetNodeId: '' })}><option value="">Source node</option>{allNodes.map((n) => <option key={n.id} value={n.id}>{n.label} · {humanize(n.nodeType)}</option>)}</select><select aria-label="Relationship type" className={inputClass} disabled={!source} value={link.relationshipType} onChange={(e) => setLink((p) => ({ ...p, relationshipType: e.target.value, targetNodeId: '' }))}><option value="">Relationship type</option>{relationshipTypes.map((type) => <option key={type} value={type}>{humanize(type).toUpperCase()}</option>)}</select><select aria-label="Target node" className={inputClass} disabled={!link.relationshipType} value={link.targetNodeId} onChange={(e) => setLink((p) => ({ ...p, targetNodeId: e.target.value }))}><option value="">Target node</option>{targetNodes.map((n) => <option key={n.id} value={n.id}>{n.label} · {humanize(n.nodeType)}</option>)}</select><Button type="submit" variant="primary" disabled={!link.sourceNodeId || !link.relationshipType || !link.targetNodeId || createRelationship.isPending}>{createRelationship.isPending ? 'Linking…' : 'Link'}</Button></form>{notice && <p role="status" className={`mt-3 text-sm ${createRelationship.isError ? 'text-danger' : 'text-primary'}`}>{notice}</p>}</section>}
    <section className="rounded-2xl border border-border bg-surface p-5"><h2 className="font-semibold text-heading">Find shortest path</h2><p className="mb-4 text-sm text-body/60">Uses breadth-first search across stored relationships.</p><div className="flex flex-wrap gap-3"><select aria-label="Path source node" className={inputClass} value={pathForm.sourceNodeId} onChange={(e) => setPathForm((p) => ({ ...p, sourceNodeId: e.target.value }))}><option value="">From</option>{allNodes.map((n) => <option key={n.id} value={n.id}>{n.label}</option>)}</select><select aria-label="Path target node" className={inputClass} value={pathForm.targetNodeId} onChange={(e) => setPathForm((p) => ({ ...p, targetNodeId: e.target.value }))}><option value="">To</option>{allNodes.map((n) => <option key={n.id} value={n.id}>{n.label}</option>)}</select><Button variant="secondary" disabled={!pathForm.sourceNodeId || !pathForm.targetNodeId} onClick={() => setPathQuery({ ...pathForm, maxDepth: 50 })}>Find Path</Button></div>{path.isFetching && <p className="mt-3 text-sm text-body/60">Finding the shortest connection…</p>}{path.data && <div className="mt-4 rounded-xl bg-background p-4">{path.data.found ? <><p className="font-semibold text-primary">Shortest path found: {path.data.length} {path.data.length === 1 ? 'relationship' : 'relationships'}.</p><div className="mt-2 flex flex-wrap items-center gap-2 text-sm">{path.data.nodes.map((node, index) => <span key={node.id} className="contents"><button type="button" onClick={() => setSelectedNode(node)} className="rounded-full border border-border bg-surface px-3 py-1 font-medium">{node.label}</button>{path.data.relationships[index] && <span className="text-body/60">→ {humanize(path.data.relationships[index].relationshipType)} →</span>}</span>)}</div></> : <p className="text-body/70">No connection exists between these nodes.</p>}</div>}</section>
    <div className="mt-6"><h2 className="mb-3 font-semibold text-heading">Nodes</h2><AsyncState isLoading={nodesQuery.isLoading} isError={nodesQuery.isError} error={nodesQuery.error}><div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{filteredNodes.map((node) => <button key={node.id} type="button" onClick={() => setSelectedNode(node)} className="rounded-xl border border-border bg-surface p-4 text-left transition hover:-translate-y-0.5 hover:border-primary/40 hover:shadow-sm"><div className="flex items-center justify-between"><strong className="text-heading">{node.label}</strong><Badge>{humanize(node.nodeType)}</Badge></div><p className="mt-2 line-clamp-2 text-sm text-body/60">{node.description || 'No description'}</p><p className="mt-3 text-xs text-body/50">{node.outgoingCount + node.incomingCount} relationships · {node.status}</p></button>)}{!filteredNodes.length && <p className="text-sm text-body/60">No knowledge nodes match the current filters.</p>}</div></AsyncState></div><NodeDrawer node={selectedNode} graph={selectedGraph.data || visibleGraph} canManage={canManage} onClose={() => setSelectedNode(null)} />
  </div>;
}
