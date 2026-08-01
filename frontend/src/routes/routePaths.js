export const routePaths = {
  home: '/',

  // Explore
  explore: '/explore',
  exploreDistricts: '/explore/districts',
  exploreDistrictDetails: '/explore/districts/:districtId',
  exploreVillages: '/explore/villages',
  exploreVillageDetails: '/explore/villages/:villageId',
  exploreCrafts: '/explore/crafts',
  exploreCraftDetails: '/explore/crafts/:craftId',
  exploreProducers: '/explore/producers',
  exploreProducerDetails: '/explore/producers/:producerId',
  exploreUnesco: '/explore/unesco',
  exploreMuseum: '/explore/museum',

  // Marketplace
  marketplace: '/marketplace',
  marketplaceProducts: '/marketplace/products',
  marketplaceProductDetails: '/marketplace/products/:productId',
  marketplaceCategories: '/marketplace/categories',
  marketplaceWishlist: '/marketplace/wishlist',
  marketplaceCart: '/marketplace/cart',
  marketplaceCheckout: '/marketplace/checkout',
  marketplaceAuctions: '/marketplace/auctions',

  // Tourism
  tourism: '/tourism',
  tourismMap: '/tourism/map',
  tourismFestivals: '/tourism/festivals',
  tourismEvents: '/tourism/events',
  tourismVillages: '/tourism/villages',
  tourismRoutes: '/tourism/routes',
  tourismPassport: '/tourism/passport',

  // Academy
  academy: '/academy',
  academyCourseDetails: '/academy/courses/:courseId',
  academyMentors: '/academy/mentors',
  academyCertifications: '/academy/certifications',
  academyLearning: '/academy/learning',
  academyCertificates: '/academy/certificates',
  academyPortfolio: '/academy/portfolio',

  // Research / Innovation Hub
  research: '/research',
  researchWorkspace: '/research/workspace',
  researchPublications: '/research/publications',
  researchHeritageDatabase: '/research/heritage-database',

  // Content
  news: '/news',
  newsDetails: '/news/:newsId',
  about: '/about',

  // Auth
  login: '/login',
  register: '/register',

  // Dashboard (authenticated shell)
  dashboard: '/dashboard',
  dashboardExplore: '/dashboard/explore',
  dashboardMarketplace: '/dashboard/marketplace',
  dashboardTourism: '/dashboard/tourism',
  dashboardAcademy: '/dashboard/academy',
  dashboardCommunity: '/dashboard/community',
  dashboardAnalytics: '/dashboard/analytics',
  dashboardMessages: '/dashboard/messages',
  dashboardNotifications: '/dashboard/notifications',
  dashboardSettings: '/dashboard/settings',
  dashboardProfile: '/dashboard/profile',

  // Customer journey
  customer: '/customer',
  customerMarketplace: '/customer/marketplace',
  customerProductDetails: '/customer/products/:productId',
  customerCraftStory: '/customer/crafts/:craftId/story',
  customerProducerProfile: '/customer/producers/:producerId',
  customerProducerStory: '/customer/producers/:producerId/story',
  customerWorkshops: '/customer/workshops',
  customerWishlist: '/customer/wishlist',
  customerCart: '/customer/cart',
  customerCheckout: '/customer/checkout',

  // Roles
  visitor: '/visitor',
  artisan: '/artisan',
  farmer: '/farmer',
  retailer: '/retailer',
  industryGarments: '/industry-garments',
  tourist: '/tourist',
  trainerMasterArtisan: '/trainer-master-artisan',
  apprenticeStudent: '/apprentice-student',
  government: '/government',
  ngo: '/ngo',
  researcher: '/researcher',
  exporter: '/exporter',
  logisticsPartner: '/logistics-partner',

  // Admin
  admin: '/admin',
  adminUsers: '/admin/users',
  adminHeritage: '/admin/heritage',
  adminMarketplace: '/admin/marketplace',
  adminCms: '/admin/cms',
  adminSecurity: '/admin/security',

  unauthorized: '/unauthorized',
  notFound: '*',
};
