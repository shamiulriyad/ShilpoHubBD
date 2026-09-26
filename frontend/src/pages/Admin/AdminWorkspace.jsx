import { Link, useParams } from 'react-router-dom';
import { PageHeader } from '../../components/ui';
import { adminGroups, resources } from './adminConfig';
import AdminResources from './AdminResources';
import AdminUsers from './AdminUsers';
import AdminMarketplace from './AdminMarketplace';
import AdminModeration from './AdminModeration';
import AdminSecurity, { SystemHealth } from './AdminSecurity';
import { Panel, ErrorNotice, useAdminQuery } from './AdminUI';
export default function AdminWorkspace({
  section: providedSection
}) {
  const params = useParams(),
    section = params.section || providedSection;
  if (!section) return <Overview />;
  const group = adminGroups.find(g => g[0] === section),
    view = params.view || group?.[2][0][0];
  const title = group?.[2].find(v => v[0] === view)?.[1];
  if (!title) return <Panel><h1 className="text-xl font-semibold">Admin page not found</h1><Link to="/admin" className="text-primary underline">Return to dashboard</Link></Panel>;
  return <div><PageHeader title={title} description={`${group[1]} · Super Admin workspace`} breadcrumbs={[{
      label: 'Admin',
      path: '/admin'
    }, {
      label: title
    }]} /><div className="mb-6 flex flex-wrap gap-2" aria-label={`${group[1]} sections`}>{group[2].map(([key, label]) => <Link key={key} to={`/admin/${section}/${key}`} aria-current={view === key ? 'page' : undefined} className={`rounded-full border px-4 py-2 text-sm font-medium ${view === key ? 'border-primary bg-primary text-white' : 'border-border bg-surface text-heading hover:border-primary'}`}>{label}</Link>)}</div><div key={`${section}/${view}`}>{section === 'users' ? <AdminUsers view={view} /> : section === 'heritage' || section === 'cms' || (section === 'marketplace' && ['productTypes', 'materials'].includes(view) && resources[view]) ? <AdminResources view={view} /> : section === 'moderation' || view === 'fraud' ? <AdminModeration view={view} /> : section === 'marketplace' ? <AdminMarketplace view={view} /> : <AdminSecurity view={view} />}</div></div>;
}
function QueueCard({
  title,
  path,
  params,
  to
}) {
  const q = useAdminQuery(path, {
    page: 1,
    pageSize: 1,
    ...params
  });
  return <Panel><p className="text-sm text-body/60">{title}</p><p className="my-3 text-3xl font-semibold">{q.isPending ? '…' : q.isError ? '—' : q.data?.totalCount ?? 0}</p><ErrorNotice error={q.error} /><Link to={to} className="text-sm font-semibold text-primary">Open queue →</Link></Panel>;
}
function Overview() {
  return <div><PageHeader title="Super Admin" description="Manage access, preserve heritage, and keep the marketplace safe." /><div className="mb-6 rounded-2xl bg-[#173f32] p-6 text-white sm:p-8"><p className="text-xs font-semibold uppercase tracking-[0.2em] text-[#d7c39d]">ShilpoHub control centre</p><h2 className="mt-3 text-2xl font-semibold text-white">A trusted platform starts here.</h2><p className="mt-2 max-w-2xl text-sm leading-6 text-[#d1dfd4]">Review pending decisions, publish community stories, and monitor platform operations from one workspace.</p></div><div className="mb-6 grid gap-4 md:grid-cols-3"><QueueCard title="Products awaiting approval" path="/products/pending-approval" to="/admin/marketplace/approval" /><QueueCard title="Identity checks awaiting review" path="/identity-verifications" params={{
        status: 'Pending'
      }} to="/admin/users/identity" /><QueueCard title="Open fraud findings" path="/governance/monitoring/flags" params={{
        status: 'Open', flagType: 'FraudRisk'
      }} to="/admin/marketplace/fraud" /></div><SystemHealth /><div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{adminGroups.map(([key, label, views]) => <Panel key={key}><h2 className="text-lg font-semibold">{label}</h2><p className="my-3 text-sm leading-6 text-body/60">{views.map(v => v[1]).join(' · ')}</p><Link className="text-sm font-semibold text-primary" to={`/admin/${key}/${views[0][0]}`}>Open workspace →</Link></Panel>)}</div></div>;
}
