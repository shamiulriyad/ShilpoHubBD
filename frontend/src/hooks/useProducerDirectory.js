import { useQuery } from '@tanstack/react-query';
import { productsService } from '../services/productsService';

export function groupProducers(products) {
  const records=new Map();
  products.forEach(product=>{
    if(!product.producerId) return;
    if(!records.has(product.producerId)) records.set(product.producerId,{id:product.producerId,name:product.producerName||'Maker',crafts:new Map(),districts:new Map(),productCount:0,minPrice:Infinity});
    const record=records.get(product.producerId);
    record.crafts.set(product.categoryId,product.categoryName);
    record.districts.set(product.districtId,product.districtName);
    record.productCount++;
    record.minPrice=Math.min(record.minPrice,Number(product.discountPrice??product.price));
  });
  return [...records.values()].map(p=>({...p,crafts:[...p.crafts].map(([id,name])=>({id,name})),districts:[...p.districts].map(([id,name])=>({id,name}))}));
}
export function useProducerDirectory() {
  return useQuery({queryKey:['producer-directory'],staleTime:300000,queryFn:async({signal})=>{
    const products=[];
    let page=1,totalPages=1;
    do {
      if(signal.aborted) throw new DOMException('Aborted','AbortError');
      const result=await productsService.list({page,pageSize:50},signal);
      products.push(...(result.items||[]));totalPages=result.totalPages||1;page++;
    } while(page<=totalPages);
    return groupProducers(products);
  }});
}

