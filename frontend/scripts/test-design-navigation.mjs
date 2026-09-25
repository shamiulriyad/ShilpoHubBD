import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { presentNavigation, priorities } from '../src/components/layout/workspaceNavigation.js';
// Read the real navigation module; resolve extensionless imports for Node only.
const source=readFileSync(new URL('../src/data/navigation.js',import.meta.url),'utf8')
 .replace("'../routes/routePaths'",JSON.stringify(pathToFileURL(resolve('src/routes/routePaths.js')).href))
 .replace("'../pages/Admin/adminConfig'",JSON.stringify(pathToFileURL(resolve('src/pages/Admin/adminConfig.js')).href));
const {roleSidebars,sidebarNav}=await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
for(const [role,{nav}] of Object.entries(roleSidebars)) {
 const before=nav.flatMap(group=>group.items || [group]);
 const {primary,secondary}=presentNavigation(nav,role);
 const after=[...primary,...secondary.flatMap(group=>group.items)];
 assert.deepEqual(new Set(after.map(x=>`${x.label}|${x.path}`)),new Set(before.map(x=>`${x.label}|${x.path}`)),`${role}: every existing feature and route retained`);
 assert(primary.length<=5,`${role}: primary navigation stays concise`);
 for(const label of priorities[role]) assert(before.some(item=>item.label===label),`${role}: priority uses an existing feature name: ${label}`);
 assert(primary.every(item=>before.includes(item)),`${role}: no fabricated destination`);
 console.log(`PASS ${role}: ${before.length} destinations retained; ${primary.length} primary links`);
}
assert(presentNavigation(sidebarNav,null).primary.length<=5);
console.log('All role navigation presentation checks passed. No network or authentication mutations.');
