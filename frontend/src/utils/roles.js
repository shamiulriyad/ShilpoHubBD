import { routePaths } from '../routes/routePaths';

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

export const roleHomePath = (role) => ROLE_HOME[role] || routePaths.dashboard;
