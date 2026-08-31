import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
<<<<<<< HEAD
import { PageHeader, QueryState } from '../../components/ui';
import { VillageCard, ProductCard, StatCard } from '../../components/cards';
import { useDistricts, useVillages, useProducts } from '../../hooks/queries/useCatalog';
import { mapProduct, mapVillage } from '../../utils/mappers';
=======
import { PageHeader, AsyncState } from '../../components/ui';
import { VillageCard, StatCard } from '../../components/cards';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { toVillageCardItem } from '../../utils/villageAdapters';
>>>>>>> origin/main

export default function DistrictDetails() {
  const { districtId } = useParams();
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
<<<<<<< HEAD
  const productsQuery = useProducts(districtId ? { districtId, pageSize: 8 } : {});

  const district = (districtsQuery.data ?? []).find((d) => d.id === districtId);
  const villages = (villagesQuery.data ?? []).filter((v) => v.districtId === districtId);
  const products = productsQuery.data?.items ?? [];
  const producerNames = [...new Set(products.map((p) => p.producerName).filter(Boolean))];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <QueryState query={districtsQuery} loadingLabel="Loading district…" isEmpty={() => !district} emptyLabel="District not found.">
        {() => (
=======

  const district = (districtsQuery.data || []).find((d) => d.id === districtId);
  const districtVillages = (villagesQuery.data || []).filter((v) => v.districtId === districtId);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={districtsQuery.isLoading} isError={districtsQuery.isError} error={districtsQuery.error}>
        {district && (
>>>>>>> origin/main
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Explore', path: routePaths.explore },
                { label: 'Districts', path: routePaths.exploreDistricts },
                { label: district.name },
              ]}
              title={district.name}
<<<<<<< HEAD
              description={district.division ? `${district.division} Division` : 'District heritage overview.'}
            />

            <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
              <StatCard label="Heritage Villages" value={villages.length} />
              <StatCard label="Products Listed" value={productsQuery.data?.totalCount ?? 0} />
              <StatCard label="Producers" value={producerNames.length} />
            </div>

            <p className="mb-3 text-sm font-semibold text-heading">Villages in {district.name}</p>
            <QueryState query={villagesQuery} loadingLabel="Loading villages…" isEmpty={() => villages.length === 0} emptyLabel="No heritage villages recorded for this district yet.">
              {() => (
                <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
                  {villages.map((village) => (
                    <VillageCard
                      key={village.id}
                      village={mapVillage(village)}
                      to={routePaths.exploreVillageDetails.replace(':villageId', village.id)}
                    />
                  ))}
                </div>
              )}
            </QueryState>

            <p className="mb-3 text-sm font-semibold text-heading">Products from {district.name}</p>
            <QueryState query={productsQuery} loadingLabel="Loading products…" isEmpty={(page) => !page?.items?.length} emptyLabel="No products listed from this district yet.">
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
=======
              description={`${district.division} Division`}
            />

            <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
              <StatCard label="Heritage Villages" value={districtVillages.length} />
              <StatCard label="Division" value={district.division} />
            </div>

            <p className="mb-3 text-sm font-semibold text-heading">Villages in {district.name}</p>
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {districtVillages.map((village) => (
                <VillageCard
                  key={village.id}
                  village={toVillageCardItem(village)}
                  to={routePaths.exploreVillageDetails.replace(':villageId', village.id)}
                />
              ))}
              {districtVillages.length === 0 && (
                <p className="col-span-full text-sm text-body/60">No villages recorded for this district yet.</p>
              )}
            </div>
          </>
        )}
      </AsyncState>
>>>>>>> origin/main
    </div>
  );
}
