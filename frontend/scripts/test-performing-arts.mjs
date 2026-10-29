// Contract + unit checks for the Performing Arts module: frontend enums/divisions/payloads match the backend C#
// sources, bilingual fallback never mislabels English as Bangla, media validation mirrors PerformingArtRules,
// workflow buttons match the server transitions, and routes/nav/admin/assistant wiring is in place.
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { resolve, dirname } from 'node:path';
import {
  CATEGORIES, DIVISIONS, MEDIA_TYPES, SOURCE_TYPES, STATUSES, categoryLabel, divisionLabel, isValidMediaUrl, pickLocalized,
  placeLabel, primaryTitle, secondaryTitle, slugify, toFormValues, toSavePayload, validateForm, videoEmbedUrl, workflowActions,
} from '../src/utils/performingArtsAdapters.js';
import { adminGroups } from '../src/pages/Admin/adminConfig.js';
import { routePaths } from '../src/routes/routePaths.js';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const backend = resolve(root, '../backend/src');
const read = (p) => readFileSync(p, 'utf8');
let checks = 0;
const check = (name, fn) => { fn(); checks++; console.log(`PASS ${name}`); };

const enumValues = (source, name) => {
  const body = new RegExp(`enum ${name}\\s*\\{([^}]*)\\}`).exec(source)?.[1];
  assert(body, `enum ${name} not found`);
  return body.split(',').map((v) => v.trim()).filter(Boolean);
};
const classProps = (source, name) => {
  const start = source.indexOf(`class ${name}`);
  assert(start >= 0, `class ${name} not found`);
  const body = source.slice(start, source.indexOf('\n}', start));
  return [...body.matchAll(/public\s+[\w?<>]+\s+(\w+)\s*\{\s*get;/g)].map((m) => m[1][0].toLowerCase() + m[1].slice(1)).sort();
};

const domain = read(resolve(backend, 'ShilpoHubBD.Domain/Entities/PerformingArts/PerformingArtRecord.cs'));
const sourceEntity = read(resolve(backend, 'ShilpoHubBD.Domain/Entities/PerformingArts/PerformingArtSource.cs'));
const mediaEntity = read(resolve(backend, 'ShilpoHubBD.Domain/Entities/PerformingArts/PerformingArtMedia.cs'));
const dtos = read(resolve(backend, 'ShilpoHubBD.Application/DTOs/PerformingArts/PerformingArtDtos.cs'));
const rules = read(resolve(backend, 'ShilpoHubBD.Application/Services/PerformingArts/PerformingArtRules.cs'));
const controller = read(resolve(backend, 'ShilpoHubBD.Api/Controllers/PerformingArtsController.cs'));

// ---------- backend contract ----------
check('categories match the backend enum', () => assert.deepEqual(CATEGORIES.map((c) => c.value), enumValues(domain, 'PerformingArtCategory')));
check('statuses match the backend enum', () => assert.deepEqual(STATUSES, enumValues(domain, 'PerformingArtStatus')));
check('source types match the backend enum', () => assert.deepEqual(SOURCE_TYPES, enumValues(sourceEntity, 'PerformingArtSourceType')));
check('media types match the backend enum', () => assert.deepEqual(MEDIA_TYPES, enumValues(mediaEntity, 'PerformingArtMediaType')));
check('divisions match PerformingArtRules.Divisions', () => {
  const list = /Divisions\s*=\s*new\[\]\s*\{([^}]*)\}/.exec(rules)[1].match(/"([^"]+)"/g).map((s) => s.slice(1, -1));
  assert.deepEqual(DIVISIONS.map((d) => d.value), list);
});
check('save payload keys match SavePerformingArtRequest', () => {
  assert.deepEqual(Object.keys(toSavePayload(toFormValues({}))).sort(), classProps(dtos, 'SavePerformingArtRequest'));
});
check('source and media payload keys match their request DTOs', () => {
  const payload = toSavePayload({ ...toFormValues({}), sources: [{ title: 'x' }], media: [{ url: 'https://a.org/x.png', credit: 'c' }] });
  assert.deepEqual(Object.keys(payload.sources[0]).sort(), classProps(dtos, 'SavePerformingArtSourceRequest'));
  assert.deepEqual(Object.keys(payload.media[0]).sort(), classProps(dtos, 'SavePerformingArtMediaRequest'));
});
check('service endpoints exist on the controllers', () => {
  const service = read(resolve(root, 'src/services/performingArtsService.js'));
  assert(controller.includes('[Route("api/performing-arts")]') && service.includes("'/performing-arts'"));
  assert(controller.includes('[Route("api/admin/performing-arts")]') && service.includes("'/admin/performing-arts'"));
  for (const t of ['featured', 'facets', '{slug}']) assert(controller.includes(`HttpGet("${t}")`), t);
  for (const action of ['submit', 'publish', 'return', 'archive', 'restore']) assert(controller.includes(`HttpPost("{id:guid}/${action}")`), action);
});
check('admin controller is Super Admin only; public controller is anonymous', () => {
  assert.match(controller, /\[Authorize\(Roles = RoleNames\.SuperAdmin\)\]\s*\[Route\("api\/admin\/performing-arts"\)\]/);
  assert.match(controller, /\[AllowAnonymous\][\s\S]*\[Route\("api\/performing-arts"\)\]/);
});
check('workflow buttons only offer transitions the server allows', () => {
  assert.deepEqual(workflowActions('Draft'), ['submit', 'archive', 'delete']);
  assert.deepEqual(workflowActions('InReview'), ['publish', 'return', 'archive']);
  assert.deepEqual(workflowActions('Published'), ['archive']);
  assert.deepEqual(workflowActions('Archived'), ['restore', 'delete']);
  assert(!workflowActions('Draft').includes('publish'), 'Draft cannot be published without review');
});

// ---------- bilingual content ----------
const record = { titleEn: 'Dhamer Gaan', titleBn: 'ধামের গান', summaryEn: 'English summary', summaryBn: '', division: 'Rangpur', districtName: 'Thakurgaon' };
check('Bangla mode uses the Bangla field when present', () => {
  assert.deepEqual(pickLocalized({ summaryEn: 'e', summaryBn: 'বাংলা' }, 'summary', 'bn'), { value: 'বাংলা', lang: 'bn', isFallback: false });
});
check('Bangla mode falls back to English, flagged and tagged lang=en', () => {
  assert.deepEqual(pickLocalized(record, 'summary', 'bn'), { value: 'English summary', lang: 'en', isFallback: true });
});
check('English mode never shows Bangla-only text as English', () => {
  assert.deepEqual(pickLocalized({ summaryEn: '', summaryBn: 'বাংলা' }, 'summary', 'en'), { value: '', lang: 'en', isFallback: false });
});
check('titles swap primary/secondary by language', () => {
  assert.equal(primaryTitle(record, 'bn'), 'ধামের গান');
  assert.equal(secondaryTitle(record, 'bn'), 'Dhamer Gaan');
  assert.equal(primaryTitle(record, 'en'), 'Dhamer Gaan');
  assert.equal(secondaryTitle({ titleEn: 'Only English' }, 'en'), '');
});
check('category, division and place labels are bilingual', () => {
  assert.equal(categoryLabel('FolkTheatre', 'bn'), 'লোকনাট্য');
  assert.equal(categoryLabel('FolkTheatre', 'en'), 'Folk theatre');
  assert.equal(divisionLabel('Rangpur', 'bn'), 'রংপুর');
  assert.equal(placeLabel(record, 'en'), 'Thakurgaon, Rangpur Division');
  assert.equal(placeLabel(record, 'bn'), 'Thakurgaon, রংপুর বিভাগ');
});

// ---------- media validation (mirrors PerformingArtRules.IsValidMediaUrl) ----------
check('media URL rules match the server', () => {
  const ok = [['Image', 'https://example.org/a.JPG'], ['Image', '/uploads/images/x.webp'], ['Audio', 'https://e.org/s.mp3'],
    ['Video', 'https://www.youtube.com/watch?v=abc'], ['Video', 'https://youtu.be/abc'], ['Video', 'https://e.org/c.webm']];
  const bad = [['Image', 'http://example.org/a.jpg'], ['Image', 'javascript:alert(1)'], ['Image', '/uploads/../secret.jpg'],
    ['Image', 'https://e.org/s.mp3'], ['Audio', 'https://e.org/a.jpg'], ['Video', 'https://e.org/watch'], ['Image', ''], ['Image', 'data:image/png;base64,AAA']];
  for (const [t, u] of ok) assert(isValidMediaUrl(t, u), `${t} ${u} should be valid`);
  for (const [t, u] of bad) assert(!isValidMediaUrl(t, u), `${t} ${u} should be rejected`);
});
check('video embeds use privacy-friendly hosts only', () => {
  assert.equal(videoEmbedUrl('https://www.youtube.com/watch?v=abc'), 'https://www.youtube-nocookie.com/embed/abc');
  assert.equal(videoEmbedUrl('https://youtu.be/xyz'), 'https://www.youtube-nocookie.com/embed/xyz');
  assert.equal(videoEmbedUrl('https://vimeo.com/123456'), 'https://player.vimeo.com/video/123456');
  assert.equal(videoEmbedUrl('https://e.org/clip.mp4'), null);
});

// ---------- admin form ----------
check('form validation mirrors the server: slug, required English, Bangla script, sources, media credit', () => {
  const base = { ...toFormValues({}), slug: 'dhamer-gaan', titleEn: 'Dhamer Gaan', summaryEn: 'Summary' };
  assert.deepEqual(validateForm(base), []);
  assert(validateForm({ ...base, slug: 'Dhamer Gaan' }).length);
  assert(validateForm({ ...base, slug: 'featured' }).some((e) => e.includes('reserved')));
  assert(validateForm({ ...base, titleEn: ' ' }).length);
  assert(validateForm({ ...base, summaryBn: 'not bangla' }).some((e) => e.includes('Bangla')));
  assert.deepEqual(validateForm({ ...base, summaryBn: 'বাংলা সারাংশ' }), []);
  assert(validateForm({ ...base, sources: [{ title: '' }] }).length);
  assert(validateForm({ ...base, sources: [{ title: 't', url: 'ftp://x' }] }).length);
  assert(validateForm({ ...base, media: [{ mediaType: 'Image', url: 'https://a.org/x.png', credit: '' }] }).some((e) => e.includes('credit')));
});
check('payload trims, nulls empty optionals, keeps order and booleans', () => {
  const payload = toSavePayload({ ...toFormValues({}), slug: ' Dhamer-Gaan ', titleEn: ' T ', summaryEn: 'S', titleBn: '  ', isFeatured: true, displayOrder: '3',
    sources: [{ sourceType: 'Book', title: ' A ', accessedOn: '2026-10-30' }, { title: 'B' }] });
  assert.equal(payload.slug, 'dhamer-gaan');
  assert.equal(payload.titleEn, 'T');
  assert.equal(payload.titleBn, null);
  assert.equal(payload.isFeatured, true);
  assert.equal(payload.displayOrder, 3);
  assert.deepEqual(payload.sources.map((s) => s.title), ['A', 'B']);
  assert.equal(payload.sources[0].accessedOn, '2026-10-30T00:00:00.000Z');
});
check('editing round-trips a detail DTO', () => {
  const detail = { slug: 'x', titleEn: 'X', category: 'FolkTheatre', isFeatured: true, displayOrder: 2, districtId: 'd1',
    sources: [{ id: '1', sourceType: 'Book', title: 'B', accessedOn: '2026-10-30T00:00:00Z' }], media: [{ id: '2', mediaType: 'Audio', url: 'https://a.org/a.mp3', credit: 'c' }] };
  const values = toFormValues(detail);
  assert.equal(values.sources[0].accessedOn, '2026-10-30');
  const payload = toSavePayload(values);
  assert.equal(payload.category, 'FolkTheatre');
  assert.equal(payload.districtId, 'd1');
  assert.equal(payload.media[0].mediaType, 'Audio');
});
check('slugify produces server-valid slugs', () => assert.equal(slugify('Ektara [Gupijantra]'), 'ektara-gupijantra'));

// ---------- wiring ----------
check('public routes are declared and registered', () => {
  assert.equal(routePaths.explorePerformingArts, '/explore/performing-arts');
  assert.equal(routePaths.explorePerformingArtDetails, '/explore/performing-arts/:slug');
  const router = read(resolve(root, 'src/routes/router.jsx'));
  assert(router.includes('path: routePaths.explorePerformingArts, element: <PerformingArts />'));
  assert(router.includes('path: routePaths.explorePerformingArtDetails, element: <PerformingArtDetails />'));
});
check('assistant detail links match the frontend route', () => {
  const composer = read(resolve(backend, 'ShilpoHubBD.Application/Services/PerformingArts/PerformingArtKnowledge.cs'));
  assert(composer.includes(`DetailPathPrefix = "${routePaths.explorePerformingArts}/"`));
});
check('Explore menu links to Performing Arts', () => {
  assert(read(resolve(root, 'src/data/navigation.js')).includes('path: routePaths.explorePerformingArts'));
});
check('admin Heritage Management has a Performing Arts view routed to its workflow page', () => {
  const heritage = adminGroups.find(([key]) => key === 'heritage');
  assert(heritage[2].some(([view]) => view === 'performingArts'));
  assert(read(resolve(root, 'src/pages/Admin/AdminWorkspace.jsx')).includes("view === 'performingArts' ? <AdminPerformingArts />"));
});
check('assistant widget renders only same-site answer links', () => {
  const widget = read(resolve(root, 'src/components/ui/AIAssistantWidget.jsx'));
  assert(widget.includes('<AnswerLinks links={message.links} />'));
  const filter = /data\.links\.filter\(\(link\) => typeof link\?\.url === 'string' && (\/.+\/)\.test\(link\.url\)\)/.exec(widget);
  assert(filter, 'link filter not found');
  const pattern = new RegExp(filter[1].slice(1, -1));
  assert(pattern.test('/explore/performing-arts/dotara'));
  assert(!pattern.test('//evil.example/x'));
  assert(!pattern.test('https://evil.example/x'));
  assert(!pattern.test('javascript:alert(1)'));
});

console.log(`\n${checks} performing-arts checks passed`);
