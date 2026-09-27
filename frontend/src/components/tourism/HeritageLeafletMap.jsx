import { useEffect, useRef, useState } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { hasCoordinates } from '../../utils/tourismAdapters';

const MARKER_COLORS = {
  origin: '#2f7d4f', destination: '#173b35', stop: '#af4d29',
  hotel: '#2f6f8f', resort: '#7a4fa8', hostel: '#b8860b',
  guesthouse: '#8a5a44', motel: '#5b6b7a', homestay: '#4d7c0f',
  touristplace: '#af4d29', heritagesite: '#1f6f4a', restaurant: '#c2410c', attraction: '#0f766e',
};

// Real-world points close enough together (~300m) render as one overlapping blob at any zoom a
// travel map is actually used at. Fan them out in a small ring around their shared centre so each
// one stays visible and clickable, instead of stacking exactly on top of each other.
const OVERLAP_THRESHOLD_DEGREES = 0.003;
const FAN_OUT_RADIUS_DEGREES = 0.0018;
function spreadOverlapping(mapped) {
  const groups = [];
  mapped.forEach((place) => {
    const lat = Number(place.latitude);
    const lng = Number(place.longitude);
    const group = groups.find((g) => Math.abs(g.lat - lat) < OVERLAP_THRESHOLD_DEGREES && Math.abs(g.lng - lng) < OVERLAP_THRESHOLD_DEGREES);
    if (group) group.items.push({ place, lat, lng });
    else groups.push({ lat, lng, items: [{ place, lat, lng }] });
  });
  return groups.flatMap(({ items }) => {
    if (items.length === 1) {
      const { place, lat, lng } = items[0];
      return [{ ...place, latitude: lat, longitude: lng }];
    }
    const angleStep = (2 * Math.PI) / items.length;
    return items.map(({ place, lat, lng }, i) => ({
      ...place,
      latitude: lat + FAN_OUT_RADIUS_DEGREES * Math.cos(angleStep * i),
      longitude: lng + FAN_OUT_RADIUS_DEGREES * Math.sin(angleStep * i),
    }));
  });
}

export default function HeritageLeafletMap({ places, selectedId, onSelect, route, routeGeometry }) {
  const host = useRef(null);
  const mapRef = useRef(null);
  const layerRef = useRef(null);
  const routeLayerRef = useRef(null);
  const roadRouteLayerRef = useRef(null);
  const tileRef = useRef(null);
  const selectRef = useRef(onSelect);
  selectRef.current = onSelect;
  const [tileError, setTileError] = useState(false);
  useEffect(() => {
    const map = L.map(host.current, { scrollWheelZoom: false, minZoom: 5, maxZoom: 18 }).setView([23.8,90.3],7);
    mapRef.current = map;
    tileRef.current = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution:'&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' }).addTo(map);
    tileRef.current.on('tileerror',()=>setTileError(true));
    layerRef.current = L.layerGroup().addTo(map);
    routeLayerRef.current = L.layerGroup().addTo(map);
    roadRouteLayerRef.current = L.layerGroup().addTo(map);
    const resize = new ResizeObserver(()=>map.invalidateSize());
    resize.observe(host.current);
    return () => { resize.disconnect(); map.remove(); mapRef.current=null; };
  }, []);
  useEffect(()=>{
    const map = mapRef.current;
    if (!map) return;

    // Draw everything first, then make ONE decision about the view. Letting each layer fit its
    // own bounds independently (markers, then the road route) meant whichever ran last silently
    // won -- the full origin-to-destination road line would zoom the map out to fit ~350km,
    // crushing a tight cluster of nearby destination markers into an overlapping blob.
    layerRef.current.clearLayers();
    routeLayerRef.current.clearLayers();
    roadRouteLayerRef.current.clearLayers();

    const mapped = places.filter(hasCoordinates);
    const spread = spreadOverlapping(mapped);
    spread.forEach(place=>{
      const active = place.id === selectedId;
      const kind = place.kind || 'stop';
      const color = active && kind==='stop' ? '#173b35' : MARKER_COLORS[kind] || MARKER_COLORS.stop;
      const size = kind === 'stop' ? 22 : 26;
      const marker = L.marker([place.latitude,place.longitude], {
        title:place.name, alt:place.name, keyboard:true,
        icon:L.divIcon({className:'heritage-map-marker',html:`<span style="display:block;width:${size}px;height:${size}px;border-radius:50%;background:${color};border:3px solid white;box-shadow:0 2px 8px #0005"></span>`,iconSize:[size,size],iconAnchor:[size/2,size/2]}),
      }).addTo(layerRef.current);
      const label = document.createElement('span'); label.textContent=place.name;
      marker.bindTooltip(label,{direction:'top'});
      if (place.popupHtml) marker.bindPopup(place.popupHtml, { maxWidth: 260 });
      marker.on('click',()=>selectRef.current?.(place.id));
    });

    if (route?.length) {
      const points = route
        .map(id=>mapped.find(p=>p.id===id))
        .filter(Boolean)
        .map(p=>[Number(p.latitude),Number(p.longitude)]);
      if (points.length >= 2) {
        L.polyline(points, { color:'#a84f2d', weight:3, dashArray:'6 8', opacity:.85 }).addTo(routeLayerRef.current);
      }
    }

    if (routeGeometry?.length) {
      const points = routeGeometry.map(p=>[Number(p.latitude),Number(p.longitude)]);
      L.polyline(points, { color:'#173b35', weight:4, opacity:.85 }).addTo(roadRouteLayerRef.current);
    }

    const selected = mapped.find(p=>p.id===selectedId);
    // A distant "origin" marker (e.g. a starting city far from the destination) must not drag the
    // fit-bounds calculation out to a country-wide view -- that's what was crushing the
    // destination cluster in the first place. Prefer fitting the destination-side markers only;
    // origin still renders on the map, just may sit outside the initial viewport.
    const focusMarkers = mapped.filter(p => p.kind !== 'origin');
    const bounds = focusMarkers.length > 0 ? focusMarkers : mapped;

    if (selected) {
      map.setView([Number(selected.latitude),Number(selected.longitude)],13);
    } else if (bounds.length > 1) {
      map.fitBounds(bounds.map(p=>[Number(p.latitude),Number(p.longitude)]),{padding:[45,45],maxZoom:14});
    } else if (bounds.length === 1) {
      map.setView([Number(bounds[0].latitude),Number(bounds[0].longitude)],13);
    } else if (routeGeometry?.length) {
      map.fitBounds(routeGeometry.map(p=>[Number(p.latitude),Number(p.longitude)]),{padding:[35,35]});
    } else {
      map.setView([23.8,90.3],7);
    }
  },[places,selectedId,route,routeGeometry]);
  return <div className="overflow-hidden rounded-xl border border-border bg-surface">
    <div ref={host} aria-label="Interactive heritage map. Use plus and minus to zoom; select a place from the list for details." className="relative z-0 h-[420px] w-full sm:h-[560px]" />
    {tileError && <p role="status" className="border-t border-border p-3 text-sm">Some map tiles could not load. The place list remains available. <button className="font-semibold text-link underline" onClick={()=>{setTileError(false);tileRef.current?.redraw();}}>Retry map</button></p>}
    <p className="border-t border-border px-4 py-3 text-xs text-body/70">Drag to pan · Use + / − to zoom · Select a marker or a place below. Reference locations are approximate; check your route before travel.{routeGeometry?.length>1 && ' The solid line is a real road route (OSRM).'}{route?.length>1 && ' The dashed line is a straight-line visit order, not a real road or transit path.'}</p>
  </div>;
}
