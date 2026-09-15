import React from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Routes, Route, Link } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import apiClient from '/src/services/apiClient.js';
import DashboardLayout from '/src/layouts/DashboardLayout.jsx';
import { roleSidebars } from '/src/data/navigation.js';
import Dashboard from '/src/pages/Producer/ProducerDashboard.jsx';
import Csr from '/src/pages/Producer/CsrSponsorship.jsx';
import Investments from '/src/pages/Producer/InvestmentOpportunities.jsx';
import Sustainability from '/src/pages/Producer/Sustainability.jsx';
import Inventory from '/src/pages/Producer/Inventory.jsx';
import Contracts from '/src/pages/Producer/Contracts.jsx';
import Quotations from '/src/pages/Producer/Quotations.jsx';
import Partnerships from '/src/pages/Producer/ManufacturingPartnerships.jsx';
import Designs from '/src/pages/Producer/DesignCollaborations.jsx';
import Development from '/src/pages/Producer/ProductDevelopment.jsx';
import Orders from '/src/pages/Producer/Orders.jsx';
import Live from '/src/pages/Producer/LiveShoppingManager.jsx';
import '/src/styles/index.css';
const paged=(items,page=1,totalPages=1)=>({items,page,pageSize:10,totalPages,totalCount:totalPages>1?11:items.length});
let products=[],txs=[],profile={ecoScore:0,badgeLevel:'None',totalCarbonSavingsKg:0,materialRecords:[],certifications:[]};
const common={id:'one',title:'Test project',referenceNumber:'TEST-001',contractValue:2500,quantity:4,progressPercentage:0,revisionCount:0,prototypeVersionCount:0,requiredDeliveryDate:'2026-10-10T00:00:00Z'};
const item={id:'item-one',productName:'Handwoven cloth',quantity:2,lineTotal:2500};
const cases={contracts:{status:'PendingApproval',terms:'Test contract terms',items:[item],statusHistory:[]},quotations:{status:'Sent',requirements:'Test quotation requirements',items:[item],recipients:[]},'manufacturing-partnerships':{status:'Accepted',productRequirements:'Test production requirements',manufacturingSpecifications:'Hand woven',milestones:[{id:'mile',title:'Weave first batch',status:'Pending',dueDate:'2026-10-10T00:00:00Z'}]},'design-collaborations':{status:'Active',designRequirements:'Indigo pattern brief',comments:[{id:'comment',authorName:'Partner',content:'Please use indigo.'}],revisions:[]},'product-development':{status:'Active',businessRequirements:'New collection brief',productSpecifications:'Cotton',comments:[],prototypeVersions:[]}};
const opportunity={id:'opp',title:'Test funding project',fundingGoal:1000,fundingRequirement:1000,fundingSecured:0,status:'Open',proposalCount:1};
apiClient.defaults.adapter=async config=>{
 const url=config.url,method=config.method,body=config.data?JSON.parse(config.data):{};let data;
 if(method!=='get'){
  document.getElementById('request-output').textContent=JSON.stringify({url,body});
  if(url==='/products'){const {imageUrls,...rest}=body;data={...rest,id:'new-product',primaryImageUrl:imageUrls[0]};products.push(data);}
  else if(url.includes('/adjust')){const product=products[0];const previousStock=product.stock;product.stock+=body.changeAmount;data={id:'tx',...body,previousStock,newStock:product.stock};txs.push(data);}
  else if(url==='/sustainability/materials'){data={id:'material',...body,totalCarbonSavingsKg:body.quantityUsed*body.carbonSavingsPerUnitKg};profile.materialRecords.push(data);profile.totalCarbonSavingsKg+=data.totalCarbonSavingsKg;}
  else if(url==='/sustainability/certifications'){data={id:'cert',...body,isVerified:false};profile.certifications.push(data);}
  else if(url.includes('/comments')) {throw {response:{status:400,data:{title:'Test validation error',detail:'Comment could not be saved.'}},config};}
  else data={id:'saved',...body};
 } else if(url==='/products/mine')data=products;
 else if(url==='/inventory/low-stock')data=products.filter(p=>p.stock<=p.lowStockThreshold);
 else if(url.includes('/inventory/products/'))data=txs;
 else if(url==='/categories')data=[{id:'category',name:'Weaving'}];
 else if(url==='/districts')data=[{id:'district',name:'Dhaka'}];
 else if(url==='/sustainability/me')data=profile;
 else if(url.endsWith('/analytics/revenue'))data={totalRevenue:7500,totalOrders:3,averageOrderValue:2500,pendingCount:1,acceptedCount:0,processingCount:0,shippedCount:1,deliveredCount:1,rejectedCount:0,cancelledCount:0};
 else if(url.endsWith('/analytics/sales'))data={dailySales:[{date:'2026-09-01T00:00:00Z',revenue:0},{date:'2026-09-02T00:00:00Z',revenue:2500},{date:'2026-09-03T00:00:00Z',revenue:5000}],topProducts:[]};
 else if(url.endsWith('/analytics/product-performance'))data=[{productId:'p',productName:'Handwoven cloth',salesCount:3,revenue:7500,averageRating:4.5}];
 else if(url==='/producer/orders')data=paged([{id:'order',productName:'Handwoven cloth',quantity:2,orderNumber:'ORDER-1',customerName:'Test Customer',lineTotal:2500,producerStatus:'Processing'}]);
 else if(url.includes('csr-sponsorship')||url.includes('investment-opportunities'))data=url.endsWith('/proposals')?paged([{id:'proposal',businessPartnerName:'Test partner',fundingAmount:1000,investmentAmount:1000,status:'Submitted'}]):paged([{...opportunity,title:config.params?.page===2?'Second page opportunity':opportunity.title}],config.params?.page||1,2);
 else if(url.includes('live'))data=paged([]);
 else {const key=url.split('/')[1];if(cases[key])data=url.endsWith('/received')?paged([{...common,status:cases[key].status}]):{...common,...cases[key]};else throw Error('Unmocked endpoint: '+url);}
 return {data:JSON.parse(JSON.stringify(data)),status:200,statusText:'OK',headers:{},config};
};
const pages=[['dashboard',Dashboard],['csr',Csr],['investments',Investments],['sustainability',Sustainability],['inventory',Inventory],['contracts',Contracts],['quotations',Quotations],['partnerships',Partnerships],['designs',Designs],['development',Development],['orders',Orders],['live',Live]];
createRoot(document.getElementById('root')).render(<QueryClientProvider client={new QueryClient({defaultOptions:{queries:{retry:false,refetchOnWindowFocus:false}}})}><MemoryRouter initialEntries={['/check/csr']}><nav aria-label="Test pages" className="flex flex-wrap gap-3 p-3">{pages.map(([name])=><Link key={name} to={'/check/'+name}>{name}</Link>)}</nav><output id="request-output" className="block break-all p-2 text-xs"/><Routes><Route element={<DashboardLayout navItems={roleSidebars.Producer.nav} sidebarTitle="Producer" />}>{pages.map(([name,Page])=><Route key={name} path={'/check/'+name} element={<Page/>}/>)}</Route></Routes></MemoryRouter></QueryClientProvider>);
