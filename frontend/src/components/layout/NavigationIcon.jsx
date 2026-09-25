const paths = {
  Marketplace: 'M5 7h14l2 14H3ZM8 8V6a4 4 0 0 1 8 0v2',
  Wishlist: 'M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1.1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 21l8.8-8.6a5.5 5.5 0 0 0 0-7.8Z',
  Products: 'm3 7 9-4 9 4v10l-9 4-9-4Zm0 0 9 4 9-4m-9 4v10M7 5l10 5',
  People: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 3a4 4 0 1 0 0 8 4 4 0 0 0 0-8Zm8 1a4 4 0 0 1 0 8m5 9v-2a4 4 0 0 0-3-3.9',
  Analytics: 'M4 3v18h17M8 16v-4m5 4V7m5 9v-7',
  Security: 'm12 3 9 4v5c0 5-9 9-9 9s-9-4-9-9V7Zm-4 9 3 3 5-6',
  Warehouse: 'm3 8 9-5 9 5v13H3Zm4 13V11h10v10M7 15h10M7 18h10',
  Research: 'M9 3h6m-5 0v6l-6 11h16L14 9V3M8 14h8',

  More: 'M5 12h.01M12 12h.01M19 12h.01',
  Search: 'M21 21l-5-5M10 3a7 7 0 1 0 0 14 7 7 0 0 0 0-14Z',
  Language: 'M3 12h18M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18Zm0 0c-5 5-5 13 0 18m0-18c5 5 5 13 0 18',
  Menu: 'M4 6h16M4 12h16M4 18h16',
  Notifications: 'M5 17h14l-2-4V9a5 5 0 0 0-10 0v4Zm5 3h4',
  Dashboard: 'M3 10 12 3l9 7v11h-6v-7H9v7H3Z',
  'My Bookings': 'M4 5h16v15H4ZM8 3v4m8-4v4M4 10h16m-12 4h3m2 0h3',
  'Travel Passport': 'M5 3h14v18H5ZM8 17h8M12 6a4 4 0 1 0 0 8 4 4 0 0 0 0-8Zm0 0v8m-4-4h8',
  'Heritage Map': 'm3 5 6-2 6 2 6-2v16l-6 2-6-2-6 2Zm6-2v16m6-14v16',
  Festivals: 'M5 3v18M5 4h14l-3 4 3 4H5',
  'Cultural Events': 'M4 5h16v15H4ZM8 3v4m8-4v4M4 10h16m-8 3v4m-2-2h4',
  'Tour Routes': 'M5 18c0-8 14 0 14-9M5 15a3 3 0 1 0 0 6 3 3 0 0 0 0-6ZM19 3a3 3 0 1 0 0 6 3 3 0 0 0 0-6Z',
  'Village Explorer': 'm2 11 6-5 6 5m-10 0v9h8v-9m3-4 3-3 4 4m-6 0v12h4V8',
  'Local Cuisine': 'M5 3v7m4-7v7M3 3v6a4 4 0 0 0 8 0V3M7 13v8m11-18c-4 4-4 9 0 10V3Zm0 10v8',
  'Tourist Services': 'M4 16a8 8 0 0 1 16 0M2 19h20M12 8V5m-2 0h4',
  'Explore Heritage': 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18Zm4 5-3 5-5 3 3-5Z',
  Messages: 'M3 5h18v14H3Zm0 0 9 7 9-7',
  Settings: 'M4 7h16M4 17h16M8 4v6m8 4v6',
};
export default function NavigationIcon({ label }) {
  return <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d={paths[label] || paths[iconCategory(label)] || 'm12 3 2 6 7 3-7 3-2 6-2-6-7-3 7-3Z'} /></svg>;
}

function iconCategory(label = '') {
  if (/wishlist|favorite/i.test(label)) return 'Wishlist';
  if (/analytics|report|insight|monitoring|intelligence/i.test(label)) return 'Analytics';
  if (/warehouse|inventory|stock/i.test(label)) return 'Warehouse';
  if (/security|moderation|approval|permission|verification|fraud/i.test(label)) return 'Security';
  if (/user|supplier|producer|mentor|partner/i.test(label) && !/dashboard/i.test(label)) return 'People';
  if (/research|innovation experiment/i.test(label)) return 'Research';
  if (/dashboard|overview|innovation hub/i.test(label)) return 'Dashboard';
  if (/message|discussion|community|question/i.test(label)) return 'Messages';
  if (/map|district|village|location/i.test(label)) return 'Heritage Map';
  if (/order|booking|contract|procurement|quotation/i.test(label)) return 'My Bookings';
  if (/route|shipment|pickup|delivery/i.test(label)) return 'Tour Routes';
  if (/course|learn|publication|research|certificate|passport/i.test(label)) return 'Travel Passport';
  if (/setting|inventory|stock|profile|admin|policy/i.test(label)) return 'Settings';
  if (/marketplace|auction|procurement/i.test(label)) return 'Marketplace';
  if (/product|craft/i.test(label)) return 'Products';
  if (/event|class|workshop/i.test(label)) return 'Cultural Events';
  return 'Explore Heritage';
}
