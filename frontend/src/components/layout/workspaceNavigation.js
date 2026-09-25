// Presentation only: every destination comes from the existing role navigation.
export const priorities = {
  Customer: ['Dashboard', 'Browse Marketplace', 'Order History', 'Wishlist', 'Messages'],
  Producer: ['Dashboard', 'Orders & Fulfillment', 'My Products', 'Inventory', 'Contracts'],
  BusinessPartner: ['Dashboard', 'Supplier Discovery', 'Procurement', 'Contracts', 'Analytics'],
  Tourist: ['Dashboard', 'Heritage Map', 'My Bookings', 'Village Explorer', 'Travel Passport'],
  HeritageAcademyMember: ['My Dashboard', 'Learning Dashboard', 'Course Catalog', 'Live Classes', 'Certificates'],
  HeritageInnovationHub: ['Innovation Hub', 'Research Workspace', 'Heritage Database', 'Publications'],
  GovernmentNGO: ['Government Dashboard', 'Reports & Forecasts', 'Policy & Compliance', 'Funding & Grants'],
  LogisticsPartner: ['Logistics Dashboard', 'Shipments', 'Pickup Requests', 'Warehouses', 'Delivery Routes'],
  SuperAdmin: ['Dashboard', 'User directory', 'Product Approval', 'Content Moderation', 'System Monitoring'],
};
export function presentNavigation(items, role) {
  const groups = items[0]?.items ? items : [{ section: 'Workspace', items }];
  const all = groups.flatMap(group => group.items);
  const wanted = priorities[role] || [];
  const primary = wanted.map(label => all.find(item => item.label === label)).filter(Boolean);
  if (!primary.length) primary.push(...all.slice(0, 4));
  const paths = new Set(primary.map(item => item.path));
  const secondary = groups.map(group => ({ ...group, items: group.items.filter(item => !paths.has(item.path)) })).filter(group => group.items.length);
  return { primary, secondary, all };
}
