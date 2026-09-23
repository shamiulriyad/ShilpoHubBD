import React from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '../../src/contexts/ThemeContext';
import RootLayout from '../../src/layouts/RootLayout';
import HeritageMap from '../../src/pages/Tourism/HeritageMap';
import VillageExplorer from '../../src/pages/Tourism/VillageExplorer';
import LocalCuisines from '../../src/pages/Tourism/LocalCuisines';
import TouristServices from '../../src/pages/Tourism/TouristServices';
import Districts from '../../src/pages/Explore/Districts';
import DistrictDetails from '../../src/pages/Explore/DistrictDetails';
import Producers from '../../src/pages/Explore/Producers';
import ProductListing from '../../src/pages/Marketplace/ProductListing';
import { useAuthStore } from '../../src/stores/useAuthStore';
import apiClient from '../../src/services/apiClient';
import references from '../../src/data/tourismReferences.json';
import '../../src/styles/index.css';

// Isolated development fixture: no API request reaches a real account.
const params=new URLSearchParams(location.search);
useAuthStore.setState({accessToken:'fixture',user:{id:'preview',fullName:'Tourist preview'},roles:['Tourist'],activeRole:'Tourist',sessionReady:true});
const products=[
  {id:'one',name:'Handwoven sari',producerId:'maker-one',producerName:'Amina Weaving',categoryId:'weaving',categoryName:'Dhakai Jamdani',districtId:'Narayanganj',districtName:'Narayanganj',price:5000},
  {id:'two',name:'Silk scarf',producerId:'maker-two',producerName:'Silk Studio',categoryId:'silk',categoryName:'Rajshahi Silk',districtId:'Rajshahi',districtName:'Rajshahi',price:1200},
  {id:'three',name:'Cotton shawl',producerId:'maker-one',producerName:'Amina Weaving',categoryId:'cotton',categoryName:'Handloom',districtId:'Narayanganj',districtName:'Narayanganj',price:1800},
];
apiClient.defaults.adapter=async config=>{
  if(params.has('error')&&!config.url.startsWith('/notifications'))throw new Error('Fixture connection unavailable');
  let data={items:[],totalCount:0,totalPages:1};
  if(config.url==='/districts')data=references.districts.map(d=>({id:d.name,name:d.name,division:['Barguna','Patuakhali','Pirojpur','Bhola','Barishal','Jhalokati'].includes(d.name)?'Barishal':'Other divisions'}));
  else if(config.url==='/categories')data=[{id:'weaving',name:'Dhakai Jamdani'},{id:'silk',name:'Rajshahi Silk'},{id:'cotton',name:'Handloom'}];
  else if(config.url==='/products'){
    const q=config.params||{};let items=products.filter(p=>(!q.producerId||p.producerId===q.producerId)&&(!q.categoryId||p.categoryId===q.categoryId)&&(!q.districtId||p.districtId===q.districtId)&&(!q.minPrice||p.price>=Number(q.minPrice))&&(!q.maxPrice||p.price<=Number(q.maxPrice))&&(!q.search||p.name.toLowerCase().includes(q.search.toLowerCase())));
    if(q.sortBy==='PriceLowToHigh')items.sort((a,b)=>a.price-b.price);
    data={items,totalCount:items.length,totalPages:1};
  }else if(config.url.startsWith('/notifications'))data={items:[],unreadCount:0,totalCount:0};
  return {data,status:200,statusText:'OK',headers:{},config};
};
createRoot(document.getElementById('root')).render(<ThemeProvider><QueryClientProvider client={new QueryClient({defaultOptions:{queries:{retry:false}}})}><MemoryRouter initialEntries={[params.get('path')||'/tourism/map']}><Routes><Route element={<RootLayout/>}><Route path="/tourism/map" element={<HeritageMap/>}/><Route path="/tourism/villages" element={<VillageExplorer/>}/><Route path="/tourism/cuisines" element={<LocalCuisines/>}/><Route path="/tourism/services" element={<TouristServices/>}/><Route path="/explore/districts" element={<Districts/>}/><Route path="/explore/districts/:districtId" element={<DistrictDetails/>}/><Route path="/explore/producers" element={<Producers/>}/><Route path="/marketplace/products" element={<ProductListing/>}/><Route path="*" element={<p>Fixture destination</p>}/></Route></Routes></MemoryRouter></QueryClientProvider></ThemeProvider>);
