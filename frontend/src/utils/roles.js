import { routePaths } from '../routes/routePaths';

// The nine roles the backend actually issues (ShilpoHubBD.Domain.Constants.RoleNames).
// Keep this list in sync with the backend — nothing here is invented on the client.
export const ROLES = {
  Customer: 'Customer',
  Producer: 'Producer',
  BusinessPartner: 'BusinessPartner',
  Tourist: 'Tourist',
  HeritageAcademyMember: 'HeritageAcademyMember',
  HeritageInnovationHub: 'HeritageInnovationHub',
  GovernmentNGO: 'GovernmentNGO',
  LogisticsPartner: 'LogisticsPartner',
  SuperAdmin: 'SuperAdmin',
};

export const ALL_ROLES = Object.values(ROLES);

// Backend role name -> human label shown in the UI.
export const ROLE_LABELS = {
  Customer: 'Customer',
  Producer: 'Producer',
  BusinessPartner: 'Business Partner',
  Tourist: 'Tourist',
  HeritageAcademyMember: 'Heritage Academy Member',
  HeritageInnovationHub: 'Heritage Innovation Hub',
  GovernmentNGO: 'Government & NGO',
  LogisticsPartner: 'Logistics Partner',
  SuperAdmin: 'Administrator',
};

// Backend role name -> that role's dashboard landing route.
const ROLE_HOME = {
  Customer: routePaths.customer,
  Producer: routePaths.producer,
  BusinessPartner: routePaths.businessPartner,
  Tourist: routePaths.tourist,
  HeritageAcademyMember: routePaths.academyMember,
  HeritageInnovationHub: routePaths.researcher,
  GovernmentNGO: routePaths.government,
  LogisticsPartner: routePaths.logisticsPartner,
  SuperAdmin: routePaths.admin,
};

export const roleLabel = (role) => ROLE_LABELS[role] || role || 'Member';

// The single rule for turning an auth response into an *effective* role.
// The backend only sets `activeRole` when a user holds exactly one role; a
// multi-role user comes back with `activeRole: null`. In that case we fall back
// to the first granted role so routing/sidebar/dashboard are always driven by a
// concrete role. We never invent a role and never default to "Customer" — a user
// with no roles at all resolves to `null`.
export const resolveActiveRole = (roles = [], activeRole = null) => {
  if (activeRole && roles.includes(activeRole)) return activeRole;
  return roles[0] ?? null;
};

// Where a signed-in user should land. Falls back to the shared dashboard shell
// only when the role is genuinely unknown — never to a specific role's area.
export const roleHomePath = (role) => ROLE_HOME[role] || routePaths.dashboard;

// True when `userRoles` satisfies an `allowedRoles` list (empty list = any role).
export const matchesAllowedRoles = (userRoles = [], allowedRoles = []) => {
  if (allowedRoles.length === 0) return true;
  return allowedRoles.some((role) => userRoles.includes(role));
};
