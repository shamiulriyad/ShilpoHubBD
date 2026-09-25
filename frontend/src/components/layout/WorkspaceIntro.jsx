import SafeImage from '../media/SafeImage';
import { Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { roleSidebars } from '../../data/navigation';
import { presentNavigation } from './workspaceNavigation';
const stories = {
  Customer: ['Discover · Support · Preserve', 'Heritage for everyday living.', '/images/hero-weaver.png'],
  Producer: ['Your craft. Your business.', 'Make room for your next chapter.', '/images/loom-photo.jpg'],
  BusinessPartner: ['Relationships that build value', 'Better partnerships start with people.'],
  Tourist: ['Go beyond the familiar', 'A journey into living heritage.', '/images/bangladesh-river.jpg'],
  HeritageAcademyMember: ['Keep curiosity alive', 'Learn a skill. Carry a tradition.', '/images/learning-together.jpg'],
  HeritageInnovationHub: ['Evidence · Research · Preservation', 'Connect knowledge with possibility.'],
  GovernmentNGO: ['A national perspective', 'Turn heritage intelligence into action.'],
  LogisticsPartner: ['Every delivery connects a community', 'Keep craft moving.'],
  SuperAdmin: ['Platform operations', 'Clarity for every decision.'],
};
export default function WorkspaceIntro() {
  const { activeRole,user } = useAuth();
  const story=stories[activeRole];
  if(!story)return null;
  const links=presentNavigation(roleSidebars[activeRole]?.nav || [],activeRole).primary.slice(1,4);
  return <section className={`workspace-intro ${story[2] ? 'has-photo' : ''}`} aria-label="Workspace overview">
    <div className="workspace-intro-copy"><p className="intro-kicker">{story[0]}</p><h2>{story[1]}</h2><p className="intro-greeting">Welcome{user?.name ? `, ${user.name}` : ' back'}. Your workspace, at a glance.</p><div className="intro-actions">{links.map(item=><Link key={item.path} to={item.path}>{item.label}<span aria-hidden="true">↗</span></Link>)}</div></div>
    {story[2] ? <SafeImage src={story[2]} alt="" className="workspace-intro-photo"/> : <div className="workspace-intro-pattern" aria-hidden="true"><span/><span/><span/><span/></div>}
  </section>;
}
