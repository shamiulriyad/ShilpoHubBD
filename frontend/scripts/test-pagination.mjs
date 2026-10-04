import fs from 'node:fs';
import assert from 'node:assert/strict';

// Exercise the actual pagination calculation without mounting a browser.
const source = fs.readFileSync(new URL('../src/components/ui/Pagination.jsx', import.meta.url), 'utf8');
const body = source.slice(source.indexOf('  const count'), source.indexOf('  return ('));
const calculate = new Function('currentPage', 'totalPages', `${body}return { pages, current, count };`);
for (const total of [1, 5, 50, 625, 5000]) {
  for (const page of [1, 2, Math.ceil(total / 2), total]) {
    const result = calculate(page, total);
    assert(result.pages.length <= 7, 'Pagination must remain bounded for large datasets');
    assert(result.pages.includes(result.current), 'Current page must be reachable');
    assert(result.pages.includes(1) && result.pages.includes(total), 'First and last pages must be reachable');
    assert(result.pages.filter(item => typeof item === 'number').every(item => item >= 1 && item <= total));
  }
}
assert.equal(calculate(-1, 10).current, 1);
assert.equal(calculate(20, 10).current, 10);
console.log('Pagination behavior passed for 1–5,000 pages.');
