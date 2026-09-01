import { routePaths } from '../routes/routePaths';

export const mainNav = [
  { label: 'Home', path: routePaths.home },
  { label: 'Explore', path: routePaths.explore, menu: 'explore' },
  { label: 'Marketplace', path: routePaths.marketplace, menu: 'marketplace' },
  { label: 'Tourism', path: routePaths.tourism, menu: 'tourism' },
  { label: 'Academy', path: routePaths.academy, menu: 'academy' },
  { label: 'Innovation Hub', path: routePaths.research, menu: 'research' },
  { label: 'About', path: routePaths.about },
  { label: 'News', path: routePaths.news },
];

export const megaMenus = {
  explore: {
    heading: 'Explore Heritage',
    description: 'Discover the districts, villages, crafts and people behind Bangladesh’s living heritage.',
    links: [
      { label: 'Districts', description: 'Browse heritage by district', path: routePaths.exploreDistricts },
      { label: 'Heritage Villages', description: 'Craft villages and their stories', path: routePaths.exploreVillages },
      { label: 'Crafts', description: 'Traditional craft disciplines', path: routePaths.exploreCrafts },
      { label: 'Producers', description: 'Artisans, farmers & makers', path: routePaths.exploreProducers },
      { label: 'UNESCO Heritage', description: 'Recognized intangible heritage', path: routePaths.exploreUnesco },
      { label: 'Digital Museum', description: 'Curated heritage collections', path: routePaths.exploreMuseum },
    ],
  },
  marketplace: {
    heading: 'Marketplace',
    description: 'Shop authentic heritage products directly from verified producers.',
    links: [
      { label: 'Products', description: 'Browse the full catalog', path: routePaths.marketplaceProducts },
      { label: 'Categories', description: 'Shop by craft category', path: routePaths.marketplaceCategories },
      { label: 'Featured Products', description: 'Curated picks of the season', path: routePaths.marketplaceProducts },
      { label: 'Featured Producers', description: 'Meet top-rated makers', path: routePaths.exploreProducers },
      { label: 'Auctions', description: 'Bid on rare and limited pieces', path: routePaths.marketplaceAuctions },
    ],
  },
  tourism: {
    heading: 'Tourism',
    description: 'Plan heritage journeys across Bangladesh.',
    links: [
      { label: 'Heritage Map', description: 'Interactive heritage locations', path: routePaths.tourismMap },
      { label: 'Festivals', description: 'Cultural festival directory', path: routePaths.tourismFestivals },
      { label: 'Cultural Events', description: 'Upcoming events calendar', path: routePaths.tourismEvents },
      { label: 'Tour Routes', description: 'Guided heritage travel routes', path: routePaths.tourismRoutes },
      { label: 'Local Cuisine', description: 'Traditional dishes and where to try them', path: routePaths.tourismCuisines },
      { label: 'Tourist Services', description: 'Book guides, workshops, homestays and transport', path: routePaths.tourismServices },
      { label: 'AI Trip Planner', description: 'Get an AI-generated day-by-day itinerary', path: routePaths.tourismAiPlanner },
    ],
  },
  academy: {
    heading: 'Academy',
    description: 'Learn traditional crafts from master artisans.',
    links: [
      { label: 'Courses', description: 'Browse the course catalog', path: routePaths.academy },
      { label: 'Mentors', description: 'Master artisans & trainers', path: routePaths.academyMentors },
      { label: 'Certifications', description: 'Recognized skill certificates', path: routePaths.academyCertifications },
      { label: 'Live Classes', description: 'Join live sessions with mentors', path: routePaths.academyLiveClasses },
    ],
  },
  research: {
    heading: 'Innovation Hub',
    description: 'Research, publications and open heritage data.',
    links: [
      { label: 'Research', description: 'Research workspace & projects', path: routePaths.researchWorkspace },
      { label: 'Publications', description: 'Papers, reports & case studies', path: routePaths.researchPublications },
      { label: 'Heritage Database', description: 'Open heritage datasets', path: routePaths.researchHeritageDatabase },
    ],
  },
};

export const userMenu = [
  { label: 'Profile', path: routePaths.dashboardProfile },
  { label: 'Dashboard', path: routePaths.dashboard },
  { label: 'Notifications', path: routePaths.dashboardNotifications },
  { label: 'Messages', path: routePaths.dashboardMessages },
  { label: 'Settings', path: routePaths.dashboardSettings },
];

// Generic member workspace (shared /dashboard/* area and role landing pages).
export const sidebarNav = [
  {
    section: 'Overview',
    items: [
      { label: 'Dashboard', path: routePaths.dashboard, icon: '🏠' },
      { label: 'Analytics', path: routePaths.dashboardAnalytics, icon: '📊' },
    ],
  },
  {
    section: 'Discover',
    items: [
      { label: 'Explore', path: routePaths.dashboardExplore, icon: '🧭' },
      { label: 'Marketplace', path: routePaths.dashboardMarketplace, icon: '🛍️' },
      { label: 'Tourism', path: routePaths.dashboardTourism, icon: '🗺️' },
    ],
  },
  {
    section: 'Learning',
    items: [
      { label: 'Academy', path: routePaths.dashboardAcademy, icon: '🎓' },
      { label: 'My Learning', path: routePaths.academyLearning, icon: '📚' },
      { label: 'Skill Assessments', path: routePaths.academySkillAssessments, icon: '📝' },
      { label: 'Certificates', path: routePaths.academyCertificates, icon: '📜' },
      { label: 'Portfolio', path: routePaths.academyPortfolio, icon: '🖼️' },
      { label: 'Learning Roadmap', path: routePaths.academyRoadmap, icon: '🧭' },
      { label: 'Mentorship Requests', path: routePaths.academyMentorshipRequests, icon: '🤝' },
      { label: 'AI Mentor Matching', path: routePaths.academyMentorMatching, icon: '🎯' },
    ],
  },
  {
    section: 'Connect',
    items: [
      { label: 'Community', path: routePaths.dashboardCommunity, icon: '💬' },
      { label: 'Messages', path: routePaths.dashboardMessages, icon: '✉️' },
      { label: 'Notifications', path: routePaths.dashboardNotifications, icon: '🔔' },
    ],
  },
  {
    section: 'Account',
    items: [{ label: 'Settings', path: routePaths.dashboardSettings, icon: '⚙️' }],
  },
];

// Grouped ("premium") sidebar for the Customer workspace. Every param-free Customer
// feature route from backendsetup.md is represented here; product-scoped views
// (craft/producer story, 360°, QR, traceability, AI interior/fashion/similar) are
// reached from a product page and intentionally omitted.
export const customerSidebarNav = [
  {
    section: 'Overview',
    items: [{ label: 'Dashboard', path: routePaths.customer, icon: '🏠' }],
  },
  {
    section: 'Marketplace',
    items: [
      { label: 'Browse Marketplace', path: routePaths.customerMarketplace, icon: '🛍️' },
      { label: 'Custom Orders', path: routePaths.customerCustomOrder, icon: '✍️' },
      { label: 'Auctions', path: routePaths.customerAuctions, icon: '🔨' },
      { label: 'Workshops & Live', path: routePaths.customerWorkshops, icon: '🎥' },
    ],
  },
  {
    section: 'Shopping',
    items: [
      { label: 'Wishlist', path: routePaths.customerWishlist, icon: '🤍' },
      { label: 'Cart', path: routePaths.customerCart, icon: '🛒' },
    ],
  },
  {
    section: 'Orders',
    items: [
      { label: 'Order History', path: routePaths.customerOrders, icon: '📦' },
      { label: 'Returns', path: routePaths.customerReturns, icon: '↩️' },
      { label: 'Refunds', path: routePaths.customerRefunds, icon: '💸' },
      { label: 'Saved Addresses', path: routePaths.customerAddresses, icon: '📍' },
      { label: 'Notifications', path: routePaths.customerNotifications, icon: '🔔' },
    ],
  },
  {
    section: 'Community',
    items: [
      { label: 'Community Feed', path: routePaths.customerCommunity, icon: '💬' },
      { label: 'Discussion Forum', path: routePaths.customerForum, icon: '🗣️' },
      { label: 'Questions & Answers', path: routePaths.customerQA, icon: '❓' },
      { label: 'Messages', path: routePaths.customerMessages, icon: '✉️' },
      { label: 'Following Producers', path: routePaths.customerFollowing, icon: '👥' },
      { label: 'Favorite Villages', path: routePaths.customerFavoriteVillages, icon: '🏘️' },
    ],
  },
  {
    section: 'Heritage Passport',
    items: [
      { label: 'Heritage Collection', path: routePaths.customerHeritageCollection, icon: '🏺' },
      { label: 'Heritage Passport', path: routePaths.customerHeritagePassport, icon: '🛂' },
      { label: 'Achievements', path: routePaths.customerAchievements, icon: '🏅' },
      { label: 'Badge Collection', path: routePaths.customerBadges, icon: '🎖️' },
    ],
  },
  {
    section: 'Insights',
    items: [
      { label: 'Purchase Analytics', path: routePaths.customerPurchaseAnalytics, icon: '📊' },
      { label: 'Impact Dashboard', path: routePaths.customerImpactDashboard, icon: '🌱' },
    ],
  },
  {
    section: 'AI Shopping',
    items: [{ label: 'Gift Recommendation', path: routePaths.customerAIGiftRecommendation, icon: '🎁' }],
  },
];

// Producer workspace — production, fulfilment, B2B partnerships & growth.
export const producerSidebarNav = [
  {
    section: 'Overview',
    items: [{ label: 'Dashboard', path: routePaths.producer, icon: '🏠' }],
  },
  {
    section: 'Sell & Fulfil',
    items: [
      { label: 'Orders & Fulfillment', path: routePaths.producerOrders, icon: '📦' },
      { label: 'Inventory', path: routePaths.producerInventory, icon: '🗃️' },
    ],
  },
  {
    section: 'Partnerships',
    items: [
      { label: 'Contracts', path: routePaths.producerContracts, icon: '📄' },
      { label: 'Quotation Requests', path: routePaths.producerQuotations, icon: '🧾' },
      { label: 'Manufacturing Partnerships', path: routePaths.producerPartnerships, icon: '🏭' },
      { label: 'Design Collaborations', path: routePaths.producerDesignCollaborations, icon: '🎨' },
      { label: 'Product Development', path: routePaths.producerProductDevelopment, icon: '🛠️' },
    ],
  },
  {
    section: 'Growth',
    items: [
      { label: 'CSR Sponsorship', path: routePaths.producerCsr, icon: '🤝' },
      { label: 'Investment Opportunities', path: routePaths.producerInvestments, icon: '💰' },
      { label: 'Sustainability Profile', path: routePaths.producerSustainability, icon: '🌱' },
    ],
  },
  {
    section: 'AI',
    items: [{ label: 'AI Business Assistant', path: routePaths.producerAiAssistant, icon: '🤖' }],
  },
];

// Business Partner workspace — sourcing, deals, marketplaces & intelligence.
export const businessPartnerSidebarNav = [
  {
    section: 'Overview',
    items: [
      { label: 'Dashboard', path: routePaths.businessPartner, icon: '🏠' },
      { label: 'Company Profile', path: routePaths.businessPartnerProfile, icon: '🏢' },
    ],
  },
  {
    section: 'Sourcing',
    items: [
      { label: 'Supplier Discovery', path: routePaths.businessPartnerSupplierDiscovery, icon: '🔍' },
      { label: 'Supplier Matching (AI)', path: routePaths.businessPartnerSupplierMatching, icon: '🧠' },
      { label: 'Compare Producers', path: routePaths.businessPartnerProducerComparison, icon: '⚖️' },
      { label: 'Procurement', path: routePaths.businessPartnerProcurements, icon: '🛒' },
    ],
  },
  {
    section: 'Deals',
    items: [
      { label: 'Contracts', path: routePaths.businessPartnerContracts, icon: '📄' },
      { label: 'Quotations', path: routePaths.businessPartnerQuotations, icon: '🧾' },
      { label: 'Manufacturing Partnerships', path: routePaths.businessPartnerPartnerships, icon: '🏭' },
      { label: 'Design Collaborations', path: routePaths.businessPartnerDesignCollaborations, icon: '🎨' },
      { label: 'Product Development', path: routePaths.businessPartnerProductDevelopment, icon: '🛠️' },
    ],
  },
  {
    section: 'Marketplaces',
    items: [
      { label: 'Sponsorship Marketplace', path: routePaths.businessPartnerCsr, icon: '🤝' },
      { label: 'Investment Marketplace', path: routePaths.businessPartnerInvestments, icon: '💰' },
    ],
  },
  {
    section: 'Intelligence',
    items: [
      { label: 'Analytics', path: routePaths.businessPartnerAnalytics, icon: '📊' },
      { label: 'AI Intelligence', path: routePaths.businessPartnerAiIntelligence, icon: '🤖' },
    ],
  },
];

// Admin / platform-operations workspace.
export const adminSidebarNav = [
  {
    section: 'Overview',
    items: [{ label: 'Dashboard', path: routePaths.admin, icon: '🏠' }],
  },
  {
    section: 'Management',
    items: [
      { label: 'User Management', path: routePaths.adminUsers, icon: '👥' },
      { label: 'Heritage Management', path: routePaths.adminHeritage, icon: '🏺' },
      { label: 'Marketplace Monitoring', path: routePaths.adminMarketplace, icon: '🛍️' },
    ],
  },
  {
    section: 'Platform',
    items: [
      { label: 'CMS', path: routePaths.adminCms, icon: '📝' },
      { label: 'Security Center', path: routePaths.adminSecurity, icon: '🛡️' },
    ],
  },
];

// Items every signed-in member shares. Kept in one place so each role sidebar can
// spread it in without the role-specific groups ever overlapping.
const generalGroup = {
  section: 'General',
  items: [
    { label: 'Explore Heritage', path: routePaths.explore, icon: '🧭' },
    { label: 'Messages', path: routePaths.dashboardMessages, icon: '✉️' },
    { label: 'Notifications', path: routePaths.dashboardNotifications, icon: '🔔' },
    { label: 'Settings', path: routePaths.dashboardSettings, icon: '⚙️' },
  ],
};

// Tourist workspace — heritage travel: discovery, planning, bookings.
export const touristSidebarNav = [
  {
    section: 'Overview',
    items: [
      { label: 'Dashboard', path: routePaths.tourist, icon: '🏠' },
      { label: 'My Bookings', path: routePaths.tourismBookings, icon: '🎫' },
      { label: 'Travel Passport', path: routePaths.tourismPassport, icon: '🛂' },
    ],
  },
  {
    section: 'Discover',
    items: [
      { label: 'Heritage Map', path: routePaths.tourismMap, icon: '🗺️' },
      { label: 'Festivals', path: routePaths.tourismFestivals, icon: '🎉' },
      { label: 'Cultural Events', path: routePaths.tourismEvents, icon: '📅' },
      { label: 'Tour Routes', path: routePaths.tourismRoutes, icon: '🧳' },
      { label: 'Village Explorer', path: routePaths.tourismVillages, icon: '🏘️' },
      { label: 'Local Cuisine', path: routePaths.tourismCuisines, icon: '🍲' },
      { label: 'Tourist Services', path: routePaths.tourismServices, icon: '🛎️' },
    ],
  },
  {
    section: 'Plan',
    items: [{ label: 'AI Trip Planner', path: routePaths.tourismAiPlanner, icon: '🤖' }],
  },
  generalGroup,
];

// Heritage Academy Member workspace — learning, practice, credentials.
export const academyMemberSidebarNav = [
  {
    section: 'Overview',
    items: [
      { label: 'My Dashboard', path: routePaths.academyMember, icon: '🏠' },
      { label: 'Learning Dashboard', path: routePaths.academyLearning, icon: '📈' },
    ],
  },
  {
    section: 'Learn',
    items: [
      { label: 'Course Catalog', path: routePaths.academy, icon: '📚' },
      { label: 'Mentors', path: routePaths.academyMentors, icon: '🧑‍🏫' },
      { label: 'Live Classes', path: routePaths.academyLiveClasses, icon: '🎥' },
      { label: 'Skill Assessments', path: routePaths.academySkillAssessments, icon: '📝' },
    ],
  },
  {
    section: 'My Progress',
    items: [
      { label: 'Certifications', path: routePaths.academyCertifications, icon: '📜' },
      { label: 'Certificates', path: routePaths.academyCertificates, icon: '🎖️' },
      { label: 'Portfolio', path: routePaths.academyPortfolio, icon: '🖼️' },
      { label: 'Learning Roadmap', path: routePaths.academyRoadmap, icon: '🧭' },
      { label: 'Mentorship Requests', path: routePaths.academyMentorshipRequests, icon: '🤝' },
      { label: 'AI Mentor Matching', path: routePaths.academyMentorMatching, icon: '🎯' },
    ],
  },
  generalGroup,
];

// Heritage Innovation Hub workspace — research projects, publications, open data.
export const innovationHubSidebarNav = [
  {
    section: 'Overview',
    items: [{ label: 'Innovation Hub', path: routePaths.researcher, icon: '🏠' }],
  },
  {
    section: 'Research',
    items: [
      { label: 'Research Workspace', path: routePaths.researchWorkspace, icon: '🔬' },
      { label: 'Publications', path: routePaths.researchPublications, icon: '📄' },
      { label: 'Heritage Database', path: routePaths.researchHeritageDatabase, icon: '🗄️' },
    ],
  },
  generalGroup,
];

// Logistics Partner workspace — pickups, warehousing, deliveries.
// NOTE: the logistics backend (api/logistics/*) exists but dedicated frontend
// pages/routes do not yet — Operations items land on the dashboard for now.
export const logisticsPartnerSidebarNav = [
  {
    section: 'Overview',
    items: [
      { label: 'Logistics Dashboard', path: routePaths.logisticsPartner, icon: '🏠' },
      { label: 'Company Profile', path: routePaths.logisticsPartnerProfile, icon: '🏢' },
      { label: 'Warehouses', path: routePaths.logisticsPartnerWarehouses, icon: '🏭' },
      { label: 'Shipments', path: routePaths.logisticsPartnerShipments, icon: '📦' },
      { label: 'Warehouse Stock', path: routePaths.logisticsPartnerStock, icon: '📊' },
      { label: 'Pickup Requests', path: routePaths.logisticsPartnerPickups, icon: '🚚' },
      { label: 'Returns', path: routePaths.logisticsPartnerReturns, icon: '↩️' },
      { label: 'Delivery Routes', path: routePaths.logisticsPartnerRoutes, icon: '🗺️' },
      { label: 'AI Logistics Tools', path: routePaths.logisticsPartnerAiTools, icon: '🤖' },
    ],
  },
  generalGroup,
];

// Government & NGO workspace — heritage programmes and national initiatives.
export const governmentNgoSidebarNav = [
  {
    section: 'Overview',
    items: [
      { label: 'Government Dashboard', path: routePaths.government, icon: '🏛️' },
      { label: 'Reports & Forecasts', path: routePaths.governmentReportsForecasts, icon: '📈' },
      { label: 'Policy & Compliance', path: routePaths.governmentPolicyCompliance, icon: '⚖️' },
      { label: 'Complaints & Monitoring', path: routePaths.governmentComplaintsMonitoring, icon: '🚨' },
      { label: 'NGO Programs', path: routePaths.ngo, icon: '🤲' },
    ],
  },
  generalGroup,
];

export const footerLinks = {
  about: [
    { label: 'About ShilpoHub', path: routePaths.about },
    { label: 'Mission', path: routePaths.about },
    { label: 'Vision', path: routePaths.about },
  ],
  explore: [
    { label: 'Districts', path: routePaths.exploreDistricts },
    { label: 'Heritage Villages', path: routePaths.exploreVillages },
    { label: 'Crafts', path: routePaths.exploreCrafts },
    { label: 'UNESCO Heritage', path: routePaths.exploreUnesco },
  ],
  marketplace: [
    { label: 'Products', path: routePaths.marketplaceProducts },
    { label: 'Producers', path: routePaths.exploreProducers },
    { label: 'Categories', path: routePaths.marketplaceCategories },
  ],
  resources: [
    { label: 'Academy', path: routePaths.academy },
    { label: 'Innovation Hub', path: routePaths.research },
    { label: 'News', path: routePaths.news },
    { label: 'Tourism Events', path: routePaths.tourismEvents },
  ],
  support: [
    { label: 'Contact', path: routePaths.about },
    { label: 'FAQ', path: routePaths.about },
    { label: 'Privacy Policy', path: routePaths.about },
    { label: 'Terms & Conditions', path: routePaths.about },
  ],
};

export const socialLinks = [
  { label: 'Facebook', href: '#' },
  { label: 'Instagram', href: '#' },
  { label: 'YouTube', href: '#' },
  { label: 'LinkedIn', href: '#' },
];
