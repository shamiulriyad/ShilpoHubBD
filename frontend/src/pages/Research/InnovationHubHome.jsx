import { Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { routePaths } from '../../routes/routePaths';

const pillars = [
  ['01', 'Document living heritage', 'Bring craft knowledge, regional context and trusted references together in a structured heritage database.'],
  ['02', 'Research with communities', 'Organise field research, evidence and publications around the people who keep traditions alive.'],
  ['03', 'Develop thoughtful innovation', 'Explore ideas, prototypes and preservation strategies that connect traditional knowledge with new possibilities.'],
];

export default function InnovationHubHome() {
  const { isAuthenticated, activeRole } = useAuth();
  const isResearcher = isAuthenticated && activeRole === 'HeritageInnovationHub';
  return (
    <div className="mx-auto max-w-7xl px-5 py-12 sm:py-20 lg:px-8">
      <section className="grid gap-10 border-b border-border pb-12 lg:grid-cols-[1.3fr_1fr] lg:items-end">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-primary">ShilpoHub · Innovation Hub</p>
          <h1 className="mt-5 max-w-3xl text-4xl font-semibold leading-tight text-heading sm:text-5xl">Living heritage.<br />Thoughtful progress.</h1>
          <p className="mt-6 max-w-2xl text-lg leading-8 text-body/75">A place to document Bangladesh’s craft traditions, support research and turn knowledge into ideas that help heritage thrive.</p>
          {isResearcher && <Link to={routePaths.researcher} className="mt-7 inline-flex rounded-lg bg-primary px-5 py-3 text-sm font-semibold text-white">Open your workspace →</Link>}
        </div>
        <div className="rounded-2xl border border-border bg-surface p-7">
          <p className="text-xs font-semibold uppercase tracking-widest text-primary">Our focus</p>
          <h2 className="mt-4 text-2xl font-semibold text-heading">Knowledge rooted in people and place.</h2>
          <p className="mt-4 text-sm leading-7 text-body/75">From Dhakai Jamdani and Dhakai Muslin to Rajshahi Silk, the hub connects cultural context, materials and making traditions. References distinguish geographical indications from UNESCO recognition.</p>
        </div>
      </section>
      <section aria-label="What the Innovation Hub does" className="mt-10 grid gap-5 md:grid-cols-3">
        {pillars.map(([number, title, description]) => <article key={number} className="rounded-xl border border-border bg-surface p-7"><p className="text-sm font-semibold text-primary">{number}</p><h2 className="mt-5 text-xl font-semibold text-heading">{title}</h2><p className="mt-3 text-sm leading-7 text-body/75">{description}</p></article>)}
      </section>
      <p className="mt-8 max-w-3xl text-sm leading-7 text-body/65">Research teams use a dedicated workspace for the heritage database, datasets, fieldwork and innovation projects.</p>
    </div>
  );
}
