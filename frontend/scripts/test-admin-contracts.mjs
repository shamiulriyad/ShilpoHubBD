import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {resolve,dirname} from 'node:path';
import {resources,editorFields,formPayload,flagViews,adminGroups} from '../src/pages/Admin/adminConfig.js';

const root=resolve(dirname(fileURLToPath(import.meta.url)),'..');
const backend=resolve(root,'../backend/src/ShilpoHubBD.Application');
const read=p=>readFileSync(p,'utf8');
let checks=0;
const check=(name,fn)=>{fn();checks++;console.log(`PASS ${name}`);};
const contracts={
  categories:['Marketplace','Category'],villages:['Community','Village'],
  festivals:['HeritageDiscovery','HeritageFestival'],homepage:['Cms','HomepageSection'],
  blogs:['Cms','BlogPost'],news:['Cms','NewsItem'],events:['Cms','CmsEvent'],announcements:['Cms','Announcement'],
};
for(const [view,[folder,type]] of Object.entries(contracts)) {
  for(const editing of [false,true]) {
    check(`${view}: ${editing?'update':'create'} fields match backend DTO`,()=>{
      const source=read(resolve(backend,`DTOs/${folder}/${editing?'Update':'Create'}${type}Request.cs`));
      const properties=[...source.matchAll(/public\s+[\w?<>]+\s+(\w+)\s*\{\s*get;/g)].map(m=>m[1][0].toLowerCase()+m[1].slice(1)).sort();
      assert.deepEqual(editorFields(resources[view],editing).map(f=>f.key).sort(),properties);
    });
  }
}
check('districts only expose supported updates',()=>{
  assert.equal(resources.districts.noCreate,true);assert.equal(resources.districts.noDelete,true);
  assert.deepEqual(resources.districts.fields.map(f=>f.key),['division','displayOrder','isActive']);
});
check('CMS updates use isPublished; creation uses publish',()=>{
  for(const view of ['blogs','news','events']) {
    assert(editorFields(resources[view],false).some(f=>f.key==='publish'));
    assert(editorFields(resources[view],true).some(f=>f.key==='isPublished'));
    assert(!editorFields(resources[view],true).some(f=>f.key==='publish'));
  }
});
check('form values retain false and zero, normalize optional values and dates',()=>{
  const fields=[{key:'isActive',type:'checkbox'},{key:'displayOrder',type:'number',required:true},{key:'imageUrl'},{key:'startsAt',type:'datetime-local'},{key:'title',required:true}];
  assert.deepEqual(formPayload(fields,{isActive:false,displayOrder:0,imageUrl:'',startsAt:'2026-10-01T08:00:00Z',title:' Heritage '}),{isActive:false,displayOrder:0,imageUrl:null,startsAt:'2026-10-01T08:00:00.000Z',title:'Heritage'});
});
check('all requested scan types pass the backend validator',()=>{
  const validator=read(resolve(backend,'Validators/Governance/MonitoringValidators.cs'));
  for(const config of Object.values(flagViews)) assert(validator.includes(`"${config.scanType}"`),config.scanType);
});
check('all admin navigation paths are unique',()=>{
  const paths=adminGroups.flatMap(([s,,views])=>views.map(([v])=>`/admin/${s}/${v}`));
  assert.equal(paths.length,27);assert.equal(new Set(paths).size,paths.length);
});

// Inject a transport; no live requests, tokens, or database mutations.
const source=read(resolve(root,'src/services/superAdminService.js')).replace("import apiClient from './apiClient';",'const apiClient = {};');
const {createSuperAdminService}=await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
const calls=[];
const client=Object.fromEntries(['get','post','put'].map(method=>[method,(...args)=>{calls.push({method,args});return Promise.resolve({data:{ok:true}});} ]));
client.request=config=>{calls.push(config);return Promise.resolve({data:{ok:true}});};
const api=createSuperAdminService(client);
await api.list('/products/pending-approval',{page:2,pageSize:20});
check('approval queue pagination travels as query parameters',()=>assert.deepEqual(calls.pop(),{method:'get',args:['/products/pending-approval',{params:{page:2,pageSize:20}}]}));
await api.save('/cms/blogs','record-id',{isPublished:false,content:'Preserved content'});
check('editing sends PUT with full content',()=>assert.deepEqual(calls.pop(),{method:'put',args:['/cms/blogs/record-id',{isPublished:false,content:'Preserved content'}]}));
await api.action('patch','/products/product-id/approval',{status:'Approved'});
check('approval uses PATCH',()=>assert.equal(calls.pop().method,'patch'));
await api.action('delete','/admin/security/threats/blocked-ips',undefined,{ipAddress:'2001:db8::1'});
check('IP unblock uses DELETE query parameter, not a path segment',()=>assert.deepEqual(calls.pop(),{method:'delete',url:'/admin/security/threats/blocked-ips',data:undefined,params:{ipAddress:'2001:db8::1'}}));
await api.action('post','/admin/security/backups');
check('long-running backups have an extended timeout',()=>assert.equal(calls.pop().timeout,300000));
check('public registration cannot submit SuperAdmin',()=>assert(read(resolve(root,'src/pages/Auth/RegisterPage.jsx')).includes("if (!selectedRole || selectedRole === 'SuperAdmin' || mutation.isPending) return;")));
console.log(`\n${checks} admin contract checks passed.`);
