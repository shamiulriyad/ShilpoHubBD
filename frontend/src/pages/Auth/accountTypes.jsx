function Icon({ children }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      className="h-6 w-6"
      aria-hidden="true"
    >
      {children}
    </svg>
  );
}

const CustomerIcon = () => (
  <Icon>
    <path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4" />
    <path d="M3 6h18" />
    <path d="M16 10a4 4 0 0 1-8 0" />
  </Icon>
);

const ProducerIcon = () => (
  <Icon>
    <path d="M3 21V9l6-4 6 4v12" />
    <path d="M15 21V13h4a2 2 0 0 1 2 2v6" />
    <path d="M9 21v-4h2v4" />
  </Icon>
);

const BusinessPartnerIcon = () => (
  <Icon>
    <path d="m11 17 2 2 4-4" />
    <path d="M8.5 21H5a2 2 0 0 1-2-2v-2a4 4 0 0 1 4-4h1" />
    <circle cx="7" cy="7" r="3" />
    <path d="M16.5 8.5a3 3 0 1 0-3-3" />
  </Icon>
);

const TouristIcon = () => (
  <Icon>
    <circle cx="12" cy="12" r="9" />
    <path d="m14.5 9.5-2 5-5 2 2-5 5-2Z" />
  </Icon>
);

const HeritageAcademyIcon = () => (
  <Icon>
    <path d="m2 9 10-5 10 5-10 5-10-5Z" />
    <path d="M6 11.5V16c0 1.5 2.5 3 6 3s6-1.5 6-3v-4.5" />
    <path d="M22 9v6" />
  </Icon>
);

const HeritageInnovationIcon = () => (
  <Icon>
    <path d="M9 18h6" />
    <path d="M10 22h4" />
    <path d="M12 2a6 6 0 0 0-4 10.5c.6.5 1 1.3 1 2.1V16h6v-1.4c0-.8.4-1.6 1-2.1A6 6 0 0 0 12 2Z" />
  </Icon>
);

const LogisticsPartnerIcon = () => (
  <Icon>
    <rect x="1" y="7" width="14" height="10" rx="1" />
    <path d="M15 10h4l3 3v4h-7" />
    <circle cx="6" cy="19" r="2" />
    <circle cx="17" cy="19" r="2" />
  </Icon>
);

const GovernmentNGOIcon = () => (
  <Icon>
    <path d="M3 21h18" />
    <path d="M5 21V9l7-5 7 5v12" />
    <path d="M9 21v-6h6v6" />
    <path d="M9 12h.01M15 12h.01" />
  </Icon>
);

export const ACCOUNT_TYPES = [
  {
    id: 'Customer',
    label: 'Customer',
    description: 'Buy authentic heritage products.',
    Icon: CustomerIcon,
  },
  {
    id: 'Producer',
    label: 'Producer',
    description: 'Sell and manage handmade or agricultural products.',
    Icon: ProducerIcon,
  },
  {
    id: 'BusinessPartner',
    label: 'Business Partner',
    description: 'Purchase products in bulk and collaborate with producers.',
    Icon: BusinessPartnerIcon,
  },
  {
    id: 'Tourist',
    label: 'Tourist',
    description: 'Explore heritage places and book experiences.',
    Icon: TouristIcon,
  },
  {
    id: 'HeritageAcademyMember',
    label: 'Heritage Academy Member',
    description: 'Learn heritage skills and earn certificates.',
    Icon: HeritageAcademyIcon,
  },
  {
    id: 'HeritageInnovationHub',
    label: 'Heritage Innovation Hub',
    description: 'Research, innovation, and heritage knowledge.',
    Icon: HeritageInnovationIcon,
  },
  {
    id: 'LogisticsPartner',
    label: 'Logistics Partner',
    description: 'Manage pickups, warehouses, and deliveries.',
    Icon: LogisticsPartnerIcon,
  },
  {
    id: 'GovernmentNGO',
    label: 'Government & NGO',
    description: 'Manage heritage programs and national initiatives.',
    Icon: GovernmentNGOIcon,
    approvalRequired: true,
  },
];
