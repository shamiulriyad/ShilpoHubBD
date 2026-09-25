// Craft heritage articles are admin-managed (Admin › Heritage Management › Craft Heritage) and loaded through
// useCraftHeritage(). This module only holds the helpers that work on those records.

// The API stores aliases as "a, b" and sources as one "Label | url" per line; the pages want arrays.
export function toCraftHeritage(entry) {
  return {
    ...entry,
    aliases: (entry.aliases || '').split(',').map(a => a.trim()).filter(Boolean),
    sources: (entry.sources || '').split('\n').map(line => line.split('|').map(part => part.trim())).filter(([label, url]) => label && url),
  };
}

export function heritageForCategory(category, records = []) {
  const key = (category?.slug || '').toLowerCase();
  const name = (category?.name || '').toLowerCase();
  return records.find(c => c.slug === key || [c.name, ...(c.aliases || [])].some(n => n.toLowerCase() === name));
}

export function filterCraftHeritage(records, query, recognition) {
  const term = query.trim().toLocaleLowerCase();
  return records.filter(c => (!term || [c.name, c.region, c.type, c.summary, c.materials, ...(c.aliases || [])].join(' ').toLocaleLowerCase().includes(term))
    && (recognition === 'all' || (recognition === 'gi' && c.giName) || (recognition === 'unesco' && c.unesco)));
}
