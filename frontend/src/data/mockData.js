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
  currentBid: 4200 + i * 900,
  bidsCount: 6 + i * 3,
  closesIn: `${2 + i}d ${5 * i}h`,
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
