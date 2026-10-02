const artisans = ['Afsana Rahman', 'Mahmudul Hasan', 'Nusrat Jahan', 'Tanvir Ahmed', 'Samira Sultana', 'Rafiq Uddin', 'Farzana Akter', 'Imran Hossain', 'Tahmina Noor', 'Sabbir Chowdhury'];
const crafts = ['Jamdani', 'Nakshi Kantha', 'Shital Pati', 'Terracotta', 'Jute Craft', 'Bamboo Craft', 'Muslin', 'Wood Carving', 'Pottery', 'Rickshaw Art'];
const districts = ['Dhaka', 'Jashore', 'Tangail', 'Rajshahi', 'Cumilla', 'Sylhet', 'Rangpur', 'Mymensingh', 'Chattogram', 'Khulna'];
const day = (offset) => new Date(Date.now() + offset * 86400000).toISOString();
const demo = (type, i, values) => ({ id: `demo-${type}-${i + 1}`, isDemo: true, ...values });

const contracts = Array.from({ length: 10 }, (_, i) => {
  const quantity = 40 + i * 15, unitPrice = 950 + i * 175;
  return demo('contract', i, {
    title: `${crafts[i]} seasonal supply agreement`, referenceNumber: `CTR-2026-${String(301 + i).padStart(4, '0')}`,
    producerName: artisans[i], businessPartnerName: ['Bengal Heritage Retail', 'Dhaka Design Collective', 'Karukaj Export House'][i % 3],
    contractValue: quantity * unitPrice, status: ['PendingApproval', 'Active', 'Active', 'Expired', 'Active'][i % 5],
    terms: `Supply ${quantity} quality-inspected ${crafts[i]} pieces using approved materials. Delivery is split into two batches with sustainable packaging and product story cards.`,
    startDate: day(i + 5), endDate: day(90 + i * 10),
    items: [{ id: `demo-contract-item-${i}`, productName: `${crafts[i]} artisan collection`, quantity, unitPrice, lineTotal: quantity * unitPrice }],
    statusHistory: [{ status: 'Created', createdAt: day(-12 - i), note: 'Contract drafted by business partner.' }, { status: i % 5 === 0 ? 'PendingApproval' : 'Active', createdAt: day(-8 - i), note: i % 5 === 0 ? 'Sent to producer for review.' : 'Terms accepted by both parties.' }],
  });
});

const customerReturns = Array.from({ length: 10 }, (_, i) => demo('customer-return', i, {
  orderNumber: `SHB-2609-${String(820 + i)}`, itemCount: 1 + (i % 3), total: 1850 + i * 725,
  status: i % 2 ? 'Returned' : 'ReturnRequested', createdAt: day(-4 - i),
  reason: ['Size did not fit', 'Transit damage', 'Duplicate delivery', 'Colour variation'][i % 4],
}));

const customerRefunds = Array.from({ length: 10 }, (_, i) => demo('customer-refund', i, {
  orderNumber: `SHB-2608-${String(640 + i)}`, itemCount: 1 + (i % 2), total: 2200 + i * 860,
  status: 'Refunded', createdAt: day(-12 - i * 2), refundedAt: day(-10 - i * 2),
}));

const sponsorships = Array.from({ length: 10 }, (_, i) => demo('csr-opportunity', i, {
  title: [`Women weavers' solar lighting programme`, 'Natural dye wastewater treatment', 'Youth pottery apprenticeship', 'Jute workshop safety upgrade', 'Jamdani loom restoration', 'Craft village digital catalogue', 'Accessible artisan workspace', 'Rainwater harvesting for dyeing', 'Bamboo treatment facility', 'Heritage skills scholarship'][i],
  producerName: artisans[i], description: `A measurable community initiative supporting ${crafts[i]} artisans in ${districts[i]} with equipment, training and long-term income resilience.`,
  status: 'Open', fundingSecured: i * 18000, fundingGoal: 180000 + i * 45000,
}));

const proposals = Array.from({ length: 10 }, (_, i) => demo('csr-proposal', i, {
  opportunityTitle: sponsorships[i].title, fundingAmount: 35000 + i * 7500,
  status: ['Submitted', 'Approved', 'Active', 'Completed'][i % 4], proposalMessage: 'Funding linked to verified milestones and quarterly impact reporting.',
}));

const suppliers = Array.from({ length: 10 }, (_, i) => ({
  producerId: `demo-supplier-${i + 1}`, isDemo: true, producerName: artisans[i], workshopName: `${districts[i]} ${crafts[i]} Studio`,
  primaryCraft: crafts[i], districtName: districts[i], averageRating: 4.2 + (i % 7) * 0.1, totalReviewCount: 18 + i * 7,
  productCount: 12 + i * 4, minPrice: 650 + i * 120, maxPrice: 9800 + i * 1350, isHandmadeVerified: i < 8,
  expertiseLevel: ['Bronze', 'Silver', 'Gold'][i % 3], productionCapacity: 40 + i * 15,
}));

const procurements = Array.from({ length: 10 }, (_, i) => demo('bp-procurement', i, {
  title: `${crafts[i]} wholesale collection — ${districts[i]}`, referenceNumber: `PROC-2026-${String(810 + i)}`,
  businessPartnerName: 'Bengal Heritage Commerce', producerName: artisans[i], deliveryDeadline: day(25 + i * 4),
  status: ['PendingApproval', 'Approved', 'Converted', 'Approved'][i % 4], itemsTotal: 85000 + i * 28500,
  requiredAdvance: (85000 + i * 28500) / 2, advanceAmount: i % 4 ? (85000 + i * 28500) / 2 : 0,
  advancePaidAt: i % 4 ? day(-i) : null, inspectionStatus: ['NotRequired', 'Pending', 'Approved', 'Approved'][i % 4],
}));

export const roleDemoData = { contracts, customerReturns, customerRefunds, sponsorships, proposals, suppliers, procurements };

export const mergeRolePage = (live, key) => {
  if (!import.meta.env.DEV) return live;
  const existing = live?.items || [];
  const items = [...existing, ...(roleDemoData[key] || []).slice(0, Math.max(0, 10 - existing.length))];
  return { ...(live || {}), items, page: live?.page || 1, pageSize: live?.pageSize || 20, totalCount: items.length, totalPages: 1 };
};

export const roleDemoRecord = (key, id) => roleDemoData[key]?.find((item) => item.id === id);
