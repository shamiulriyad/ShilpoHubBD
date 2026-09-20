// Local synthetic fixture: no backend requests or real orders.
import React from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import apiClient from '../../src/services/apiClient';
import Marketplace from '../../src/pages/Customer/Marketplace';
import ShoppingCart from '../../src/pages/Customer/ShoppingCart';
import { useAuthStore } from '../../src/stores/useAuthStore';
import '../../src/styles/index.css';
useAuthStore.setState({accessToken:'fixture',user:{fullName:'Test customer'},roles:['Customer'],activeRole:'Customer'});
const product = {id:'test',name:'Clay pot',description:'Handmade pottery',price:700,categoryName:'Pottery',producerName:'Test artisan',districtName:'Dhaka',primaryImageUrl:'/images/pottery-photo.jpg'};
let cart = [{id:'item',productId:'test',productName:'Clay pot',primaryImageUrl:product.primaryImageUrl,unitPrice:700,quantity:1}];
apiClient.defaults.adapter = async config => {
 const body = config.data ? JSON.parse(config.data) : {};
 let data;
 if(config.url === '/products') { const match = !config.params.search || product.name.toLowerCase().includes(config.params.search.toLowerCase()); data={items:match?[product]:[],totalCount:match?1:0,totalPages:1}; }
 else if(config.url === '/categories') data=[{id:'category',name:'Pottery'}];
 else if(config.url === '/districts') data=[{id:'district',name:'Dhaka'}];
 else if(config.url.startsWith('/cart')) {
  if(config.method === 'delete') cart=[];
  if(config.method === 'post') cart=[{id:'item',productId:'test',productName:'Clay pot',unitPrice:700,quantity:body.quantity}];
  if(config.method === 'put') cart=cart.map(item => ({...item,quantity:body.quantity}));
  data=cart;
 } else throw new Error('Unexpected fixture request: '+config.url);
 return {data,status:200,statusText:'OK',headers:{},config};
};
const client = new QueryClient({defaultOptions:{queries:{retry:false},mutations:{retry:false}}});
createRoot(document.getElementById('root')).render(<QueryClientProvider client={client}><div className="mx-auto max-w-6xl p-6"><p className="mb-4">Synthetic customer test fixture</p><MemoryRouter initialEntries={['/customer/marketplace']}><Routes><Route path="/customer/marketplace" element={<Marketplace/>}/><Route path="/customer/cart" element={<ShoppingCart/>}/></Routes></MemoryRouter></div></QueryClientProvider>);
