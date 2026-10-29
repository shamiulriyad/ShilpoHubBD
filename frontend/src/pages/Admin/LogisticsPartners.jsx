import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { PageHeader, Button } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { logisticsPartnersService } from '../../services/logisticsPartnersService';
import { enrichAdminData } from '../../data/adminDemoData';

const emptyPartner = { companyName: '', logoUrl: '', description: '', contactPersonName: '', contactPhone: '', contactEmail: '', baseAddressLine: '', baseCity: '', country: 'Bangladesh', fleetSize: 0, maxDailyPickups: 0, offersCashOnDelivery: true, offersPickup: true, supportsReturns: true, isAcceptingRequests: true, isActive: true };
const emptyArea = { districtId: '', areaName: '', deliveryMethod: 'Standard', standardDeliveryDays: 3, deliveryCharge: 0, pickupAvailable: true, returnSupported: true, isActive: true };
const input = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

export default function AdminLogisticsPartners() {
  const districts = useDistricts(); const [partners, setPartners] = useState([]); const [selected, setSelected] = useState(null);
  const [credentials, setCredentials] = useState({ fullName: '', email: '', password: '' });
  const [accountMessage, setAccountMessage] = useState('');
  const createAccount = async (event) => {
    event.preventDefault(); setBusy(true); setError(''); setAccountMessage('');
    try {
      const account = await logisticsPartnersService.createAccount(selected.id, credentials);
      setCredentials({ fullName: '', email: '', password: '' });
      setAccountMessage(`Account created for ${account.email}. Give the operator the email and initial password you entered. They can now sign in to their assigned company.`);
      await choose(selected.id);
    } catch (e) { setError(e.response?.data?.detail || e.response?.data?.title || e.message); }
    finally { setBusy(false); }
  };
  const [form, setForm] = useState(emptyPartner); const [area, setArea] = useState(emptyArea); const [performance, setPerformance] = useState(null); const [busy, setBusy] = useState(false); const [error, setError] = useState('');
  const load = async () => { const page = enrichAdminData('/admin/logistics-partners', await logisticsPartnersService.list({ pageSize: 100 })); setPartners(page.items || []); };
  useEffect(() => { load().catch((e) => setError(e.response?.data?.title || e.message)); }, []);
  const choose = async (id) => { try { const [value, stats] = await Promise.all([logisticsPartnersService.getOfficial(id), logisticsPartnersService.performance(id)]); setSelected(value); setPerformance(stats); setForm({ ...emptyPartner, ...value }); } catch (e) { setError(e.response?.data?.title || e.message); } };
  const set = (key) => (e) => setForm((p) => ({ ...p, [key]: e.target.type === 'checkbox' ? e.target.checked : e.target.value }));
  const save = async (e) => { e.preventDefault(); setBusy(true); setError(''); try { const payload = { ...form, fleetSize: Number(form.fleetSize), maxDailyPickups: Number(form.maxDailyPickups), baseDistrictId: form.baseDistrictId || null }; const value = selected ? await logisticsPartnersService.updateOfficial(selected.id, payload) : await logisticsPartnersService.createOfficial(payload); setSelected(value); setForm({ ...emptyPartner, ...value }); await load(); } catch (x) { setError(x.response?.data?.title || x.message); } finally { setBusy(false); } };
  const saveArea = async (e) => { e.preventDefault(); setBusy(true); setError(''); try { const value = await logisticsPartnersService.upsertOfficialServiceArea(selected.id, { ...area, standardDeliveryDays: Number(area.standardDeliveryDays), deliveryCharge: Number(area.deliveryCharge), surchargeAmount: 0 }); setSelected(value); setArea(emptyArea); await load(); } catch (x) { setError(x.response?.data?.title || x.message); } finally { setBusy(false); } };
  const removeArea = async (id) => { if (!window.confirm('Remove this delivery coverage?')) return; setSelected(await logisticsPartnersService.removeOfficialServiceArea(selected.id, id)); await load(); };
  return <div><PageHeader title="Logistics Partners" description="Official companies, delivery coverage, methods, ETA and charges." />
    {error && <p className="mb-4 rounded-lg border border-danger/30 p-3 text-sm text-danger">{error}</p>}
    {accountMessage && <p role="status" className="mb-4 rounded-lg bg-primary/10 p-3 text-sm">{accountMessage}</p>}
    {selected && performance && <section className="mb-6 rounded-xl border border-border bg-surface p-5">
      <h2 className="mb-3 font-semibold">Delivery performance — {selected.companyName}</h2>
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">{[['Delivered', performance.delivered], ['In transit', performance.inTransit], ['Failed', performance.failed], ['Cancelled', performance.cancelled], ['Returned', performance.returned], ['Total delivery revenue', `৳${performance.totalRevenue}`], ['Average delivery time', performance.averageDeliveryHours == null ? 'No completed deliveries' : `${performance.averageDeliveryHours.toFixed(1)} hours`]].map(([label, value]) => <div key={label}><p className="text-xs text-body/60">{label}</p><strong>{value}</strong></div>)}</div>
      <Link className="mt-4 inline-block font-semibold text-primary underline" to={`/logistics-partner/shipments?partnerId=${selected.id}`}>View company deliveries and tracking history</Link>
      <div className="mt-4 space-y-2">{performance.areas.map(a => <p key={a.area} className="text-sm">{a.area}: {a.deliveries} deliveries · {a.delivered} delivered · {a.failed} failed</p>)}</div>
    </section>}
    {selected && <section className="mb-6 rounded-xl border border-border bg-surface p-5">
      <h2 className="font-semibold">Edit a coverage rule</h2>
      <select aria-label="Edit existing coverage" className={`${input} mt-3 w-full`} value={area.id || ''} onChange={e => setArea(selected.serviceAreas.find(a => a.id === e.target.value) || emptyArea)}>
        <option value="">New coverage rule</option>{selected.serviceAreas.map(a => <option key={a.id} value={a.id}>{a.division} / {a.districtName} / {a.areaName || 'All areas'} / {a.deliveryMethod}</option>)}
      </select>
      <p className="mt-2 text-xs text-body/60">Edit the selected rule in the coverage form below. Changing its district, area or method creates a separate rule.</p>
      <div className="mt-3 flex flex-wrap gap-4">{[['isActive', 'Delivery available'], ['pickupAvailable', 'Pickup available'], ['returnSupported', 'Returns supported']].map(([key, label]) => <label key={key} className="flex gap-2 text-sm"><input type="checkbox" checked={area[key]} onChange={e => setArea(p => ({ ...p, [key]: e.target.checked }))} />{label}</label>)}</div>
    </section>}
    {selected && <section className="mb-6 rounded-xl border border-border bg-surface p-5">
      <h2 className="font-semibold">Operator login — {selected.companyName}</h2>
      {selected.userId ? <p className="mt-2 text-sm">An operator account is linked to this company. Manage account access in the admin user directory.</p> :
        <form onSubmit={createAccount} className="mt-3 grid gap-3 sm:grid-cols-2">
          <p className="text-sm text-body/70 sm:col-span-2">Create credentials for this company. Logistics operators cannot register themselves or change company coverage.</p>
          <label className="grid gap-1 text-sm">Operator name<input required maxLength={200} className={input} value={credentials.fullName} onChange={e => setCredentials(p => ({ ...p, fullName: e.target.value }))} /></label>
          <label className="grid gap-1 text-sm">Login email<input required type="email" autoComplete="off" className={input} value={credentials.email} onChange={e => setCredentials(p => ({ ...p, email: e.target.value }))} /></label>
          <label className="grid gap-1 text-sm">Initial password<input required type="password" minLength={8} autoComplete="new-password" className={input} value={credentials.password} onChange={e => setCredentials(p => ({ ...p, password: e.target.value }))} /><span className="text-xs text-body/60">At least 8 characters with uppercase, lowercase and a number.</span></label>
          <Button type="submit" disabled={busy}>Create operator account</Button>
        </form>}
    </section>}
    <div className="grid gap-6 xl:grid-cols-[320px_1fr]"><section className="rounded-xl border border-border bg-surface p-4"><Button className="mb-4 w-full" onClick={() => { setSelected(null); setForm(emptyPartner); }}>Add partner</Button><div className="space-y-2">{partners.map((p) => <button key={p.id} disabled={p.isDemo} onClick={() => choose(p.id)} className="w-full rounded-lg border border-border p-3 text-left hover:border-primary disabled:cursor-default disabled:hover:border-border"><span className="block font-medium text-heading">{p.companyName}</span><span className="text-xs text-body/60">{p.isActive ? 'Active' : 'Inactive'} · {p.serviceAreaCount} coverage rules{p.isDemo ? ' · Preview data' : ''}</span></button>)}</div></section>
      <div className="space-y-6"><form onSubmit={save} className="grid gap-3 rounded-xl border border-border bg-surface p-5 sm:grid-cols-2"><h2 className="sm:col-span-2 font-semibold text-heading">{selected ? 'Edit company' : 'New official partner'}</h2>
        {[['companyName','Company name'],['logoUrl','Logo URL'],['contactPersonName','Contact person'],['contactPhone','Phone'],['contactEmail','Email'],['baseCity','Base city'],['baseAddressLine','Base address']].map(([key,label]) => <label key={key} className="flex flex-col gap-1 text-xs text-body/60">{label}<input required={key !== 'logoUrl'} value={form[key] || ''} onChange={set(key)} className={input}/></label>)}
        <label className="flex flex-col gap-1 text-xs text-body/60 sm:col-span-2">Description<textarea value={form.description || ''} onChange={set('description')} className={input}/></label>
        {[['legalName', 'Legal name'], ['registrationNumber', 'Registration number'], ['basePostalCode', 'Postal code']].map(([key, label]) => <label key={key} className="grid gap-1 text-xs text-body/60">{label}<input value={form[key] || ''} onChange={set(key)} className={input} /></label>)}
        <label className="grid gap-1 text-xs text-body/60">Base district<select className={input} value={form.baseDistrictId || ''} onChange={set('baseDistrictId')}><option value="">Select district</option>{(districts.data || []).map(d => <option key={d.id} value={d.id}>{d.division} / {d.name}</option>)}</select></label>
        {[['fleetSize', 'Fleet size'], ['maxDailyPickups', 'Daily pickup capacity'], ['operatingDayStartHour', 'Opening hour (0–24)'], ['operatingDayEndHour', 'Closing hour (0–24)'], ['maxVehicleCapacityKg', 'Vehicle capacity (kg)']].map(([key, label]) => <label key={key} className="grid gap-1 text-xs text-body/60">{label}<input type="number" min="0" value={form[key] ?? ''} onChange={e => setForm(p => ({ ...p, [key]: e.target.value === '' ? null : Number(e.target.value) }))} className={input} /></label>)}
        {[['offersCashOnDelivery', 'Cash on delivery'], ['offersColdChain', 'Cold chain'], ['offersFragileHandling', 'Fragile handling']].map(([key, label]) => <label key={key} className="flex gap-2 text-sm"><input type="checkbox" checked={Boolean(form[key])} onChange={set(key)} />{label}</label>)}
        {[['isActive','Active'],['isAcceptingRequests','Accepting deliveries'],['offersPickup','Pickup available'],['supportsReturns','Return delivery']].map(([key,label]) => <label key={key} className="flex items-center gap-2 text-sm"><input type="checkbox" checked={Boolean(form[key])} onChange={set(key)}/>{label}</label>)}
        <Button type="submit" disabled={busy} className="sm:col-span-2">{busy ? 'Saving…' : 'Save partner'}</Button></form>
        {selected && <section className="rounded-xl border border-border bg-surface p-5">{performance && <div className="mb-5 grid grid-cols-2 gap-3 md:grid-cols-4"><div>Total deliveries<br/><strong>{performance.totalDeliveries}</strong></div><div>Success rate<br/><strong>{performance.successRate}%</strong></div><div>ShilpoHub revenue<br/><strong>৳{performance.shilpoHubRevenue}</strong></div><div>Partner revenue<br/><strong>৳{performance.partnerRevenue}</strong></div></div>}<h2 className="mb-3 font-semibold text-heading">Service coverage</h2><div className="mb-4 space-y-2">{selected.serviceAreas.map((a) => <div key={a.id} className="flex justify-between rounded-lg border border-border p-3 text-sm"><span>{a.division} → {a.districtName}{a.areaName ? ` → ${a.areaName}` : ''} · {a.deliveryMethod} · {a.standardDeliveryDays}d · ৳{a.deliveryCharge}</span><button className="text-danger" onClick={() => removeArea(a.id)}>Remove</button></div>)}</div>
          <form onSubmit={saveArea} className="grid gap-3 sm:grid-cols-3"><select aria-label="Coverage district" required value={area.districtId} onChange={(e) => setArea((p) => ({...p,districtId:e.target.value}))} className={input}><option value="">District</option>{(districts.data || []).map((d)=><option key={d.id} value={d.id}>{d.division} → {d.name}</option>)}</select><input aria-label="Coverage upazila or area" placeholder="Upazila / area (blank = district-wide)" value={area.areaName} onChange={(e)=>setArea((p)=>({...p,areaName:e.target.value}))} className={input}/><select aria-label="Delivery method" value={area.deliveryMethod} onChange={(e)=>setArea((p)=>({...p,deliveryMethod:e.target.value}))} className={input}><option>Standard</option><option>Express</option><option>SameDay</option></select><input aria-label="Estimated delivery days" type="number" min="0" placeholder="ETA days" value={area.standardDeliveryDays} onChange={(e)=>setArea((p)=>({...p,standardDeliveryDays:e.target.value}))} className={input}/><input aria-label="Delivery charge" type="number" min="0" step="0.01" placeholder="Charge" value={area.deliveryCharge} onChange={(e)=>setArea((p)=>({...p,deliveryCharge:e.target.value}))} className={input}/><Button type="submit" disabled={busy}>Save coverage</Button></form></section>}
      </div></div></div>;
}
