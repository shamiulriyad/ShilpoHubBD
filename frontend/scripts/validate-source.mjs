import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';
import { parse } from '@babel/parser';

const projectRoot = path.resolve(process.cwd());
const sourceRoot = path.join(projectRoot, 'src');
const extensions = ['.js', '.jsx', '.mjs'];
const failures = [];
let checkedFiles = 0;

function walk(directory) {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) return walk(fullPath);
    return extensions.includes(path.extname(entry.name)) ? [fullPath] : [];
  });
}

function relative(file) {
  return path.relative(projectRoot, file).replaceAll(path.sep, '/');
}

function addFailure(file, line, message) {
  failures.push(`${relative(file)}${line ? `:${line}` : ''} — ${message}`);
}

function jsxName(node) {
  if (!node) return null;
  if (node.type === 'JSXIdentifier') return node.name;
  if (node.type === 'JSXMemberExpression') return `${jsxName(node.object)}.${jsxName(node.property)}`;
  return null;
}

function hasAttribute(openingElement, names) {
  const accepted = new Set(names);
  return openingElement.attributes.some(
    (attribute) => attribute.type === 'JSXAttribute' && accepted.has(attribute.name?.name),
  );
}

function getStringAttribute(openingElement, name) {
  const attribute = openingElement.attributes.find(
    (item) => item.type === 'JSXAttribute' && item.name?.name === name,
  );
  return attribute?.value?.type === 'StringLiteral' ? attribute.value.value : null;
}

function hasAncestor(ancestors, names) {
  return ancestors.some(
    (ancestor) => ancestor.type === 'JSXElement' && names.includes(jsxName(ancestor.openingElement.name)),
  );
}

function visit(node, ancestors, file) {
  if (!node || typeof node !== 'object') return;

  if (node.type === 'DebuggerStatement') {
    addFailure(file, node.loc?.start.line, 'debugger statement left in application source');
  }

  if (
    node.type === 'CallExpression' &&
    node.callee?.type === 'MemberExpression' &&
    node.callee.object?.name === 'console' &&
    node.callee.property?.name === 'log'
  ) {
    addFailure(file, node.loc?.start.line, 'console.log left in application source');
  }

  if (node.type === 'CallExpression' && node.callee?.type === 'Identifier' && node.callee.name === 'alert') {
    addFailure(file, node.loc?.start.line, 'browser alert() used instead of application feedback');
  }

  let nextAncestors = ancestors;
  if (node.type === 'JSXElement') {
    const opening = node.openingElement;
    const name = jsxName(opening.name);

    if (['input', 'textarea', 'select'].includes(name)) {
      const named = hasAttribute(opening, ['aria-label', 'aria-labelledby', 'id', 'name']);
      const wrappedByLabel = hasAncestor(ancestors, ['label']);
      if (!named && !wrappedByLabel) {
        addFailure(file, opening.loc?.start.line, `<${name}> has no accessible name`);
      }
    }

    if (name === 'a' || name === 'Link' || name === 'NavLink') {
      const target = getStringAttribute(opening, name === 'a' ? 'href' : 'to');
      if (target === '#') addFailure(file, opening.loc?.start.line, `${name} points to a dead "#" target`);
    }

    if (name === 'img' && relative(file) !== 'src/components/media/SafeImage.jsx') {
      addFailure(file, opening.loc?.start.line, 'raw <img> bypasses the shared broken-image fallback');
    }

    nextAncestors = [...ancestors, node];
  }

  for (const [key, value] of Object.entries(node)) {
    if (['loc', 'start', 'end', 'openingElement', 'closingElement'].includes(key)) continue;
    if (Array.isArray(value)) {
      for (const child of value) visit(child, nextAncestors, file);
    } else if (value && typeof value === 'object' && value.type) {
      visit(value, nextAncestors, file);
    }
  }
}

if (!fs.existsSync(sourceRoot)) {
  console.error('Source directory not found:', sourceRoot);
  process.exit(1);
}

for (const file of walk(sourceRoot)) {
  checkedFiles += 1;
  const source = fs.readFileSync(file, 'utf8');

  for (const match of source.matchAll(/\b(TODO|FIXME|debugger)\b/g)) {
    const line = source.slice(0, match.index).split('\n').length;
    addFailure(file, line, `${match[1]} development leftover found`);
  }

  if (relative(file) !== 'src/config/runtime.js' && /https?:\/\/localhost(?::\d+)?/i.test(source)) {
    addFailure(file, null, 'hard-coded localhost URL found outside runtime configuration');
  }

  let ast;
  try {
    ast = parse(source, {
      sourceType: 'module',
      plugins: ['jsx', 'importMeta', 'topLevelAwait'],
      errorRecovery: false,
    });
  } catch (error) {
    addFailure(file, error.loc?.line, `syntax error: ${error.message}`);
    continue;
  }

  for (const statement of ast.program.body) {
    if (statement.type !== 'ImportDeclaration' || !statement.source.value.startsWith('.')) continue;
    const base = path.resolve(path.dirname(file), statement.source.value);
    const candidates = [
      base,
      ...extensions.map((extension) => `${base}${extension}`),
      ...extensions.map((extension) => path.join(base, `index${extension}`)),
    ];
    if (!candidates.some((candidate) => fs.existsSync(candidate))) {
      addFailure(file, statement.loc?.start.line, `missing relative import: ${statement.source.value}`);
    }
  }

  visit(ast, [], file);
}

if (failures.length > 0) {
  console.error(`Source validation failed with ${failures.length} issue(s):`);
  for (const failure of failures) console.error(`  - ${failure}`);
  process.exit(1);
}

console.log(`Source validation passed: ${checkedFiles} modules checked.`);