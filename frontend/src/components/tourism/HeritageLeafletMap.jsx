import { useEffect, useRef, useState } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { hasCoordinates } from '../../data/tourismGuides';

export default function HeritageLeafletMap({ places, selectedId, onSelect }) {
  const host = useRef(null);
  const mapRef = useRef(null);
  const layerRef = useRef(null);
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
    const resize = new ResizeObserver(()=>map.invalidateSize());
    resize.observe(host.current);
    return () => { resize.disconnect(); map.remove(); mapRef.current=null; };
  }, []);
  useEffect(()=>{
    const map = mapRef.current;
    if (!map) return;
    layerRef.current.clearLayers();
    const mapped = places.filter(hasCoordinates);
    mapped.forEach(place=>{
      const active = place.id === selectedId;
      const marker = L.marker([Number(place.latitude),Number(place.longitude)], {
        title:place.name, alt:place.name, keyboard:true,
        icon:L.divIcon({className:'heritage-map-marker',html:`<span style="display:block;width:22px;height:22px;border-radius:50%;background:${active?'#173b35':'#af4d29'};border:3px solid white;box-shadow:0 2px 8px #0005"></span>`,iconSize:[22,22],iconAnchor:[11,11]}),
      }).addTo(layerRef.current);
      const label = document.createElement('span'); label.textContent=place.name;
      marker.bindTooltip(label,{direction:'top'}).on('click',()=>selectRef.current(place.id));
    });
    const selected=mapped.find(p=>p.id===selectedId);
    if(selected) map.setView([Number(selected.latitude),Number(selected.longitude)],11);
    else if(mapped.length) map.fitBounds(mapped.map(p=>[Number(p.latitude),Number(p.longitude)]),{padding:[35,35],maxZoom:11});
    else map.setView([23.8,90.3],7);
  },[places,selectedId]);
  return <div className="overflow-hidden rounded-xl border border-border bg-surface">
    <div ref={host} aria-label="Interactive heritage map. Use plus and minus to zoom; select a place from the list for details." className="relative z-0 h-[420px] w-full sm:h-[560px]" />
    {tileError && <p role="status" className="border-t border-border p-3 text-sm">Some map tiles could not load. The place list remains available. <button className="font-semibold text-link underline" onClick={()=>{setTileError(false);tileRef.current?.redraw();}}>Retry map</button></p>}
    <p className="border-t border-border px-4 py-3 text-xs text-body/70">Drag to pan · Use + / − to zoom · Select a marker or a place below. Reference locations are approximate; check your route before travel.</p>
  </div>;
}
