import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
<<<<<<< HEAD
import { PageHeader, AsyncState } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { useCategory } from '../../hooks/useCategories';
import { useCraftStory } from '../../hooks/useCraftStories';
import { useProducts } from '../../hooks/useProducts';
import { toProductCardItem } from '../../utils/productAdapters';
=======
import { PageHeader, QueryState } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { useCategory, useCraftStoryByCategory, useProducts } from '../../hooks/queries/useCatalog';
import { mapProduct } from '../../utils/mappers';
>>>>>>> origin/main

export default function CraftDetails() {
  const { craftId } = useParams();
  const categoryQuery = useCategory(craftId);
<<<<<<< HEAD
  const storyQuery = useCraftStory(craftId);
  const productsQuery = useProducts(craftId ? { categoryId: craftId, pageSize: 8 } : {});

  const craft = categoryQuery.data;
  const products = productsQuery.data?.items || [];
=======
  const storyQuery = useCraftStoryByCategory(craftId);
  const productsQuery = useProducts(craftId ? { categoryId: craftId, pageSize: 8 } : {});

  const category = categoryQuery.data;
  const products = productsQuery.data?.items ?? [];
>>>>>>> origin/main
  const producerNames = [...new Set(products.map((p) => p.producerName).filter(Boolean))];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
<<<<<<< HEAD
      <AsyncState
        isLoading={categoryQuery.isLoading}
        isError={categoryQuery.isError}
        error={categoryQuery.error}
        loadingText="Loading craft…"
      >
        {craft && (
=======
      <QueryState query={categoryQuery} loadingLabel="Loading craft…" emptyLabel="Craft not found.">
        {(craft) => (
>>>>>>> origin/main
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Explore', path: routePaths.explore },
                { label: 'Crafts', path: routePaths.exploreCrafts },
                { label: craft.name },
              ]}
              title={craft.name}
              description={craft.description || undefined}
            />

            <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
              <StatCard label="Products Listed" value={productsQuery.data?.totalCount ?? craft.productCount ?? 0} />
              <StatCard label="Active Producers" value={producerNames.length} />
<<<<<<< HEAD
              <StatCard label="Practiced Since" value={storyQuery.data?.since ? String(storyQuery.data.since) : '—'} />
=======
              <StatCard
                label="Practiced Since"
                value={storyQuery.data?.since ? String(storyQuery.data.since) : '—'}
              />
>>>>>>> origin/main
            </div>

            {storyQuery.data?.summary && (
              <p className="mb-10 max-w-3xl text-sm text-body/70">{storyQuery.data.summary}</p>
            )}

            {producerNames.length > 0 && (
              <>
                <p className="mb-3 text-sm font-semibold text-heading">Producers</p>
                <div className="mb-10 flex flex-wrap gap-2">
                  {producerNames.map((name) => (
                    <span
                      key={name}
                      className="rounded-full border border-border bg-surface px-3 py-1.5 text-xs text-body/70"
                    >
                      {name}
                    </span>
                  ))}
                </div>
              </>
            )}

            <p className="mb-3 text-sm font-semibold text-heading">Products</p>
<<<<<<< HEAD
            <AsyncState
              isLoading={productsQuery.isLoading}
              isError={productsQuery.isError}
              error={productsQuery.error}
              loadingText="Loading products…"
            >
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
                {products.map((p) => (
                  <ProductCard
                    key={p.id}
                    product={toProductCardItem(p)}
                    to={routePaths.marketplaceProductDetails.replace(':productId', p.id)}
                  />
                ))}
                {products.length === 0 && (
                  <p className="col-span-full text-sm text-body/60">No products listed for this craft yet.</p>
                )}
              </div>
            </AsyncState>
          </>
        )}
      </AsyncState>
=======
            <QueryState
              query={productsQuery}
              loadingLabel="Loading products…"
              emptyLabel="No products listed for this craft yet."
              isEmpty={(page) => !page?.items?.length}
            >
              {(page) => (
                <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
                  {page.items.map((p) => (
                    <ProductCard
                      key={p.id}
                      product={mapProduct(p)}
                      to={routePaths.marketplaceProductDetails.replace(':productId', p.id)}
                    />
                  ))}
                </div>
              )}
            </QueryState>
          </>
        )}
      </QueryState>
>>>>>>> origin/main
    </div>
  );
}
