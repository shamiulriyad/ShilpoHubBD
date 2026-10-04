// Rich development-only records keep every Producer workspace useful while the real
// account is still being populated. Production builds always return API data unchanged.
const demo = (id, values) => ({ id: `demo-${id}`, isDemo: true, ...values });
const days = (offset) => new Date(Date.now() + offset * 86400000).toISOString();

export const producerDemoData = {
  customOrders: [
    demo('order-1', { title: 'Custom Nakshi Kantha wedding set', customerName: 'Nusrat Jahan', customerId: 'demo-customer-1', createdAt: days(-3), productName: 'Hand-stitched Nakshi Kantha', specifications: 'Queen-size kantha with matching pillow covers. Use deep indigo, ivory and muted gold thread with a traditional lotus border.', budget: 18500, deadline: days(24), quotedPrice: null, status: 'Pending' }),
    demo('order-2', { title: 'Personalized Jamdani table runner', customerName: 'Farhan Ahmed', customerId: 'demo-customer-2', createdAt: days(-8), productName: 'Jamdani Table Runner', specifications: 'Six-foot runner with a geometric indigo motif and family initials woven at one end.', budget: 9000, deadline: days(14), quotedPrice: 8200, producerResponse: 'Materials are reserved and the first weaving stage is underway.', status: 'InProgress' }),
    demo('order-3', { title: 'Corporate gift basket collection', customerName: 'Maliha Trading Ltd.', customerId: 'demo-customer-3', createdAt: days(-18), specifications: 'Twelve gift baskets combining woven trays, hand-painted coasters and branded message cards.', budget: 36000, deadline: days(4), quotedPrice: 34800, producerResponse: 'All pieces have passed the final quality check.', status: 'Completed', shippingAddressLine: 'Gulshan Avenue, Dhaka' }),
  ],
  questions: [
    demo('question-1', { productName: 'Indigo Nakshi Kantha', askerName: 'Akhter Hossain', createdAt: days(-2), body: 'Is the colour washable, and what care method do you recommend?', answers: [] }),
    demo('question-2', { productName: 'Natural Jute Storage Basket', askerName: 'Samira Noor', createdAt: days(-5), body: 'Can this basket hold approximately 8 kg without losing its shape?', answers: [{ id: 'demo-answer-1', authorName: 'ShilpoHub Producer', body: 'Yes. The reinforced base is tested up to 10 kg; keep it dry and lift it using both handles.' }] }),
    demo('question-3', { productName: 'Hand-painted Terracotta Vase', askerName: 'Imran Kabir', createdAt: days(-7), body: 'Is the finish safe for indoor use around children?', answers: [{ id: 'demo-answer-2', authorName: 'ShilpoHub Producer', body: 'It uses a water-based, lead-free sealant and is intended for indoor decorative use.' }] }),
  ],
  returns: [
    demo('return-1', { orderId: 'demo-return-order-1', orderNumber: 'SHB-260923-1048', customerName: 'Tahmina Rahman', customerPhone: '+880 1712-345678', requestedAt: days(-2), reason: 'The runner is beautiful, but the size does not fit my dining table.', orderStatus: 'ReturnRequested', canRespond: false, items: [{ productName: 'Jamdani Table Runner', quantity: 1, lineTotal: 4200 }], itemsAmount: 4200, orderTotal: 4320, amountPaid: 4320, refundedAmount: null, returnReference: 'RET-2609-018', returnStatus: 'Pickup requested' }),
    demo('return-2', { orderId: 'demo-return-order-2', orderNumber: 'SHB-260915-0872', customerName: 'Rafiul Karim', customerPhone: '+880 1819-804321', requestedAt: days(-11), reason: 'Received a duplicate item in the parcel.', orderStatus: 'Refunded', canRespond: false, items: [{ productName: 'Painted Wooden Jewellery Box', quantity: 1, lineTotal: 2650 }], itemsAmount: 2650, orderTotal: 2770, amountPaid: 2770, refundedAmount: 2770, returnReference: 'RET-2609-009', returnStatus: 'Delivered to producer' }),
  ],
  complaints: [
    demo('complaint-1', { subject: 'Loose embroidery near the border', productName: 'Nakshi Kantha Cushion Cover', orderNumber: 'SHB-260925-1120', customerName: 'Mushfiqa Alam', createdAt: days(-1), description: 'A small section of embroidery became loose after the first gentle wash. I would appreciate repair guidance or a replacement.', status: 'Open' }),
    demo('complaint-2', { subject: 'Replacement delivered successfully', productName: 'Terracotta Tea Set', orderNumber: 'SHB-260901-0641', customerName: 'Zahid Hasan', createdAt: days(-17), description: 'One cup arrived chipped despite the outer packaging being intact.', producerResponse: 'We sent a replacement cup with reinforced protective packaging at no cost.', customerNote: 'The replacement arrived safely. Thank you for resolving this quickly.', status: 'Satisfied' }),
  ],
  supportCases: [
    demo('support-1', { caseNumber: 'ASC-2026-0142', problemTitle: 'Need an improved frame for larger kantha work', problemDescription: 'The current wooden frame bends during large commissioned pieces, slowing production and affecting stitch consistency.', organizationName: 'Bangladesh Folk Arts Foundation', craft: 'Nakshi Kantha', district: 'Jashore', status: 'SupportInProgress' }),
    demo('support-2', { caseNumber: 'ASC-2026-0108', problemTitle: 'Natural dye safety training', problemDescription: 'Requested practical training on colour consistency, safe mordants and wastewater handling for our five-person workshop.', organizationName: 'Craft Council Bangladesh', craft: 'Natural Dyeing', district: 'Dhaka', status: 'Closed', supportProvidedAt: days(-12), artisanConfirmation: 'Received' }),
  ],
  auctions: [
    demo('auction-1', { title: 'Museum-grade Nakshi Kantha — limited edition', productName: 'Heritage Nakshi Kantha', description: 'A one-of-one hand-stitched textile inspired by Jashore folk narratives.', startAt: days(-1), endAt: days(3), status: 'Active', currentPrice: 28500, bidCount: 9, winnerName: null }),
    demo('auction-2', { title: 'Signed Jamdani wall tapestry', productName: 'Jamdani Wall Art', description: 'Collector piece woven over six weeks and signed by the artisan.', startAt: days(5), endAt: days(10), status: 'Scheduled', currentPrice: 18000, bidCount: 0, winnerName: null }),
    demo('auction-3', { title: 'Terracotta folklore sculpture set', productName: 'Bengal Folklore Sculpture', startAt: days(-18), endAt: days(-12), status: 'Ended', currentPrice: 15400, bidCount: 14, winnerName: 'Sadia Heritage Gallery' }),
  ],
  quotations: [
    demo('quotation-1', { title: 'Handcrafted corporate gift collection', referenceNumber: 'RFQ-2609-071', requiredDeliveryDate: days(21), status: 'Sent', requirements: 'Eco-friendly packaging, consistent finishing and a small story card for each artisan-made item.', items: [{ id: 'demo-qi-1', productName: 'Jute & leather notebook cover', quantity: 80, targetPrice: 620 }, { id: 'demo-qi-2', productName: 'Hand-painted wooden coaster set', quantity: 80, targetPrice: 480 }] }),
    demo('quotation-2', { title: 'Boutique hotel textile package', referenceNumber: 'RFQ-2609-058', requiredDeliveryDate: days(35), status: 'PartiallyResponded', requirements: 'Natural fibres in indigo and warm neutral tones; samples required before bulk production.', items: [{ id: 'demo-qi-3', productName: 'Kantha cushion cover', quantity: 120, targetPrice: 850 }, { id: 'demo-qi-4', productName: 'Table runner', quantity: 30, targetPrice: 3200 }] }),
    demo('quotation-3', { title: 'Festival retail assortment', referenceNumber: 'RFQ-2608-039', requiredDeliveryDate: days(12), status: 'Responded', requirements: 'Retail-ready assortment for three Dhaka outlets.', items: [{ id: 'demo-qi-5', productName: 'Terracotta décor set', quantity: 45, targetPrice: 1500 }] }),
  ],
  procurements: [
    demo('procurement-1', { title: 'Premium craft gift boxes for annual summit', referenceNumber: 'PR-2609-044', businessPartnerName: 'Bengal Heritage Retail Ltd.', deliveryDeadline: days(28), status: 'PendingApproval', itemsTotal: 184000, requiredAdvance: 92000, advanceAmount: 0, inspectionStatus: 'NotRequired' }),
    demo('procurement-2', { title: 'Jamdani soft furnishing collection', referenceNumber: 'PR-2609-031', businessPartnerName: 'Aranya Living', deliveryDeadline: days(42), status: 'Approved', itemsTotal: 276000, requiredAdvance: 138000, advanceAmount: 138000, advancePaidAt: days(-2), inspectionStatus: 'Pending' }),
    demo('procurement-3', { title: 'Export sample assortment — Copenhagen', referenceNumber: 'PR-2608-019', businessPartnerName: 'Nordic Folk Design ApS', deliveryDeadline: days(18), status: 'Converted', itemsTotal: 128500, requiredAdvance: 64250, advanceAmount: 70000, advancePaidAt: days(-15), inspectionStatus: 'Approved', inspectionNotes: 'Materials, quantities and export packaging verified.' }),
  ],
  manufacturing: [
    demo('manufacturing-1', { title: '120-piece natural-dye home collection', quantity: 120, progressPercentage: 55, status: 'InProgress', productRequirements: 'Cushion covers, runners and storage baskets in a coordinated natural indigo palette.', manufacturingSpecifications: 'Colour variation within approved sample tolerance; plastic-free packaging.', milestones: [{ id: 'demo-mm-1', title: 'Materials sourced', status: 'Completed' }, { id: 'demo-mm-2', title: 'First production batch', status: 'Completed' }, { id: 'demo-mm-3', title: 'Final production and QC', status: 'InProgress' }] }),
    demo('manufacturing-2', { title: 'Boutique terracotta lighting range', quantity: 40, progressPercentage: 10, status: 'Requested', productRequirements: 'Pendant shades in three forms based on Bengal temple geometry.', manufacturingSpecifications: 'Heat-resistant clay body with standard electrical fittings.', milestones: [] }),
    demo('manufacturing-3', { title: 'Artisan stationery launch batch', quantity: 250, progressPercentage: 100, status: 'Completed', productRequirements: 'Recycled-paper notebooks with handwoven textile covers.', manufacturingSpecifications: 'Five approved colourways, retail barcode and story card.', milestones: [] }),
  ],
  agreements: [
    demo('agreement-1', { producerName: 'ShilpoHub Producer', businessPartnerName: 'Bengal Heritage Retail Ltd.', auctionName: 'National Retail Partnership — 2026', winningBidAmount: 425000, status: 'Active', producerSharePercentage: 72, businessPartnerSharePercentage: 23, platformFeePercentage: 5, settlementFrequency: 'Monthly', partnershipDurationMonths: 12, startDate: days(-45), endDate: days(320), agreementTerms: 'Producer maintains agreed quality and lead times; partner manages retail distribution, promotion and monthly sales reporting.', statusHistory: [{ createdAt: days(-48), status: 'Confirmed', note: 'Both parties accepted the agreement.' }, { createdAt: days(-45), status: 'Active', note: 'Partnership activated.' }] }),
    demo('agreement-2', { producerName: 'ShilpoHub Producer', businessPartnerName: 'Dhaka Design Collective', auctionName: 'Dhaka Flagship Collaboration', winningBidAmount: 295000, status: 'AwaitingProducerConfirmation', producerSharePercentage: 70, businessPartnerSharePercentage: 25, platformFeePercentage: 5, settlementFrequency: 'Monthly', partnershipDurationMonths: 6, startDate: null, endDate: null, agreementTerms: 'Limited retail exclusivity for the jointly developed Jamdani home collection.' }),
    demo('agreement-3', { producerName: 'ShilpoHub Producer', businessPartnerName: 'Karukaj Export House', auctionName: 'South Asia Export Partnership', winningBidAmount: 510000, status: 'Completed', producerSharePercentage: 75, businessPartnerSharePercentage: 20, platformFeePercentage: 5, settlementFrequency: 'Quarterly', partnershipDurationMonths: 8, startDate: days(-280), endDate: days(-35), agreementTerms: 'Completed export market pilot covering three seasonal collections.' }),
  ],
  design: [
    demo('design-1', { title: 'Contemporary Jamdani cushion collection', status: 'Active', revisionCount: 2, designRequirements: 'Translate traditional buti motifs into a contemporary six-piece home collection while preserving handwoven character.', revisions: [{ id: 'demo-dr-1' }, { id: 'demo-dr-2' }], comments: [{ id: 'demo-dc-1', authorName: 'Dhaka Design Collective', content: 'The second colour study is approved. Please refine the border scale for the final revision.' }] }),
    demo('design-2', { title: 'Heritage-inspired sustainable packaging', status: 'Invited', revisionCount: 0, designRequirements: 'Develop reusable jute packaging with a simple visual system inspired by alpana patterns.', revisions: [], comments: [] }),
    demo('design-3', { title: 'Pohela Boishakh retail display series', status: 'Completed', revisionCount: 4, designRequirements: 'Modular display objects for a seasonal retail campaign.', revisions: [{}, {}, {}, {}], comments: [] }),
  ],
  development: [
    demo('development-1', { title: 'Stackable terracotta aroma diffuser', status: 'Active', prototypeVersionCount: 2, businessRequirements: 'A giftable home fragrance product suitable for local retail and export.', productSpecifications: 'Two-piece terracotta body, stable base, low surface temperature and recyclable packaging.', prototypeVersions: [{ id: 'demo-pv-1' }, { id: 'demo-pv-2' }], comments: [{ id: 'demo-pc-1', authorName: 'Bengal Heritage Retail', content: 'Prototype 2 has the right proportions. Please improve airflow around the upper vents.' }] }),
    demo('development-2', { title: 'Travel-size Nakshi Kantha organiser', status: 'Requested', prototypeVersionCount: 0, businessRequirements: 'Compact premium organiser for the travel and gifting market.', productSpecifications: 'Six compartments, washable cotton lining, secure zipper and one signature hand-stitched panel.', prototypeVersions: [], comments: [] }),
    demo('development-3', { title: 'Modular jute desk accessories', status: 'Approved', prototypeVersionCount: 3, businessRequirements: 'Sustainable desk range for corporate gifting.', productSpecifications: 'Three modular pieces with water-resistant natural coating.', prototypeVersions: [{}, {}, {}], comments: [] }),
  ],
  expertise: { profileApproved: true, averageRating: 4.72, ratingCount: 38, earnedLevel: 'Gold', highestIssuedLevel: 'Silver', awaitingAdmin: true, nextLevel: null, rules: [{ level: 'Bronze', minRatings: 5, minAverage: 4 }, { level: 'Silver', minRatings: 10, minAverage: 4.3 }, { level: 'Gold', minRatings: 25, minAverage: 4.6 }], certificates: [demo('certificate-1', { expertise: 'Heritage Textile Craftsmanship', certificateNumber: 'SHB-EXP-2026-0187', issuedAt: days(-72), averageRating: 4.68, ratingCount: 27, level: 'Silver', isRevoked: false })] },
};

export const mergeDemoArray = (live, key) => import.meta.env.DEV ? [...(live || []), ...(producerDemoData[key] || [])] : (live || []);

export const mergeDemoPage = (live, key) => {
  if (!import.meta.env.DEV) return live;
  const items = mergeDemoArray(live?.items, key);
  return { ...(live || {}), items, page: live?.page || 1, pageSize: live?.pageSize || items.length, totalCount: items.length, totalPages: 1 };
};

export const demoRecord = (key, id) => producerDemoData[key]?.find?.((item) => item.id === id);
