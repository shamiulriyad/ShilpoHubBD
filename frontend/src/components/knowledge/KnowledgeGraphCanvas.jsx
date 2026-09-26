import { useMemo, useRef, useState } from 'react';

const colors = {
  Heritage: '#8f3f2a', Product: '#c56b39', Craft: '#a77b2d', Artisan: '#78558e',
  Producer: '#315f56', Village: '#547b45', District: '#3f6f89', Division: '#50647b',
  Material: '#9a7150', Technique: '#6f5b43', TouristPlace: '#267d79', Food: '#c27b2e',
  Festival: '#b14e65', CulturalSite: '#6e5546', Community: '#557265',
  CulturalTradition: '#856441', HeritageCategory: '#707c42',
};

function initialPositions(nodes) {
  const count = Math.max(nodes.length, 1);
  return Object.fromEntries(nodes.map((node, index) => {
    const ring = Math.floor(index / 12);
    const ringIndex = index % 12;
    const ringCount = Math.min(12, count - ring * 12);
    const angle = (Math.PI * 2 * ringIndex) / Math.max(ringCount, 1) - Math.PI / 2;
    const radius = nodes.length === 1 ? 0 : 135 + ring * 92;
    return [node.id, { x: 450 + Math.cos(angle) * radius, y: 250 + Math.sin(angle) * radius }];
  }));
}

export default function KnowledgeGraphCanvas({ graph, selectedId, onSelect, highlightedNodeIds = [], highlightedRelationshipIds = [] }) {
  const nodes = graph?.nodes || [];
  const relationships = graph?.relationships || [];
  const signature = nodes.map((n) => n.id).join('|');
  const defaults = useMemo(() => initialPositions(nodes), [signature]); // eslint-disable-line react-hooks/exhaustive-deps
  const [moved, setMoved] = useState({});
  const [view, setView] = useState({ x: 0, y: 0, scale: 1 });
  const action = useRef(null);
  const positions = { ...defaults, ...moved };
  const highlightedNodes = new Set(highlightedNodeIds);
  const highlightedEdges = new Set(highlightedRelationshipIds);

  const pointerDown = (event, nodeId) => {
    event.currentTarget.setPointerCapture(event.pointerId);
    action.current = nodeId
      ? { kind: 'node', id: nodeId, startX: event.clientX, startY: event.clientY, origin: positions[nodeId] }
      : { kind: 'pan', startX: event.clientX, startY: event.clientY, origin: view };
  };
  const pointerMove = (event) => {
    const current = action.current;
    if (!current) return;
    const dx = event.clientX - current.startX;
    const dy = event.clientY - current.startY;
    if (current.kind === 'node') {
      setMoved((old) => ({ ...old, [current.id]: { x: current.origin.x + dx / view.scale, y: current.origin.y + dy / view.scale } }));
    } else {
      setView((old) => ({ ...old, x: current.origin.x + dx, y: current.origin.y + dy }));
    }
  };
  const reset = () => { setMoved({}); setView({ x: 0, y: 0, scale: 1 }); };

  if (!nodes.length) {
    return <div className="flex h-[390px] items-center justify-center rounded-xl bg-background text-sm text-body/60">No nodes or relationships are available in this network yet.</div>;
  }

  return (
    <div className="relative overflow-hidden rounded-xl border border-border bg-[#fbfaf6]" aria-label="Interactive knowledge graph">
      <div className="absolute right-3 top-3 z-10 flex gap-1 rounded-lg border border-border bg-surface p-1 shadow-sm">
        <button type="button" aria-label="Zoom in" className="h-8 w-8 rounded hover:bg-primary/10" onClick={() => setView((v) => ({ ...v, scale: Math.min(2.2, v.scale + 0.2) }))}>+</button>
        <button type="button" aria-label="Zoom out" className="h-8 w-8 rounded hover:bg-primary/10" onClick={() => setView((v) => ({ ...v, scale: Math.max(0.45, v.scale - 0.2) }))}>−</button>
        <button type="button" className="h-8 rounded px-2 text-xs hover:bg-primary/10" onClick={reset}>Fit</button>
      </div>
      <svg viewBox="0 0 900 500" className="h-[390px] w-full touch-none select-none"
        onPointerDown={(e) => pointerDown(e)} onPointerMove={pointerMove}
        onPointerUp={() => { action.current = null; }} onPointerCancel={() => { action.current = null; }}
        onWheel={(e) => { e.preventDefault(); setView((v) => ({ ...v, scale: Math.max(0.45, Math.min(2.2, v.scale + (e.deltaY < 0 ? 0.12 : -0.12))) })); }}>
        <defs><marker id="kg-arrow" markerWidth="8" markerHeight="8" refX="7" refY="3" orient="auto"><path d="M0,0 L0,6 L8,3 z" fill="#9ca39e" /></marker></defs>
        <g transform={`translate(${view.x} ${view.y}) scale(${view.scale})`}>
          {relationships.map((edge) => {
            const a = positions[edge.sourceNodeId]; const b = positions[edge.targetNodeId];
            if (!a || !b) return null;
            const active = highlightedEdges.has(edge.id);
            return <g key={edge.id}>
              <line x1={a.x} y1={a.y} x2={b.x} y2={b.y} stroke={active ? '#b4532f' : '#b9bfba'} strokeWidth={active ? 3 : 1.4} markerEnd={edge.isDirected ? 'url(#kg-arrow)' : undefined} />
              <text x={(a.x + b.x) / 2} y={(a.y + b.y) / 2 - 5} textAnchor="middle" className="fill-body text-[9px]" paintOrder="stroke" stroke="#fbfaf6" strokeWidth="4">{edge.relationshipType.replace(/([a-z])([A-Z])/g, '$1 $2').toUpperCase()}</text>
            </g>;
          })}
          {nodes.map((node) => {
            const p = positions[node.id]; const active = selectedId === node.id || highlightedNodes.has(node.id);
            return <g key={node.id} transform={`translate(${p.x} ${p.y})`} className="cursor-grab active:cursor-grabbing"
              onPointerDown={(e) => { e.stopPropagation(); pointerDown(e, node.id); }} onClick={(e) => { e.stopPropagation(); onSelect?.(node); }}>
              <circle r={active ? 32 : 28} fill={colors[node.nodeType] || '#315f56'} stroke={active ? '#e2a43a' : '#fff'} strokeWidth={active ? 5 : 3} />
              <text y="4" textAnchor="middle" className="pointer-events-none fill-white text-[11px] font-semibold">{node.label.slice(0, 2).toUpperCase()}</text>
              <rect x="-64" y="35" width="128" height="24" rx="12" fill="#fff" stroke="#dfe3de" />
              <text y="51" textAnchor="middle" className="pointer-events-none fill-heading text-[10px] font-medium">{node.label.length > 20 ? `${node.label.slice(0, 18)}…` : node.label}</text>
            </g>;
          })}
        </g>
      </svg>
    </div>
  );
}
