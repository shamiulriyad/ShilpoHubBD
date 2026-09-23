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
export const adminGroups = [['users', 'User Management', [['directory', 'User directory'], ['verification', 'User Verification'], ['roles', 'Role Management'], ['permissions', 'Permissions'], ['identity', 'Identity Verification']]], ['heritage', 'Heritage Management', [['categories', 'Craft Categories'], ['villages', 'Heritage Villages'], ['places', 'Heritage Places'], ['locations', 'Tourism Locations'], ['districts', 'Districts'], ['festivals', 'Festivals']]], ['marketplace', 'Marketplace', [['approval', 'Product Approval'], ['monitoring', 'Marketplace Monitoring'], ['refunds', 'Refund Management'], ['fraud', 'Fraud Control']]], ['cms', 'CMS', [['homepage', 'Homepage'], ['blogs', 'Blogs'], ['news', 'News'], ['events', 'Events'], ['announcements', 'Announcements']]], ['moderation', 'AI Moderation', [['reviews', 'Fake Reviews'], ['spam', 'Spam Detection'], ['content', 'Content Moderation'], ['images', 'Image Moderation']]], ['security', 'Security', [['audit', 'Audit Logs'], ['backups', 'Backups'], ['health', 'System Monitoring'], ['keys', 'API Management'], ['threats', 'Threat Detection']]]];
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
