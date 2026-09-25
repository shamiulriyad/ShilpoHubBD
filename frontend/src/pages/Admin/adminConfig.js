const text = (key, label, required = true, extra = {}) => ({
  key,
  label,
  required,
  ...extra
});
const long = (key, label, required = true, maxLength = 4000) => text(key, label, required, {
  type: 'textarea',
  maxLength
});
const bool = (key, label) => text(key, label, false, {
  type: 'checkbox'
});
const date = (key, label, required = true) => text(key, label, required, {
  type: 'datetime-local'
});
const image = text('imageUrl', 'Image URL', false, {
  maxLength: 1000,
  type: 'image-upload'
});
const order = text('displayOrder', 'Display order', true, {
  type: 'number',
  default: 0
});
const active = bool('isActive', 'Active');
const title = text('title', 'Title', true, {
  maxLength: 200
});
const name = text('name', 'Name', true, {
  maxLength: 200
});
const district = text('districtId', 'District', true, {
  lookup: '/districts'
});
const publish = bool('publish', 'Publish now');
export const adminGroups = [['users', 'User Management', [['directory', 'User directory'], ['verification', 'User Verification'], ['roles', 'Role Management'], ['permissions', 'Permissions'], ['identity', 'Identity Verification']]], ['heritage', 'Heritage Management', [['categories', 'Craft Categories'], ['villages', 'Heritage Villages'], ['places', 'Heritage Places'], ['locations', 'Tourism Locations'], ['districts', 'Districts'], ['festivals', 'Festivals'], ['culturalEvents', 'Cultural Events'], ['routes', 'Tour Routes'], ['cuisines', 'Local Cuisines'], ['museum', 'Digital Museum'], ['unesco', 'UNESCO Heritage'], ['craftHeritage', 'Craft Heritage']]], ['marketplace', 'Marketplace', [['approval', 'Product Approval'], ['monitoring', 'Marketplace Monitoring'], ['refunds', 'Refund Management'], ['fraud', 'Fraud Control']]], ['cms', 'CMS', [['homepage', 'Homepage'], ['blogs', 'Blogs'], ['news', 'News'], ['events', 'Events'], ['announcements', 'Announcements'], ['siteContent', 'Site Content']]], ['moderation', 'AI Moderation', [['reviews', 'Fake Reviews'], ['spam', 'Spam Detection'], ['content', 'Content Moderation'], ['images', 'Image Moderation']]], ['security', 'Security', [['audit', 'Audit Logs'], ['backups', 'Backups'], ['health', 'System Monitoring'], ['keys', 'API Management'], ['threats', 'Threat Detection']]]];
export const resources = {
  categories: {
    path: '/categories',
    fields: [name, long('description', 'Description', false), image, order],
    update: [active],
    columns: ['name', 'description', 'displayOrder', 'isActive']
  },
  villages: {
    path: '/villages',
    fields: [name, text('craft', 'Craft'), district, long('description', 'Description', false), image],
    update: [active],
    columns: ['name', 'craft', 'districtName', 'isActive']
  },
  places: {
    path: '/heritage-places',
    fields: [name, long('description', 'Description'), text('placeType', 'Place type', true, { options: ['Village', 'HistoricalSite', 'Museum', 'Temple', 'Monument', 'CraftCenter', 'NaturalSite', 'Other'] }), district, text('address', 'Address', false), text('latitude', 'Latitude', true, { type: 'number', min: -90, max: 90 }), text('longitude', 'Longitude', true, { type: 'number', min: -180, max: 180 }), image, bool('isFeatured', 'Featured')],
    update: [active],
    columns: ['name', 'placeType', 'districtName', 'isFeatured', 'isActive']
  },
  locations: {
    path: '/tourism-locations',
    fields: [name, long('description', 'Description'), text('type', 'Type', true, { options: ['Hotel', 'Resort', 'Hostel', 'TouristPlace', 'HeritageSite', 'Restaurant', 'Attraction'] }), district, text('address', 'Address', false), text('latitude', 'Latitude', true, { type: 'number', min: -90, max: 90 }), text('longitude', 'Longitude', true, { type: 'number', min: -180, max: 180 }), text('price', 'Price / price per night', false, { type: 'number' }), text('entryFee', 'Entry fee', false, { type: 'number' }), text('openingHours', 'Opening hours', false), text('contactInfo', 'Contact info', false), long('facilities', 'Facilities', false, 1000), image],
    update: [active, bool('isVerified', 'Verified')],
    columns: ['name', 'type', 'districtName', 'isActive', 'isVerified']
  },
  districts: {
    path: '/districts',
    params: {
      includeInactive: true
    },
    noCreate: true,
    noDelete: true,
    noDetail: true,
    fields: [text('division', 'Division'), order, active],
    columns: ['name', 'division', 'displayOrder', 'isActive']
  },
  festivals: {
    path: '/heritage-festivals',
    params: {
      activeOnly: false
    },
    params: {
      activeOnly: false
    },
    fields: [name, long('description', 'Description'), district, text('heritagePlaceId', 'Heritage place', false, {
      lookup: '/heritage-places'
    }), date('startDate', 'Starts'), date('endDate', 'Ends'), bool('isRecurringAnnually', 'Repeats annually'), image],
    update: [active],
    columns: ['name', 'districtName', 'startDate', 'endDate', 'isActive']
  },
  culturalEvents: {
    path: '/cultural-events',
    fields: [name, long('description', 'Description'), text('category', 'Category', true, { maxLength: 100 }), district, text('heritagePlaceId', 'Heritage place', false, { lookup: '/heritage-places' }), date('eventDate', 'Starts'), date('endDate', 'Ends', false), image],
    update: [active],
    columns: ['name', 'category', 'districtName', 'eventDate', 'isActive']
  },
  routes: {
    path: '/heritage-routes',
    fields: [name, long('description', 'Description'), text('estimatedDurationMinutes', 'Estimated duration (minutes)', true, { type: 'number', min: 1 }), bool('isRecommended', 'Recommended')],
    update: [text('status', 'Status', true, { options: ['Draft', 'Published', 'Archived'] })],
    columns: ['name', 'estimatedDurationMinutes', 'totalDistanceKm', 'isRecommended', 'status']
  },
  cuisines: {
    path: '/local-cuisines',
    fields: [name, long('description', 'Description'), district, text('heritagePlaceId', 'Heritage place', false, { lookup: '/heritage-places' }), text('whereToTry', 'Where to try', false, { maxLength: 500 }), image],
    update: [active],
    columns: ['name', 'districtName', 'whereToTry', 'isActive']
  },
  museum: {
    path: '/museum-items',
    fields: [title, long('description', 'Description'), text('category', 'Category', true, { maxLength: 100 }), text('era', 'Era', false, { maxLength: 100 }), district, text('coverImageUrl', 'Cover image', true, { maxLength: 1000, type: 'image-upload' }), text('modelUrl', '3D model URL', false, { maxLength: 1000 }), bool('isFeatured', 'Featured')],
    update: [active],
    columns: ['title', 'category', 'era', 'districtName', 'isFeatured', 'isActive']
  },
  unesco: {
    path: '/unesco-records',
    params: { includeInactive: true },
    fields: [title, text('type', 'Type', true, { options: ['CulturalHeritageSite', 'NaturalHeritageSite', 'IntangibleCulturalHeritage', 'MemoryOfTheWorld'] }), long('description', 'Description'), text('inscribedYear', 'Inscribed year', true, { type: 'number', min: 1972, max: 2100 }), text('districtId', 'District', false, { lookup: '/districts' }), image, text('officialUrl', 'Official UNESCO URL', false, { maxLength: 1000 }), order],
    update: [active],
    columns: ['title', 'type', 'inscribedYear', 'displayOrder', 'isActive']
  },
  craftHeritage: {
    path: '/craft-heritage',
    params: { includeInactive: true },
    fields: [text('slug', 'Slug (matches the craft category)', true, { maxLength: 100 }), name, text('aliases', 'Also known as (comma separated)', false, { maxLength: 500 }), text('region', 'Region', true, { maxLength: 200 }), text('type', 'Heritage type', true, { maxLength: 100 }), text('giName', 'GI registration name', false, { maxLength: 300 }), text('unesco', 'UNESCO inscription', false, { maxLength: 300 }), long('summary', 'Summary', true, 2000), long('history', 'History & community', false), long('materials', 'Materials', false, 2000), long('process', 'How it is made', false), long('products', 'Products', false, 2000), long('story', 'Story', false), long('visit', 'Visiting respectfully', false, 2000), long('sources', 'Sources (one per line: Label | https://url)', false), order],
    update: [active],
    columns: ['name', 'type', 'region', 'giName', 'displayOrder', 'isActive']
  },
  siteContent: {
    path: '/cms/site-content',
    params: { includeInactive: true },
    fields: [text('group', 'Group', true, { options: ['about-stat', 'about-purpose', 'about-landscape', 'about-stakeholder', 'about-capability', 'footer-about', 'footer-explore', 'footer-marketplace', 'footer-resources', 'travel-resource'] }), text('title', 'Title / label / value', true, { maxLength: 300 }), text('subtitle', 'Subtitle / caption', false, { maxLength: 300 }), long('body', 'Text', false), text('linkUrl', 'Link (path like /tourism or full URL)', false, { maxLength: 1000 }), long('extra', 'Extra (flip-card back text, "highlight", or travel resource "Type|Button text")', false), order],
    update: [active],
    columns: ['group', 'title', 'subtitle', 'displayOrder', 'isActive']
  },
  homepage: {
    path: '/cms/homepage',
    params: {
      includeInactive: true
    },
    fields: [text('sectionKey', 'Section key', true, {
      maxLength: 100
    }), title, text('subtitle', 'Subtitle', false, {
      maxLength: 500
    }), image, text('linkUrl', 'Link URL', false, {
      maxLength: 1000
    }), order],
    update: [active],
    immutable: ['sectionKey'],
    columns: ['sectionKey', 'title', 'displayOrder', 'isActive']
  },
  blogs: {
    path: '/cms/blogs',
    drafts: true,
    fields: [title, long('summary', 'Summary', true, 500), long('content', 'Article content', true, 20000), text('coverImageUrl', 'Cover image URL', false, {
      maxLength: 1000
    }), text('tags', 'Tags', false, {
      maxLength: 500
    }), publish],
    columns: ['title', 'authorName', 'isPublished', 'createdAt']
  },
  news: {
    path: '/cms/news',
    drafts: true,
    fields: [title, long('summary', 'Summary', true, 500), long('content', 'Article content', true, 20000), image, text('source', 'Source', false, {
      maxLength: 200
    }), publish],
    columns: ['title', 'source', 'isPublished', 'createdAt']
  },
  events: {
    path: '/cms/events',
    drafts: true,
    fields: [title, long('description', 'Description'), image, text('location', 'Location', false, {
      maxLength: 300
    }), date('startDate', 'Starts'), date('endDate', 'Ends'), publish],
    columns: ['title', 'location', 'startDate', 'endDate', 'isPublished']
  },
  announcements: {
    path: '/cms/announcements',
    params: {
      activeOnly: false
    },
    fields: [title, long('message', 'Message', true, 2000), text('severity', 'Severity', true, {
      options: ['Info', 'Warning', 'Critical'],
      default: 'Info'
    }), date('startsAt', 'Starts', false), date('endsAt', 'Ends', false)],
    update: [active],
    columns: ['title', 'severity', 'startsAt', 'endsAt', 'isActive']
  }
};
export function editorFields(config, editing) {
  return [...config.fields.filter(f => !editing || !config.immutable?.includes(f.key)).map(f => editing && f.key === 'publish' ? {
    ...f,
    key: 'isPublished',
    label: 'Published'
  } : f), ...(editing ? config.update || [] : [])];
}
export function formPayload(fields, values) {
  return Object.fromEntries(fields.map(f => {
    const value = values[f.key];
    return [f.key, f.type === 'checkbox' ? Boolean(value) : f.type === 'number' ? value === '' && !f.required ? null : Number(value) : f.type === 'datetime-local' ? value ? new Date(value).toISOString() : null : String(value ?? '').trim() || (f.required ? '' : null)];
  }));
}
export const flagViews = {
  fraud: {
    flagType: 'FraudRisk',
    scanType: 'Fraud'
  },
  reviews: {
    flagType: 'ReviewAbuse',
    scanType: 'ReviewAbuse'
  },
  spam: {
    flagType: 'SpamContent',
    scanType: 'SpamContent'
  },
  content: {
    flagType: 'PolicyViolation',
    scanType: 'PolicyViolation'
  },
  images: {
    flagType: 'InappropriateImage',
    scanType: 'InappropriateImage'
  }
};
