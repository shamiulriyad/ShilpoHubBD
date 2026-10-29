// Pure helpers for the Performing Arts module (no React, no browser globals) so the node contract tests can
// import them directly. Mirrors the backend rules in PerformingArtRules.cs / PerformingArtValidators.cs.

export const LANGUAGES = ['en', 'bn'];

export const CATEGORIES = [
  { value: 'FolkMusic', en: 'Folk music', bn: 'লোকসংগীত' },
  { value: 'FolkTheatre', en: 'Folk theatre', bn: 'লোকনাট্য' },
  { value: 'TraditionalDance', en: 'Traditional dance', bn: 'ঐতিহ্যবাহী নৃত্য' },
  { value: 'OralPerformance', en: 'Oral performance', bn: 'মৌখিক পরিবেশনা' },
  { value: 'RegionalTradition', en: 'Regional performance tradition', bn: 'আঞ্চলিক পরিবেশনা ঐতিহ্য' },
];

export const DIVISIONS = [
  { value: 'Barishal', bn: 'বরিশাল' },
  { value: 'Chattogram', bn: 'চট্টগ্রাম' },
  { value: 'Dhaka', bn: 'ঢাকা' },
  { value: 'Khulna', bn: 'খুলনা' },
  { value: 'Mymensingh', bn: 'ময়মনসিংহ' },
  { value: 'Rajshahi', bn: 'রাজশাহী' },
  { value: 'Rangpur', bn: 'রংপুর' },
  { value: 'Sylhet', bn: 'সিলেট' },
];

export const STATUSES = ['Draft', 'InReview', 'Published', 'Archived'];
export const SOURCE_TYPES = ['OfficialInventory', 'Book', 'Article', 'Archive', 'Website', 'Other'];
export const MEDIA_TYPES = ['Image', 'Audio', 'Video'];

/** Interface copy. Content itself comes from the record's own English/Bangla fields. */
export const UI_TEXT = {
  en: {
    title: 'Performing Arts',
    description: 'Folk music, theatre, dance and oral performance traditions of Bangladesh, drawn from reviewed and cited heritage records.',
    searchPlaceholder: 'Search by name, community, instrument or place',
    allCategories: 'All categories',
    division: 'Division',
    district: 'District',
    allDivisions: 'All divisions',
    allDistricts: 'All districts',
    featured: 'Featured heritage',
    results: (n) => `${n} record${n === 1 ? '' : 's'}`,
    empty: 'No published performing-arts records match these filters yet.',
    emptyAll: 'No performing-arts records have been published yet.',
    clear: 'Clear filters',
    loading: 'Loading performing arts…',
    readMore: 'View record',
    sources: 'Sources',
    communities: 'Associated communities',
    artists: 'Artists and tradition bearers',
    instruments: 'Instruments and associated elements',
    region: 'Region',
    regionalContext: 'Regional context',
    about: 'About this tradition',
    media: 'Images, audio and video',
    viability: 'Present condition (as recorded)',
    inventoryReference: 'Inventory reference',
    alsoKnownAs: 'Also known as',
    upazila: 'Upazila',
    noSources: 'No sources are listed for this record.',
    credit: 'Credit',
    openSource: 'Open source',
    missingTranslation: 'A reviewed Bangla version of this text has not been added yet; the English text is shown.',
    back: 'All performing arts',
    language: 'Content language',
  },
  bn: {
    title: 'পরিবেশনা শিল্প',
    description: 'যাচাইকৃত ও উৎসভিত্তিক ঐতিহ্য রেকর্ড থেকে বাংলাদেশের লোকসংগীত, লোকনাট্য, নৃত্য ও মৌখিক পরিবেশনার ঐতিহ্য।',
    searchPlaceholder: 'নাম, সম্প্রদায়, বাদ্যযন্ত্র বা স্থান দিয়ে খুঁজুন',
    allCategories: 'সব ধরন',
    division: 'বিভাগ',
    district: 'জেলা',
    allDivisions: 'সব বিভাগ',
    allDistricts: 'সব জেলা',
    featured: 'নির্বাচিত ঐতিহ্য',
    results: (n) => `${n}টি রেকর্ড`,
    empty: 'এই ফিল্টারের সাথে মেলে এমন কোনো প্রকাশিত রেকর্ড এখনও নেই।',
    emptyAll: 'এখনও কোনো পরিবেশনা শিল্পের রেকর্ড প্রকাশিত হয়নি।',
    clear: 'ফিল্টার মুছুন',
    loading: 'পরিবেশনা শিল্প লোড হচ্ছে…',
    readMore: 'রেকর্ড দেখুন',
    sources: 'তথ্যসূত্র',
    communities: 'সংশ্লিষ্ট সম্প্রদায়',
    artists: 'শিল্পী ও ঐতিহ্যধারক',
    instruments: 'বাদ্যযন্ত্র ও সংশ্লিষ্ট উপকরণ',
    region: 'অঞ্চল',
    regionalContext: 'আঞ্চলিক প্রেক্ষাপট',
    about: 'এই ঐতিহ্য সম্পর্কে',
    media: 'ছবি, অডিও ও ভিডিও',
    viability: 'বর্তমান অবস্থা (উৎস অনুযায়ী)',
    inventoryReference: 'তালিকা নম্বর',
    alsoKnownAs: 'অন্য নাম',
    upazila: 'উপজেলা',
    noSources: 'এই রেকর্ডের কোনো তথ্যসূত্র উল্লেখ নেই।',
    credit: 'কৃতজ্ঞতা',
    openSource: 'উৎস খুলুন',
    missingTranslation: 'এই অংশের যাচাইকৃত বাংলা সংস্করণ এখনও যোগ করা হয়নি; ইংরেজি লেখা দেখানো হচ্ছে।',
    back: 'সব পরিবেশনা শিল্প',
    language: 'বিষয়বস্তুর ভাষা',
  },
};

export const text = (lang) => UI_TEXT[lang] || UI_TEXT.en;

export function categoryLabel(value, lang = 'en') {
  const category = CATEGORIES.find((c) => c.value === value);
  return category ? category[lang] || category.en : value || '';
}

export function divisionLabel(value, lang = 'en') {
  if (!value) return '';
  const division = DIVISIONS.find((d) => d.value === value);
  return lang === 'bn' && division ? division.bn : value;
}

/**
 * Picks the `${base}Bn` field in Bangla mode, falling back to `${base}En`. `isFallback` tells the page to mark
 * the English text (lang="en") and say a reviewed translation is missing — never machine-translate on the fly.
 */
export function pickLocalized(record, base, lang) {
  const en = record?.[`${base}En`] || '';
  const bn = record?.[`${base}Bn`] || '';
  if (lang === 'bn' && bn) return { value: bn, lang: 'bn', isFallback: false };
  if (lang === 'bn' && en) return { value: en, lang: 'en', isFallback: true };
  return { value: en, lang: 'en', isFallback: false };
}

export function primaryTitle(record, lang) {
  return lang === 'bn' && record?.titleBn ? record.titleBn : record?.titleEn || '';
}

export function secondaryTitle(record, lang) {
  if (!record?.titleBn) return '';
  return lang === 'bn' ? record.titleEn : record.titleBn;
}

export function placeLabel(record, lang) {
  const division = divisionLabel(record?.division, lang);
  const suffix = division ? (lang === 'bn' ? `${division} বিভাগ` : `${division} Division`) : '';
  return [record?.districtName, suffix].filter(Boolean).join(', ');
}

export const splitList = (value) => String(value || '').split(/[,;\n]/).map((v) => v.trim()).filter(Boolean);

export const containsBangla = (value) => /[ঀ-৿]/.test(String(value || ''));

const IMAGE_EXT = ['.jpg', '.jpeg', '.png', '.webp', '.gif'];
const AUDIO_EXT = ['.mp3', '.ogg', '.oga', '.wav', '.m4a', '.aac'];
const VIDEO_EXT = ['.mp4', '.webm'];
const VIDEO_HOSTS = ['youtube.com', 'www.youtube.com', 'm.youtube.com', 'youtu.be', 'vimeo.com', 'player.vimeo.com'];

function parseMediaUrl(url) {
  const value = String(url || '').trim();
  if (value.startsWith('/uploads/')) {
    if (value.includes('..') || value.includes('\\')) return null;
    return { path: value.split(/[?#]/)[0], host: null };
  }
  try {
    const parsed = new URL(value);
    return parsed.protocol === 'https:' ? { path: parsed.pathname, host: parsed.hostname.toLowerCase() } : null;
  } catch {
    return null;
  }
}

const extensionOf = (path) => {
  const match = /\.[a-z0-9]+$/i.exec(path);
  return match ? match[0].toLowerCase() : '';
};

/** Same rule as PerformingArtRules.IsValidMediaUrl. */
export function isValidMediaUrl(type, url) {
  const parsed = parseMediaUrl(url);
  if (!parsed) return false;
  const ext = extensionOf(parsed.path);
  if (type === 'Image') return IMAGE_EXT.includes(ext);
  if (type === 'Audio') return AUDIO_EXT.includes(ext);
  if (type === 'Video') return VIDEO_EXT.includes(ext) || (parsed.host !== null && VIDEO_HOSTS.includes(parsed.host));
  return false;
}

export const isWebUrl = (value) => {
  try {
    return ['http:', 'https:'].includes(new URL(String(value)).protocol);
  } catch {
    return false;
  }
};

/** YouTube/Vimeo watch links become privacy-friendly embed URLs; anything else is played with <video>. */
export function videoEmbedUrl(url) {
  try {
    const u = new URL(url);
    const host = u.hostname.toLowerCase();
    if (host === 'youtu.be') return `https://www.youtube-nocookie.com/embed/${encodeURIComponent(u.pathname.slice(1))}`;
    if (host.endsWith('youtube.com')) {
      const id = u.searchParams.get('v') || (u.pathname.startsWith('/embed/') ? u.pathname.slice(7) : '');
      return id ? `https://www.youtube-nocookie.com/embed/${encodeURIComponent(id)}` : null;
    }
    if (host === 'vimeo.com') {
      const id = u.pathname.split('/').filter(Boolean)[0];
      return /^\d+$/.test(id || '') ? `https://player.vimeo.com/video/${id}` : null;
    }
    if (host === 'player.vimeo.com') return u.href;
  } catch {
    return null;
  }
  return null;
}

// ---- Admin form <-> SavePerformingArtRequest ----

export const RECORD_FIELDS = [
  'slug', 'titleEn', 'titleBn', 'alternateNames', 'category', 'summaryEn', 'summaryBn', 'descriptionEn', 'descriptionBn',
  'regionalContextEn', 'regionalContextBn', 'division', 'districtId', 'upazila', 'communities', 'artists', 'instruments',
  'viabilityNote', 'inventoryReference', 'isFeatured', 'displayOrder',
];
export const SOURCE_FIELDS = ['sourceType', 'title', 'publisher', 'url', 'accessedOn', 'notes'];
export const MEDIA_FIELDS = ['mediaType', 'url', 'captionEn', 'captionBn', 'credit', 'license', 'sourceUrl'];

export const emptySource = () => ({ sourceType: 'OfficialInventory', title: '', publisher: '', url: '', accessedOn: '', notes: '' });
export const emptyMedia = () => ({ mediaType: 'Image', url: '', captionEn: '', captionBn: '', credit: '', license: '', sourceUrl: '' });

export function toFormValues(detail = {}) {
  const values = Object.fromEntries(RECORD_FIELDS.map((key) => [key, detail[key] ?? '']));
  values.category = detail.category || 'FolkMusic';
  values.isFeatured = Boolean(detail.isFeatured);
  values.displayOrder = detail.displayOrder ?? 0;
  values.sources = (detail.sources || []).map((s) => ({
    ...Object.fromEntries(SOURCE_FIELDS.map((key) => [key, s[key] ?? ''])),
    accessedOn: s.accessedOn ? String(s.accessedOn).slice(0, 10) : '',
  }));
  values.media = (detail.media || []).map((m) => Object.fromEntries(MEDIA_FIELDS.map((key) => [key, m[key] ?? ''])));
  return values;
}

const optional = (value) => {
  const trimmed = String(value ?? '').trim();
  return trimmed ? trimmed : null;
};

export function toSavePayload(values) {
  const payload = Object.fromEntries(RECORD_FIELDS.map((key) => [key, optional(values[key])]));
  payload.slug = String(values.slug || '').trim().toLowerCase();
  payload.titleEn = String(values.titleEn || '').trim();
  payload.summaryEn = String(values.summaryEn || '').trim();
  payload.category = values.category || 'FolkMusic';
  payload.isFeatured = Boolean(values.isFeatured);
  payload.displayOrder = Number(values.displayOrder) || 0;
  payload.sources = (values.sources || []).map((s) => ({
    sourceType: s.sourceType || 'Website',
    title: String(s.title || '').trim(),
    publisher: optional(s.publisher),
    url: optional(s.url),
    accessedOn: s.accessedOn ? new Date(`${s.accessedOn}T00:00:00Z`).toISOString() : null,
    notes: optional(s.notes),
  }));
  payload.media = (values.media || []).map((m) => ({
    mediaType: m.mediaType || 'Image',
    url: String(m.url || '').trim(),
    captionEn: optional(m.captionEn),
    captionBn: optional(m.captionBn),
    credit: String(m.credit || '').trim(),
    license: optional(m.license),
    sourceUrl: optional(m.sourceUrl),
  }));
  return payload;
}

export const slugify = (value) => String(value || '').toLowerCase().normalize('NFKD').replace(/[^\w\s-]/g, '')
  .trim().replace(/[\s_]+/g, '-').replace(/-+/g, '-').replace(/^-|-$/g, '');

/** Client-side mirror of the server validator, so editors see problems before saving. Returns a list of messages. */
export function validateForm(values) {
  const errors = [];
  const slug = String(values.slug || '').trim();
  if (!/^[a-z0-9]+(-[a-z0-9]+)*$/.test(slug)) errors.push('Slug may only contain lowercase letters, numbers and single hyphens.');
  if (['featured', 'facets', 'admin', 'new'].includes(slug)) errors.push('This slug is reserved.');
  if (!String(values.titleEn || '').trim()) errors.push('English title is required.');
  if (!String(values.summaryEn || '').trim()) errors.push('English summary is required.');
  for (const key of ['titleBn', 'summaryBn', 'descriptionBn', 'regionalContextBn']) {
    if (String(values[key] || '').trim() && !containsBangla(values[key])) errors.push(`${key} must be written in Bangla script.`);
  }
  (values.sources || []).forEach((s, i) => {
    if (!String(s.title || '').trim()) errors.push(`Source ${i + 1} needs a title.`);
    if (String(s.url || '').trim() && !isWebUrl(s.url)) errors.push(`Source ${i + 1} URL must be an http(s) address.`);
  });
  (values.media || []).forEach((m, i) => {
    if (!isValidMediaUrl(m.mediaType, m.url)) errors.push(`Media ${i + 1}: the URL is not a valid ${String(m.mediaType || '').toLowerCase()} link (https or /uploads/, matching file type).`);
    if (!String(m.credit || '').trim()) errors.push(`Media ${i + 1} needs a credit line.`);
    if (String(m.captionBn || '').trim() && !containsBangla(m.captionBn)) errors.push(`Media ${i + 1} Bangla caption must be written in Bangla script.`);
  });
  return errors;
}

/** Which workflow actions the admin UI offers for a status (the server enforces the same transitions). */
export function workflowActions(status) {
  switch (status) {
    case 'Draft': return ['submit', 'archive', 'delete'];
    case 'InReview': return ['publish', 'return', 'archive'];
    case 'Published': return ['archive'];
    case 'Archived': return ['restore', 'delete'];
    default: return [];
  }
}
