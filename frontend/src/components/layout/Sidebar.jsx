import { useLayoutEffect, useState } from 'react';
import { NavLink, Link } from 'react-router-dom';
import {
  House, Compass, Handshake, Bot, ChartColumn, ShoppingBag, Map, Bell, Package,
  FilePenLine, FileText, Wallet, Factory, Mail, Receipt, Wrench, ShoppingCart,
  Stamp, Image, ScrollText, BookOpen, TrendingUp, MessageCircle, Users, Amphora,
  Building, Landmark, TentTree, Target, Palette, Video, Medal, Sprout, Settings,
  Scale, Undo2, Briefcase, Brain, GraduationCap, HandHelping, Heart, Shield,
  BellRing, Siren, Truck, Mic, Archive, FileArchive, Microscope, Hammer, Search,
  Satellite, MapPin, Calendar, HandCoins, Award, Ticket, PartyPopper, Gift, Soup,
  CircleQuestionMark, PenLine, Circle, ChevronLeft, ChevronRight,
} from 'lucide-react';
import { useAuth } from '../../hooks/useAuth';
import { roleLabel } from '../../utils/roles';
import { routePaths } from '../../routes/routePaths';

const STORAGE_KEY = 'sh:sidebar:v2';
const WIDTH_EXPANDED = '260px';
const WIDTH_COLLAPSED = '76px';

// Every glyph used across data/navigation.js, mapped to a matching monochrome
// Lucide icon so the sidebar reads as one clean icon set instead of mixed emoji.
const ICON_MAP = {
  '🏠': House, '🧭': Compass, '🤝': Handshake, '🤖': Bot, '📊': ChartColumn,
  '🛍️': ShoppingBag, '🗺️': Map, '🔔': Bell, '📦': Package, '📝': FilePenLine,
  '📄': FileText, '💰': Wallet, '🏭': Factory, '✉️': Mail, '🧾': Receipt,
  '🛠️': Wrench, '🛒': ShoppingCart, '🛂': Stamp, '🖼️': Image, '📜': ScrollText,
  '📚': BookOpen, '📈': TrendingUp, '💬': MessageCircle, '👥': Users, '🏺': Amphora,
  '🏢': Building, '🏘️': TentTree, '🎯': Target, '🎨': Palette, '🎥': Video,
  '🎖️': Medal, '🌱': Sprout, '⚙️': Settings, '⚖️': Scale, '↩️': Undo2,
  '🧳': Briefcase, '🧠': Brain, '🧑‍🏫': GraduationCap, '🤲': HandHelping, '🤍': Heart,
  '🛡️': Shield, '🛎️': BellRing, '🚨': Siren, '🚚': Truck, '🗣️': Mic,
  '🗄️': Archive, '🗃️': FileArchive, '🔬': Microscope, '🔨': Hammer, '🔍': Search,
  '📡': Satellite, '📍': MapPin, '📅': Calendar, '💼': Briefcase, '💸': HandCoins,
  '🏛️': Landmark, '🏅': Award, '🎫': Ticket, '🎓': GraduationCap, '🎉': PartyPopper,
  '🎁': Gift, '🍲': Soup, '❓': CircleQuestionMark, '✍️': PenLine,
};

function NavIcon({ glyph, className = 'h-5 w-5' }) {
  const Icon = ICON_MAP[glyph] || Circle;
  return <Icon className={className} strokeWidth={1.75} aria-hidden="true" />;
}

function readState() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY) || '{}') || {};
  } catch {
    return {};
  }
}

function writeState(next) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  } catch {
    /* storage unavailable — ignore */
  }
}

<<<<<<< HEAD
function NavItem({ item, onNavigate }) {
=======
function NavItem({ item, collapsed }) {
>>>>>>> Riyad
  return (
    <NavLink
      to={item.path}
      end
<<<<<<< HEAD
      onClick={onNavigate}
=======
      title={collapsed ? item.label : undefined}
>>>>>>> Riyad
      className={({ isActive }) =>
        `flex items-center gap-3 rounded-lg py-2.5 text-sm font-medium transition-colors duration-200 ease-in-out ${
          collapsed ? 'justify-center px-0' : 'px-3'
        } ${isActive ? 'bg-primary-soft text-primary' : 'text-[#786C66] hover:bg-primary-soft/50 hover:text-title'}`
      }
    >
      <NavIcon glyph={item.icon} className="h-5 w-5 shrink-0" />
      {!collapsed && <span className="truncate">{item.label}</span>}
    </NavLink>
  );
}

<<<<<<< HEAD
function FlatNav({ items, onNavigate }) {
  return (
    <nav className="space-y-1">
      {items.map((item) => (
        <NavItem key={item.label} item={item} onNavigate={onNavigate} />
=======
function FlatNav({ items, collapsed }) {
  return (
    <nav className="space-y-1">
      {items.map((item) => (
        <NavItem key={item.label} item={item} collapsed={collapsed} />
>>>>>>> Riyad
      ))}
    </nav>
  );
}

<<<<<<< HEAD
function GroupedNav({ groups, onNavigate }) {
  const [collapsed, setCollapsed] = useState(readCollapsed);
=======
function GroupedNav({ groups, collapsed }) {
  const [collapsedGroups, setCollapsedGroups] = useState(() => readState().collapsedGroups ?? {});
>>>>>>> Riyad

  const toggle = (section) => {
    setCollapsedGroups((prev) => {
      const next = { ...prev, [section]: !prev[section] };
      writeState({ ...readState(), collapsedGroups: next });
      return next;
    });
  };

  return (
    <nav className="space-y-4">
      {groups.map((group) => {
        const isGroupCollapsed = Boolean(collapsedGroups[group.section]);
        return (
          <div key={group.section}>
            {!collapsed && (
              <button
                type="button"
                onClick={() => toggle(group.section)}
                className="mb-1 flex w-full items-center justify-between px-3 text-[11px] font-semibold uppercase tracking-wider text-[#786C66]/70 hover:text-title"
              >
                <span>{group.section}</span>
                <ChevronRight className={`h-3 w-3 transition-transform duration-200 ${isGroupCollapsed ? '' : 'rotate-90'}`} strokeWidth={2} />
              </button>
            )}
            {(collapsed || !isGroupCollapsed) && (
              <div className="space-y-1">
                {group.items.map((item) => (
<<<<<<< HEAD
                  <NavItem key={item.label} item={item} onNavigate={onNavigate} />
=======
                  <NavItem key={item.label} item={item} collapsed={collapsed} />
>>>>>>> Riyad
                ))}
              </div>
            )}
          </div>
        );
      })}
    </nav>
  );
}

export default function Sidebar({ items = [], title = 'Menu', className = '', onNavigate }) {
  const grouped = items.length > 0 && Array.isArray(items[0]?.items);
  const { activeRole } = useAuth();
  const [collapsed, setCollapsed] = useState(() => readState().collapsed ?? false);

  // Keep the page content offset (see DashboardLayout) in sync with the
  // sidebar's current width, since it's fixed/out-of-flow at lg+.
  useLayoutEffect(() => {
    document.documentElement.style.setProperty('--sh-sidebar-w', collapsed ? WIDTH_COLLAPSED : WIDTH_EXPANDED);
  }, [collapsed]);

  const toggleCollapsed = () => {
    setCollapsed((prev) => {
      const next = !prev;
      writeState({ ...readState(), collapsed: next });
      return next;
    });
  };

  return (
    <aside
      style={{ '--w': collapsed ? WIDTH_COLLAPSED : WIDTH_EXPANDED }}
      className={`flex h-full w-full shrink-0 flex-col border-r border-border bg-surface transition-[width] duration-200 ease-in-out lg:fixed lg:inset-y-0 lg:left-0 lg:z-40 lg:h-screen lg:w-[var(--w)] ${className}`}
    >
      <div className={`flex items-center gap-2.5 border-b border-border px-4 py-4 ${collapsed ? 'justify-center px-2' : ''}`}>
        <Link to={routePaths.home} className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary text-xs font-bold text-surface">
          SH
        </Link>
        {!collapsed && <span className="truncate text-base font-bold text-title">ShilpoHub</span>}
      </div>

<<<<<<< HEAD
      {grouped ? <GroupedNav groups={items} onNavigate={onNavigate} /> : <FlatNav items={items} onNavigate={onNavigate} />}
=======
      {!collapsed && (
        <div className="border-b border-border px-4 py-3">
          <p className="truncate text-sm font-semibold text-heading">{title} workspace</p>
          {activeRole && <p className="truncate text-[11px] font-medium text-primary">{roleLabel(activeRole)}</p>}
        </div>
      )}

      <div className={`flex-1 overflow-y-auto py-4 ${collapsed ? 'px-2' : 'px-3'}`}>
        {grouped ? <GroupedNav groups={items} collapsed={collapsed} /> : <FlatNav items={items} collapsed={collapsed} />}
      </div>

      <div className="border-t border-border p-2">
        <button
          type="button"
          onClick={toggleCollapsed}
          className={`flex w-full items-center gap-2 rounded-lg py-2.5 text-sm font-medium text-[#786C66] transition-colors duration-200 ease-in-out hover:bg-primary-soft/50 hover:text-title ${
            collapsed ? 'justify-center px-0' : 'px-3'
          }`}
        >
          {collapsed ? <ChevronRight className="h-5 w-5" strokeWidth={1.75} /> : <ChevronLeft className="h-5 w-5" strokeWidth={1.75} />}
          {!collapsed && <span>Collapse</span>}
        </button>
      </div>
>>>>>>> Riyad
    </aside>
  );
}
