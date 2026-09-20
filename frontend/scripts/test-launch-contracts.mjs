import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const frontend = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const root = resolve(frontend, '..');
const read = path => readFileSync(resolve(root, path), 'utf8');
let passed = 0;
const check = (label, test) => { test(); passed += 1; console.log(`PASS ${label}`); };

check('producer orders are restricted by product ownership', () => {
  const repository = read('backend/src/ShilpoHubBD.Data/Repositories/ProducerOrderRepository.cs');
  assert(repository.includes('Where(i => i.Product.ProducerId == producerId)'));
});
check('checkout copies every cart product into an order item', () => {
  const service = read('backend/src/ShilpoHubBD.Application/Services/Commerce/OrderService.cs');
  assert(service.includes('order.Items.Add(new OrderItem'));
  assert(service.includes('ProductId = cartItem.ProductId'));
});
check('producer fulfillment uses the dedicated producer API', () => {
  const service = read('frontend/src/services/producerOrdersService.js');
  for (const path of ['/producer/orders', '/accept', '/processing', '/ship', '/deliver']) assert(service.includes(path));
});
check('public catalog excludes unapproved products', () => {
  const repository = read('backend/src/ShilpoHubBD.Data/Repositories/ProductRepository.cs');
  assert(repository.includes('p.ApprovalStatus == ProductApprovalStatus.Approved'));
});
check('producer listings expose image upload and approval status', () => {
  assert(read('frontend/src/pages/Producer/NewProductForm.jsx').includes("type=\"file\""));
  assert(read('frontend/src/pages/Producer/Products.jsx').includes('product.approvalStatus'));
});
check('media upload validates authorization, type and size', () => {
  const controller = read('backend/src/ShilpoHubBD.Api/Controllers/MediaController.cs');
  assert(controller.includes('RoleNames.Producer'));
  assert(controller.includes('RoleNames.SuperAdmin'));
  assert(controller.includes('5 * 1024 * 1024'));
  assert(controller.includes('image/webp'));
});
check('tourist places have list, detail and admin authoring routes', () => {
  assert(read('frontend/src/routes/router.jsx').includes('tourismPlaceDetails'));
  assert(read('frontend/src/pages/Admin/adminConfig.js').includes("path: '/heritage-places'"));
});

console.log(`\n${passed} launch-critical contract checks passed.`);
