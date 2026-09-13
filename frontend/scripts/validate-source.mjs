import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';
import { parse } from '@babel/parser';

const projectRoot = path.resolve(process.cwd());
const sourceRoot = path.join(projectRoot, 'src');
const extensions = ['.js', '.jsx', '.mjs'];
const assetExtensions = ['.css', '.json', '.svg', '.png', '.jpg', '.jpeg', '.webp', '.gif'];
const failures = [];
const parsedModules = new Map();
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

function parseModule(file) {
  if (parsedModules.has(file)) return parsedModules.get(file);
  const source = fs.readFileSync(file, 'utf8');
  const ast = parse(source, {
    sourceType: 'module',
    plugins: ['jsx', 'importMeta', 'topLevelAwait'],
    errorRecovery: false,
  });
  const module = { source, ast };
  parsedModules.set(file, module);
  return module;
}

function resolveRelativeImport(importer, specifier) {
  const base = path.resolve(path.dirname(importer), specifier);
  const candidates = [
    base,
    ...extensions.map((extension) => `${base}${extension}`),
    ...extensions.map((extension) => path.join(base, `index${extension}`)),
    ...assetExtensions.map((extension) => `${base}${extension}`),
  ];
  return candidates.find((candidate) => fs.existsSync(candidate) && fs.statSync(candidate).isFile()) || null;
}

function declarationExportNames(declaration) {
  if (!declaration) return [];
  if (declaration.id?.name) return [declaration.id.name];
  if (declaration.type === 'VariableDeclaration') {
    return declaration.declarations.flatMap((item) => (item.id?.type === 'Identifier' ? [item.id.name] : []));
  }
  return [];
}

function collectExports(file, seen = new Set()) {
  if (seen.has(file)) return { names: new Set(), hasDefault: false, hasWildcard: false };
  seen.add(file);

  const { ast } = parseModule(file);
  const names = new Set();
  let hasDefault = false;
  let hasWildcard = false;

  for (const statement of ast.program.body) {
    if (statement.type === 'ExportDefaultDeclaration') {
      hasDefault = true;
      continue;
    }

    if (statement.type === 'ExportNamedDeclaration') {
      for (const name of declarationExportNames(statement.declaration)) names.add(name);
      for (const specifier of statement.specifiers || []) {
        if (specifier.exported?.type === 'Identifier') names.add(specifier.exported.name);
        else if (specifier.exported?.type === 'StringLiteral') names.add(specifier.exported.value);
      }
      continue;
    }

    if (statement.type === 'ExportAllDeclaration') {
      const target = statement.source?.value?.startsWith('.')
        ? resolveRelativeImport(file, statement.source.value)
        : null;
      if (!target) {
        hasWildcard = true;
        continue;
      }
      const nested = collectExports(target, new Set(seen));
      for (const name of nested.names) names.add(name);
      if (nested.hasWildcard) hasWildcard = true;
    }
  }

  return { names, hasDefault, hasWildcard };
}

if (!fs.existsSync(sourceRoot)) {
  console.error('Source directory not found:', sourceRoot);
  process.exit(1);
}

const files = walk(sourceRoot);

for (const file of files) {
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
    ast = parseModule(file).ast;
  } catch (error) {
    addFailure(file, error.loc?.line, `syntax error: ${error.message}`);
    continue;
  }

  for (const statement of ast.program.body) {
    if (statement.type !== 'ImportDeclaration' || !statement.source.value.startsWith('.')) continue;
    const target = resolveRelativeImport(file, statement.source.value);
    if (!target) {
      addFailure(file, statement.loc?.start.line, `missing relative import: ${statement.source.value}`);
      continue;
    }

    if (!extensions.includes(path.extname(target))) continue;

    let targetExports;
    try {
      targetExports = collectExports(target);
    } catch (error) {
      addFailure(file, statement.loc?.start.line, `could not inspect imported module ${statement.source.value}: ${error.message}`);
      continue;
    }

    for (const specifier of statement.specifiers) {
      if (specifier.type === 'ImportDefaultSpecifier' && !targetExports.hasDefault) {
        addFailure(file, statement.loc?.start.line, `default import is not exported by ${statement.source.value}`);
      }
      if (specifier.type === 'ImportSpecifier') {
        const importedName = specifier.imported?.name || specifier.imported?.value;
        if (!targetExports.hasWildcard && importedName && !targetExports.names.has(importedName)) {
          addFailure(file, statement.loc?.start.line, `"${importedName}" is not exported by ${statement.source.value}`);
        }
      }
    }
  }

  visit(ast, [], file);
}

if (failures.length > 0) {
  console.error(`Source validation failed with ${failures.length} issue(s):`);
  for (const failure of failures) console.error(`  - ${failure}`);
  process.exit(1);
}

console.log(`Source validation passed: ${checkedFiles} modules checked, including relative import/export contracts.`);
