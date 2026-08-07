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
    ],
  },
  academy: {
    heading: 'Academy',
    description: 'Learn traditional crafts from master artisans.',
    links: [
      { label: 'Courses', description: 'Browse the course catalog', path: routePaths.academy },
      { label: 'Mentors', description: 'Master artisans & trainers', path: routePaths.academyMentors },
      { label: 'Certifications', description: 'Recognized skill certificates', path: routePaths.academyCertifications },
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

export const sidebarNav = [
  { label: 'Dashboard', path: routePaths.dashboard },
  { label: 'Explore', path: routePaths.dashboardExplore },
  { label: 'Marketplace', path: routePaths.dashboardMarketplace },
  { label: 'Tourism', path: routePaths.dashboardTourism },
  { label: 'Academy', path: routePaths.dashboardAcademy },
  { label: 'Community', path: routePaths.dashboardCommunity },
  { label: 'Analytics', path: routePaths.dashboardAnalytics },
  { label: 'Messages', path: routePaths.dashboardMessages },
  { label: 'Notifications', path: routePaths.dashboardNotifications },
  { label: 'Settings', path: routePaths.dashboardSettings },
];

export const customerSidebarNav = [
  { label: 'Dashboard', path: routePaths.customer },
  { label: 'Marketplace', path: routePaths.customerMarketplace },
  { label: 'Workshops', path: routePaths.customerWorkshops },
  { label: 'Auctions', path: routePaths.customerAuctions },
  { label: 'Community', path: routePaths.customerCommunity },
  { label: 'Messages', path: routePaths.customerMessages },
  { label: 'Following', path: routePaths.customerFollowing },
  { label: 'Favorite Villages', path: routePaths.customerFavoriteVillages },
  { label: 'Wishlist', path: routePaths.customerWishlist },
  { label: 'Cart', path: routePaths.customerCart },
  { label: 'Order History', path: routePaths.customerOrders },
  { label: 'Returns', path: routePaths.customerReturns },
  { label: 'Refunds', path: routePaths.customerRefunds },
  { label: 'Notifications', path: routePaths.customerNotifications },
  { label: 'Saved Addresses', path: routePaths.customerAddresses },
  { label: 'Heritage Collection', path: routePaths.customerHeritageCollection },
  { label: 'Heritage Passport', path: routePaths.customerHeritagePassport },
  { label: 'Purchase Analytics', path: routePaths.customerPurchaseAnalytics },
  { label: 'Impact Dashboard', path: routePaths.customerImpactDashboard },
  { label: 'Achievements', path: routePaths.customerAchievements },
  { label: 'Badge Collection', path: routePaths.customerBadges },
  { label: 'AI Gift Recommendation', path: routePaths.customerAIGiftRecommendation },
];

export const adminSidebarNav = [
  { label: 'Dashboard', path: routePaths.admin },
  { label: 'User Management', path: routePaths.adminUsers },
  { label: 'Heritage Management', path: routePaths.adminHeritage },
  { label: 'Marketplace Monitoring', path: routePaths.adminMarketplace },
  { label: 'CMS', path: routePaths.adminCms },
  { label: 'Security Center', path: routePaths.adminSecurity },
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
