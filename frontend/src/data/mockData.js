// Static placeholder content for UI/UX layout purposes only. No real data.

export const heritageStats = [
  { label: 'Registered Producers', value: '12,400+' },
  { label: 'Heritage Villages', value: '640+' },
  { label: 'Heritage Products', value: '8,900+' },
  { label: 'Districts Covered', value: '64' },
];

export const districts = Array.from({ length: 8 }).map((_, i) => ({
  id: `district-${i + 1}`,
  name: ['Dhaka', 'Chattogram', 'Rajshahi', 'Khulna', 'Sylhet', 'Barishal', 'Rangpur', 'Mymensingh'][i],
  villages: 12 + i * 3,
  crafts: 4 + i,
  image: null,
}));

export const villages = Array.from({ length: 6 }).map((_, i) => ({
  id: `village-${i + 1}`,
  name: ['Rishipara', 'Tantipara', 'Shonkharibazar', 'Jamdani Palli', 'Notun Bazar', 'Kumar Para'][i],
  craft: ['Weaving', 'Pottery', 'Shell Craft', 'Jamdani', 'Bamboo Work', 'Terracotta'][i],
  district: districts[i % districts.length].name,
  image: null,
}));

export const crafts = [
  { id: 'craft-1', name: 'Jamdani Weaving', category: 'Textile', producers: 320 },
  { id: 'craft-2', name: 'Nakshi Kantha', category: 'Textile', producers: 210 },
  { id: 'craft-3', name: 'Terracotta Art', category: 'Pottery', producers: 145 },
  { id: 'craft-4', name: 'Shital Pati', category: 'Weaving', producers: 98 },
  { id: 'craft-5', name: 'Wood Carving', category: 'Woodwork', producers: 76 },
  { id: 'craft-6', name: 'Metal Craft', category: 'Metalwork', producers: 64 },
];

export const producers = Array.from({ length: 6 }).map((_, i) => ({
  id: `producer-${i + 1}`,
  name: ['Rahima Begum', 'Abdul Karim', 'Shefali Rani', 'Motiur Rahman', 'Nasrin Akter', 'Jamal Uddin'][i],
  craft: crafts[i % crafts.length].name,
  district: districts[i % districts.length].name,
  rating: (4 + (i % 2) * 0.5).toFixed(1),
  image: null,
}));

export const products = Array.from({ length: 8 }).map((_, i) => ({
  id: `product-${i + 1}`,
  name: [
    'Handwoven Jamdani Saree',
    'Nakshi Kantha Throw',
    'Terracotta Vase Set',
    'Shital Pati Mat',
    'Carved Wooden Panel',
    'Brass Table Lamp',
    'Bamboo Basket Set',
    'Clay Tea Set',
  ][i],
  producer: producers[i % producers.length].name,
  district: districts[i % districts.length].name,
  price: 1200 + i * 350,
  category: crafts[i % crafts.length].name,
  image: null,
}));

export const categories = [...new Map(crafts.map((c) => [c.category, c])).values()].map((c) => ({
  id: c.id,
  name: c.category,
  itemCount: 20 + (c.producers % 40),
}));

export const auctions = Array.from({ length: 4 }).map((_, i) => ({
  id: `auction-${i + 1}`,
  name: products[i].name,
  producer: producers[i % producers.length].name,
  producerId: producers[i % producers.length].id,
  district: districts[i % districts.length].name,
  category: crafts[i % crafts.length].name,
  description: `A rare, limited-edition ${products[i].name.toLowerCase()} offered directly by the maker. Placeholder auction description covering provenance and condition.`,
  currentBid: 4200 + i * 900,
  bidsCount: 6 + i * 3,
  closesIn: `${2 + i}d ${5 * i}h`,
  bidHistory: Array.from({ length: 3 }).map((__, j) => ({
    id: `auction-${i + 1}-bid-${j + 1}`,
    bidder: `Bidder${(j + 1) * 7 + i}`,
    amount: 4200 + i * 900 - (2 - j) * 300,
    time: `${(j + 1) * 3}h ago`,
  })),
}));

export const festivals = Array.from({ length: 6 }).map((_, i) => ({
  id: `festival-${i + 1}`,
  name: ['Pohela Boishakh', 'Nabanna Utsab', 'Jamdani Mela', 'Poush Mela', 'Tribal Culture Fest', 'Terracotta Fair'][i],
  date: `2026-${String((i + 2) % 12 + 1).padStart(2, '0')}-15`,
  district: districts[i % districts.length].name,
}));

export const culturalEvents = Array.from({ length: 5 }).map((_, i) => ({
  id: `event-${i + 1}`,
  name: ['Folk Music Evening', 'Craft Demonstration', 'Heritage Walk', 'Weaving Workshop', 'Storytelling Night'][i],
  date: `2026-0${(i % 9) + 1}-2${i}`,
  venue: `${districts[i % districts.length].name} Cultural Center`,
}));

export const tourRoutes = Array.from({ length: 4 }).map((_, i) => ({
  id: `route-${i + 1}`,
  name: ['Northern Weaving Trail', 'Southern Pottery Circuit', 'Riverine Craft Route', 'Hill Tract Heritage Trail'][i],
  duration: `${2 + i} days`,
  stops: 3 + i,
}));

export const courses = Array.from({ length: 6 }).map((_, i) => ({
  id: `course-${i + 1}`,
  title: [
    'Introduction to Jamdani Weaving',
    'Nakshi Kantha Embroidery',
    'Terracotta Pottery Basics',
    'Bamboo Craft Fundamentals',
    'Natural Dye Techniques',
    'Heritage Business Essentials',
  ][i],
  mentor: producers[i % producers.length].name,
  level: ['Beginner', 'Beginner', 'Intermediate', 'Beginner', 'Intermediate', 'Advanced'][i],
  duration: `${4 + i} weeks`,
  enrolled: 120 + i * 45,
}));

export const mentors = producers.map((p, i) => ({ ...p, expertise: crafts[i % crafts.length].name, students: 30 + i * 12 }));

export const certifications = [
  { id: 'cert-1', name: 'Certified Master Weaver', issued: 480 },
  { id: 'cert-2', name: 'Heritage Craft Practitioner', issued: 920 },
  { id: 'cert-3', name: 'Sustainable Producer Badge', issued: 310 },
];

export const publications = Array.from({ length: 5 }).map((_, i) => ({
  id: `pub-${i + 1}`,
  title: [
    'Mapping Intangible Heritage in Bangladesh',
    'Economic Impact of Craft Tourism',
    'Preservation of Jamdani Techniques',
    'Digitizing Heritage Archives',
    'Sustainable Craft Supply Chains',
  ][i],
  author: `Dr. ${producers[i % producers.length].name}`,
  year: 2022 + (i % 4),
}));

export const newsItems = Array.from({ length: 6 }).map((_, i) => ({
  id: `news-${i + 1}`,
  title: [
    'ShilpoHub Launches Digital Museum Initiative',
    'Jamdani Village Receives UNESCO Recognition',
    'New Academy Cohort Begins This Month',
    'Innovation Hub Publishes Heritage Dataset',
    'Marketplace Crosses 10,000 Products',
    'Tourism Board Partners with ShilpoHub',
  ][i],
  date: `2026-0${(i % 7) + 1}-1${i}`,
  category: ['Platform', 'Heritage', 'Academy', 'Research', 'Marketplace', 'Tourism'][i],
}));

export const timeline = [
  { year: '1971', label: 'Independence & the revival of national craft identity' },
  { year: '1985', label: 'First national craft cooperatives established' },
  { year: '2013', label: 'Jamdani recognized by UNESCO' },
  { year: '2020', label: 'Digital heritage documentation begins' },
  { year: '2026', label: 'ShilpoHub national ecosystem launches' },
];

export const notifications = Array.from({ length: 5 }).map((_, i) => ({
  id: `notif-${i + 1}`,
  title: [
    'New order received',
    'Course enrollment confirmed',
    'Your listing was approved',
    'Festival reminder: Jamdani Mela',
    'New message from a producer',
  ][i],
  time: `${i + 1}h ago`,
  read: i > 2,
}));

export const messages = Array.from({ length: 4 }).map((_, i) => ({
  id: `msg-${i + 1}`,
  from: producers[i % producers.length].name,
  preview: 'Thanks for your interest in the collection, let me know if you have questions…',
  time: `${i + 1}d ago`,
  unread: i < 2,
}));

export const adminUsers = Array.from({ length: 6 }).map((_, i) => ({
  id: `user-${i + 1}`,
  name: producers[i % producers.length].name,
  role: ['Artisan', 'Farmer', 'Customer', 'Tourist', 'NGO', 'Government'][i],
  status: i % 3 === 0 ? 'Pending' : 'Active',
  joined: `2026-0${(i % 9) + 1}-0${i + 1}`,
}));

export const craftStories = crafts.map((craft, i) => ({
  id: craft.id,
  title: craft.name,
  origin: districts[i % districts.length].name,
  since: 1850 + i * 25,
  summary: `The story of ${craft.name}, passed down through generations of makers in ${districts[i % districts.length].name}.`,
  chapters: [
    {
      heading: 'Origins',
      body: `${craft.name} traces back to riverside communities that turned local materials into everyday and ceremonial objects. Placeholder narrative describing early trade routes and the families who first practiced the technique.`,
    },
    {
      heading: 'Materials & Technique',
      body: `Each piece begins with hand-selected raw material and a technique refined over decades. Placeholder narrative describing tools, preparation steps and the patience the craft demands.`,
    },
    {
      heading: 'A Living Tradition',
      body: `Today, ${craft.producers}+ producers keep ${craft.name} alive, blending inherited technique with new patterns for a global audience. Placeholder narrative describing present-day practice and preservation efforts.`,
    },
  ],
}));

export const producerStories = producers.map((producer, i) => ({
  id: producer.id,
  heritageId: `SH-HID-${2026}${String(1000 + i)}`,
  generations: 2 + (i % 4),
  quote: `Every piece I make carries the hands of my ${['mother', 'grandfather', 'teacher', 'family'][i % 4]} before me.`,
  chapters: [
    {
      heading: 'A Craft Inherited',
      body: `${producer.name} learned ${producer.craft} as a child in ${producer.district}, watching elders work before ever touching the material. Placeholder narrative describing early years and mentorship.`,
    },
    {
      heading: 'Building a Livelihood',
      body: `Over the years, ${producer.name} turned traditional skill into a sustainable workshop, training apprentices and supplying local markets. Placeholder narrative describing growth and challenges overcome.`,
    },
    {
      heading: 'Sharing the Craft Today',
      body: `Through ShilpoHub, ${producer.name}'s work now reaches customers nationwide, with each product tracing back to this workshop in ${producer.district}. Placeholder narrative describing present work and legacy.`,
    },
  ],
}));

export const workshops = Array.from({ length: 6 }).map((_, i) => ({
  id: `workshop-${i + 1}`,
  title: [
    'Live Jamdani Loom Session',
    'Nakshi Kantha Stitch Circle',
    'Terracotta Throwing Demo',
    'Bamboo Weaving Basics',
    'Natural Dye Workshop',
    'Brass Casting Showcase',
  ][i],
  producerId: producers[i % producers.length].id,
  producer: producers[i % producers.length].name,
  craft: crafts[i % crafts.length].name,
  status: i < 2 ? 'live' : i < 4 ? 'upcoming' : 'past',
  scheduledFor: `2026-0${(i % 9) + 1}-1${i}`,
  viewers: i < 2 ? 120 + i * 40 : undefined,
}));

export const orders = Array.from({ length: 4 }).map((_, i) => ({
  id: `SH-ORD-${10234 + i}`,
  items: 1 + (i % 3),
  total: 1500 + i * 800,
  status: ['Delivered', 'Shipped', 'Processing', 'Delivered'][i],
  date: `2026-0${(i % 7) + 1}-1${i}`,
  paymentMethod: ['bKash', 'Card Payment', 'Cash on Delivery', 'Nagad'][i],
  address: `House ${12 + i}, Road ${4 + i}, ${districts[i % districts.length].name}`,
  lineItems: Array.from({ length: 1 + (i % 3) }).map((__, j) => ({
    product: products[(i + j) % products.length].name,
    qty: 1,
    price: products[(i + j) % products.length].price,
  })),
  trackingSteps: [
    { label: 'Order Placed', done: true, date: `2026-0${(i % 7) + 1}-1${i}` },
    { label: 'Processing', done: true, date: `2026-0${(i % 7) + 1}-1${i}` },
    { label: 'Shipped', done: i !== 2, date: i !== 2 ? `2026-0${(i % 7) + 1}-1${i}` : null },
    { label: 'Delivered', done: i === 0 || i === 3, date: i === 0 || i === 3 ? `2026-0${(i % 7) + 1}-1${i}` : null },
  ],
}));

export const reviews = Array.from({ length: 3 }).map((_, i) => ({
  id: `review-${i + 1}`,
  author: producers[(i + 2) % producers.length].name,
  rating: 5 - (i % 2),
  comment: 'Beautiful craftsmanship and fast delivery. Placeholder review text describing product quality.',
  date: `2026-0${(i % 7) + 1}-0${i + 1}`,
}));

export const communityPosts = Array.from({ length: 5 }).map((_, i) => ({
  id: `post-${i + 1}`,
  authorId: producers[i % producers.length].id,
  author: producers[i % producers.length].name,
  craft: producers[i % producers.length].craft,
  content: [
    'Just finished a new batch of natural dyes for this season’s collection — placeholder update from the workshop.',
    'Grateful for all the support from customers this month. Placeholder post about a recent milestone.',
    'Behind the scenes from today’s live session — placeholder recap of a workshop stream.',
    'A look at the loom setup before we start a new Jamdani piece. Placeholder process update.',
    'Thank you for 500 orders! Placeholder celebration post from the producer.',
  ][i],
  likes: 40 + i * 12,
  comments: 4 + i * 2,
  time: `${i + 1}h ago`,
}));

export const forumThreads = Array.from({ length: 6 }).map((_, i) => ({
  id: `thread-${i + 1}`,
  title: [
    'Tips for caring for handwoven Jamdani sarees',
    'How can we tell if pottery is authentic vs. mass-produced?',
    'Best practices for shipping fragile terracotta items',
    'Favorite festivals to buy directly from producers',
    'Verifying a producer’s Digital Heritage ID',
    'Recommendations for first-time heritage tourism routes',
  ][i],
  category: ['General', 'Authenticity', 'Shipping', 'Tourism', 'Authenticity', 'Tourism'][i],
  author: producers[i % producers.length].name,
  replies: 3 + i * 4,
  views: 120 + i * 45,
  lastActivity: `${i + 1}d ago`,
}));

export const qaThreads = Array.from({ length: 4 }).map((_, i) => ({
  id: `qa-${i + 1}`,
  question: [
    'How long does a handwoven Jamdani saree usually take to make?',
    'What natural materials are used in terracotta glazing?',
    'Can I request a custom color for Nakshi Kantha embroidery?',
    'Is Cash on Delivery available outside Dhaka?',
  ][i],
  craft: crafts[i % crafts.length].name,
  askedBy: 'Customer',
  time: `${i + 1}d ago`,
  answers: [
    {
      id: `qa-${i + 1}-ans-1`,
      author: producers[i % producers.length].name,
      isProducer: true,
      body: 'Placeholder answer from a verified producer explaining the process and typical timelines.',
      votes: 12 + i,
    },
  ],
}));

export const conversations = producers.slice(0, 4).map((producer, i) => ({
  id: `conv-${i + 1}`,
  producerId: producer.id,
  producer: producer.name,
  craft: producer.craft,
  lastMessage: 'Thanks for your interest in the collection, let me know if you have questions…',
  time: `${i + 1}d ago`,
  unread: i < 2,
  thread: [
    { id: 1, from: producer.name, text: 'Thanks for reaching out! How can I help?', time: '2d ago', self: false },
    { id: 2, from: 'You', text: 'Is this piece available in a custom size?', time: '2d ago', self: true },
    { id: 3, from: producer.name, text: 'Yes, I can make one to order — it takes about 2 weeks.', time: '1d ago', self: false },
  ],
}));

export const followedProducerIds = producers.slice(0, 4).map((p) => p.id);

export const favoriteVillageIds = villages.slice(0, 3).map((v) => v.id);

export const addresses = Array.from({ length: 3 }).map((_, i) => ({
  id: `address-${i + 1}`,
  label: ['Home', 'Office', 'Parents’ House'][i],
  name: 'Ayesha Rahman',
  phone: `+8801${700000000 + i * 111111}`,
  district: districts[i % districts.length].name,
  address: `House ${12 + i}, Road ${4 + i}, ${districts[i % districts.length].name}`,
  isDefault: i === 0,
}));

export const returns = orders.slice(0, 3).map((order, i) => ({
  id: `RET-${5000 + i}`,
  orderId: order.id,
  product: products[i % products.length].name,
  reason: ['Wrong size received', 'Item damaged in transit', 'Changed my mind'][i],
  status: ['Approved', 'Under Review', 'Completed'][i],
  requestedDate: `2026-0${(i % 7) + 1}-1${i}`,
}));

export const refunds = returns.map((ret, i) => ({
  id: `REF-${7000 + i}`,
  orderId: ret.orderId,
  amount: 1200 + i * 650,
  method: ['bKash', 'Card Refund', 'Bank Transfer'][i],
  status: ['Processed', 'Pending', 'Processed'][i],
  date: `2026-0${(i % 7) + 1}-1${i}`,
}));

export const heritageCollectionItems = products.slice(0, 5).map((product, i) => ({
  id: `collect-${i + 1}`,
  productId: product.id,
  product: product.name,
  category: product.category,
  heritageId: `SH-HID-${2026}${String(2000 + i)}`,
  producer: product.producer,
  acquiredDate: `2026-0${(i % 7) + 1}-0${i + 1}`,
  craftId: crafts[i % crafts.length].id,
}));

export const impactStats = [
  { label: 'Artisans Supported', value: '9' },
  { label: 'Villages Reached', value: '6' },
  { label: 'Est. Income Generated', value: '৳ 42,500' },
  { label: 'Crafts Preserved', value: '5' },
];

export const impactedProducerIds = producers.slice(0, 4).map((p) => p.id);

export const heritagePassportStamps = [
  ...villages.slice(0, 4).map((v, i) => ({
    id: `stamp-${v.id}`,
    type: 'Village',
    name: v.name,
    district: v.district,
    collected: i < 3,
    date: i < 3 ? `2026-0${i + 1}-1${i}` : null,
  })),
  ...festivals.slice(0, 3).map((f, i) => ({
    id: `stamp-${f.id}`,
    type: 'Festival',
    name: f.name,
    district: f.district,
    collected: i < 1,
    date: i < 1 ? f.date : null,
  })),
];

export const achievements = [
  { id: 'ach-1', title: 'First Purchase', description: 'Complete your first order on ShilpoHub.', current: 1, target: 1 },
  { id: 'ach-2', title: 'Heritage Explorer', description: 'Read 5 craft or producer stories.', current: 3, target: 5 },
  { id: 'ach-3', title: 'Community Voice', description: 'Post 10 times in the community forum.', current: 4, target: 10 },
  { id: 'ach-4', title: 'Master Collector', description: 'Own 10 heritage-certified products.', current: 5, target: 10 },
  { id: 'ach-5', title: 'Village Wanderer', description: 'Collect passport stamps from 8 heritage villages.', current: 3, target: 8 },
];

export const badges = [
  { id: 'badge-1', name: 'First Order', icon: '🛍️', description: 'Placed your first order.', earned: true },
  { id: 'badge-2', name: 'Heritage Supporter', icon: '🧵', description: 'Purchased from 5 different producers.', earned: true },
  { id: 'badge-3', name: 'Storyteller', icon: '📖', description: 'Read 10 craft stories.', earned: false },
  { id: 'badge-4', name: 'Festival Goer', icon: '🎉', description: 'Attended a heritage festival.', earned: true },
  { id: 'badge-5', name: 'Top Collector', icon: '🏺', description: 'Own products from every district.', earned: false },
  { id: 'badge-6', name: 'Community Pillar', icon: '💬', description: 'Active contributor in discussions.', earned: false },
];
