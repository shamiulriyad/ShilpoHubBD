// Isolated visual review: real navigation configuration, no account impersonation,
// no API replacements, no sample metrics or business records. Not a production entry point.
import React, { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider, useTheme } from '../../src/contexts/ThemeContext';
import Sidebar from '../../src/components/layout/Sidebar';
import BrandLogo from '../../src/components/brand/BrandLogo';
import { roleSidebars } from '../../src/data/navigation';
import '../../src/styles/index.css';
function Review() {
  const [role,setRole]=useState('Customer');
  const [compact,setCompact]=useState(false);
  const {toggleTheme}=useTheme();
  const config=roleSidebars[role];
  return <div className={`dashboard-shell ${compact?'sidebar-compact':''}`}><header className="workspace-topbar"><BrandLogo/><label className="ml-auto text-sm">Review workspace <select aria-label="Review workspace" value={role} onChange={e=>setRole(e.target.value)} className="rounded-lg border border-border bg-background p-2">{Object.entries(roleSidebars).map(([key,value])=><option key={key} value={key}>{value.title}</option>)}</select></label><button onClick={toggleTheme}>Toggle theme</button></header><div className="workspace-body"><div className="workspace-sidebar"><button className="sidebar-collapse-control" onClick={()=>setCompact(v=>!v)}>{compact?'Expand sidebar':'Collapse sidebar'}</button><Sidebar key={role} items={config.nav} title={config.title} presentationRole={role} compact={compact} onExpand={()=>setCompact(false)}/></div><main className="workspace-content"><h1 className="text-2xl font-semibold">Navigation component review</h1><p className="mt-3 text-sm text-muted">This isolated development page renders the existing role navigation only. It does not sign in, impersonate an account or supply business data.</p><p className="mt-4 text-sm">Open More to verify every existing feature remains accessible.</p></main></div></div>;
}
createRoot(document.getElementById('root')).render(<BrowserRouter><ThemeProvider><Review/></ThemeProvider></BrowserRouter>);
