import { useState } from 'react';
import NavigationIcon from './NavigationIcon';
export default function LanguageMenu() {
  const [open,setOpen]=useState(false);
  return <div className="language-menu" onBlur={event=>{if(!event.currentTarget.contains(event.relatedTarget))setOpen(false);}} onKeyDown={event=>{if(event.key==='Escape')setOpen(false);}}>
    <button type="button" className="language-trigger" aria-label="Language: English" aria-expanded={open} onClick={()=>setOpen(value=>!value)}><NavigationIcon label="Language"/><span>EN</span></button>
    {open && <div className="language-popover"><p className="menu-eyebrow">Interface language</p><button type="button" className="search-result w-full" onClick={()=>setOpen(false)}><span>English</span><span aria-label="Current language">✓</span></button><p className="px-3 pb-2 pt-1 text-xs leading-5 text-muted">English is currently available.</p></div>}
  </div>;
}
