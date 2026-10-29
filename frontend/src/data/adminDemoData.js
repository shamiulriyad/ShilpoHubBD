// Development-only records for the Super Admin workspace. They make sparse local
// databases representative without changing or pretending to persist production data.
const people = ['Afsana Rahman', 'Mahmudul Hasan', 'Nusrat Jahan', 'Tanvir Ahmed', 'Samira Sultana', 'Rafiq Uddin', 'Farzana Akter', 'Imran Hossain', 'Tahmina Noor', 'Sabbir Chowdhury'];
const districts = ['Dhaka', 'Jashore', 'Tangail', 'Rajshahi', 'Cumilla', 'Sylhet', 'Rangpur', 'Mymensingh', 'Chattogram', 'Khulna'];
const crafts = ['Jamdani weaving', 'Nakshi Kantha', 'Shital Pati', 'Terracotta', 'Jute craft', 'Bamboo craft', 'Muslin weaving', 'Wood carving', 'Pottery', 'Rickshaw art'];
const organizations = ['Bengal Heritage Trust', 'Karukaj Foundation', 'Craft Council Bangladesh', 'Rural Artisan Network', 'Living Traditions Initiative', 'Bangla Folk Arts Society', 'Heritage Skills Alliance', 'Women Artisan Collective', 'Sustainable Craft Forum', 'Deshi Design Foundation'];
const places = ['Sonargaon Craft Village', 'Panam Heritage Quarter', 'Dhamrai Artisan Village', 'Mahasthangarh Museum', 'Puthia Temple Complex', 'Mainamati Heritage Centre', 'Shat Gambuj Mosque', 'Kantajew Temple', 'Ahsan Manzil Museum', 'Varendra Research Museum'];
const day = (offset) => new Date(Date.now() + offset * 86400000).toISOString();
const slug = (value) => value.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');

function titleFor(path, i) {
  if (path.includes('categories')) return crafts[i];
  if (path.includes('villages')) return `${districts[i]} ${crafts[i]} Village`;
  if (path.includes('heritage-places')) return places[i];
  if (path.includes('tourism-locations')) return `${places[i]} Visitor Centre`;
  if (path.includes('districts')) return districts[i];
  if (path.includes('festivals')) return `${crafts[i]} Heritage Festival 2026`;
  if (path.includes('cultural-events')) return `${districts[i]} Folk Arts Evening`;
  if (path.includes('heritage-routes')) return `${districts[i]} Living Heritage Trail`;
  if (path.includes('local-cuisines')) return ['Kacchi Biryani', 'Chui Jhal', 'Shatkora Beef', 'Mezban', 'Bhapa Pitha', 'Kalai Ruti', 'Chomchom', 'Shorshe Ilish', 'Morog Polao', 'Roshmalai'][i];
  if (path.includes('museum-items')) return `${crafts[i]} Masterpiece — Collection ${i + 1}`;
  if (path.includes('unesco')) return `${crafts[i]} Living Heritage Practice`;
  if (path.includes('product-types')) return ['Home Textile', 'Traditional Apparel', 'Wall Décor', 'Tableware', 'Basketry', 'Jewellery', 'Stationery', 'Lighting', 'Collectible Art', 'Corporate Gifts'][i];
  if (path.includes('materials')) return ['Cotton', 'Jute', 'Clay', 'Bamboo', 'Cane', 'Silk', 'Wood', 'Brass', 'Natural Dye', 'Recycled Paper'][i];
  if (path.includes('/cms/blogs')) return `${crafts[i]}: makers preserving a living tradition`;
  if (path.includes('/cms/news')) return `${organizations[i]} launches artisan market programme`;
  if (path.includes('/cms/events')) return `${districts[i]} Craft & Culture Showcase`;
  if (path.includes('announcements')) return `Platform update for ${crafts[i]} sellers`;
  if (path.includes('homepage')) return `Discover ${crafts[i]} from ${districts[i]}`;
  return `${crafts[i]} operational record ${String(i + 1).padStart(2, '0')}`;
}

function sample(path, i) {
  const title = titleFor(path, i), person = people[i];
  return {
    id: `admin-demo-${slug(path)}-${i + 1}`, isDemo: true,
    name: title, nameBn: ['জামদানি', 'নকশিকাঁথা', 'শীতল পাটি', 'মৃৎশিল্প', 'পাটজাত পণ্য', 'বাঁশশিল্প', 'মসলিন', 'কাঠশিল্প', 'মৃৎপাত্র', 'রিকশা চিত্র'][i], title,
    fullName: person, legalName: person, actorName: i % 3 ? person : 'Super Admin', authorName: person, producerName: person, recipientName: person, requestedByName: person,
    assignedToName: i % 2 ? 'Moderation Team' : 'Trust & Safety', businessPartnerName: organizations[i], organizationName: organizations[i], companyName: `${districts[i]} Heritage Logistics`, serviceAreaCount: 3 + i,
    email: `${slug(person)}@example.com`, loginEmail: `${slug(person)}@example.com`, phone: `+880 17${12000000 + i * 73421}`, roles: [i % 3 ? 'Producer' : 'BusinessPartner'],
    userCount: 12 + i * 7, permissionCount: 8 + i, code: `SHB.${slug(crafts[i]).replaceAll('-', '.').toUpperCase()}.MANAGE`, module: ['Heritage', 'Marketplace', 'CMS', 'Security'][i % 4],
    description: `Verified operational record for ${title}, including provenance, ownership, review status and platform visibility.`, bio: `${person} is an experienced ${crafts[i]} practitioner who mentors emerging artisans through hands-on workshops.`, expertise: crafts[i], yearsOfExperience: 7 + i, location: districts[i], proofImageUrl: null, reviewNote: i % 4 ? 'Experience and references verified.' : null, summary: `A field-verified story documenting ${crafts[i]} makers and community impact in ${districts[i]}.`,
    content: `Editorial coverage of the people, materials and skills behind ${crafts[i]}, verified with local practitioners.`, message: `Service guidance for members working with ${crafts[i]}.`,
    districtName: districts[i], division: ['Dhaka', 'Khulna', 'Chattogram', 'Rajshahi', 'Rangpur'][Math.floor(i / 2)], craft: crafts[i], category: ['Textile', 'Community', 'Festival', 'Workshop'][i % 4], categoryName: crafts[i],
    type: ['HeritageSite', 'Museum', 'CraftCenter', 'HistoricalPlace'][i % 4], placeType: ['CraftCenter', 'Village', 'Museum', 'HistoricalSite'][i % 4], era: ['19th century', 'Early 20th century', 'Contemporary revival'][i % 3],
    region: `${districts[i]}, Bangladesh`, giName: i < 5 ? `${crafts[i]} of Bangladesh` : null, slug: slug(title), sectionKey: `featured-${slug(crafts[i])}`, group: ['about-stat', 'about-purpose', 'about-landscape', 'about-stakeholder'][i % 4],
    subtitle: 'Authentic craft, verified makers, lasting impact', source: 'ShilpoHub Editorial Desk', externalId: `BD-HER-${2026001 + i}`, referenceNumber: `SHB-2026-${1101 + i}`, orderNumber: `ORD-2609-${701 + i}`,
    action: ['Created', 'Reviewed', 'Approved', 'Published', 'Updated'][i % 5], entityType: ['HeritageRecord', 'Product', 'User', 'Content'][i % 4], ipAddress: `103.120.${20 + i}.${40 + i}`,
    keyPrefix: `shb_live_${String(i + 1).padStart(4, '0')}`, status: ['Pending', 'Active', 'Approved', 'UnderReview', 'Completed'][i % 5], approvalStatus: i % 4 ? 'Approved' : 'Pending', moderationStatus: ['Open', 'Investigating', 'Resolved'][i % 3],
    riskState: ['LowRisk', 'MediumRisk', 'HighRisk'][i % 3], severity: ['Low', 'Medium', 'High', 'Critical'][i % 4], riskScore: 18 + i * 8, failedAttempts: 3 + i, succeeded: false,
    isActive: i !== 8, isVerified: i < 8, isFeatured: i < 4, isPublished: i !== 9, displayOrder: i + 1, price: 850 + i * 625, amount: 5200 + i * 4100, refundedAmount: i % 3 ? 0 : 1200 + i * 100,
    inscribedYear: 1975 + i * 5, estimatedDurationMinutes: 90 + i * 25, totalDistanceKm: 4.5 + i * 1.8, isRecommended: i < 6, similarComplaintCount: 2 + i, highSeverityComplaintCount: i % 4, producerWarningCount: i % 3,
    createdAt: day(-i - 1), updatedAt: day(-i), detectedAt: day(-i - 1), generatedAt: day(0), startDate: day(i + 5), endDate: day(i + 6), eventDate: day(i + 7), startsAt: day(i), endsAt: day(i + 14),
    startedAt: day(-i - 2), completedAt: i % 3 ? day(-i - 1) : null, lastUsedAt: day(-i), expiresAt: day(90 + i), lastSyncedAt: day(-i), errorMessage: null,
    subjectLabel: title, productName: title,
  };
}

export function enrichAdminData(path, live) {
  if (!import.meta.env.DEV) return live;
  if (path.includes('platform-revenue-summary')) return live?.approvedSettlementCount ? live : { totalPlatformFee: 487350, approvedSettlementCount: 36, totalGrossRevenue: 9747000 };
  if (path.includes('system-health')) return { databaseConnected: true, uptime: '12 days, 7 hours', workingSetBytes: 178257920, runtimeVersion: '.NET 8', generatedAt: day(0), ...live, userCount: live?.userCount || 2846, productCount: live?.productCount || 1327, orderCount: live?.orderCount || 8914 };
  if (live && !Array.isArray(live) && !Array.isArray(live.items)) return live;
  const existing = Array.isArray(live) ? live : (live?.items || []);
  const items = [...existing, ...Array.from({ length: Math.max(0, 10 - existing.length) }, (_, i) => sample(path, existing.length + i))];
  if (Array.isArray(live)) return items;
  return { ...(live || {}), items, page: live?.page || 1, pageSize: live?.pageSize || 20, totalCount: Math.max(live?.totalCount || 0, items.length), totalPages: live?.totalPages || 1 };
}
