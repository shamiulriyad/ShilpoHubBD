import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { QRCodeViewer, CertificateViewer } from '../../components/media';
import { heritageCollectionItems } from '../../data/mockData';

export default function HeritageCollection() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Heritage Collection' }]}
        title="Your Heritage Collection"
        description="Every heritage-certified piece you own, with its verified Digital Heritage ID."
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {heritageCollectionItems.map((item) => (
          <div key={item.id} className="space-y-2 rounded-xl border border-border bg-surface p-5">
            <div className="flex aspect-[4/3] items-center justify-center rounded-lg bg-background text-xs text-body/40">
              Product Image
            </div>
            <Badge tone="secondary">{item.category}</Badge>
            <p className="text-sm font-semibold text-heading">{item.product}</p>
            <p className="text-xs text-body/60">By {item.producer}</p>
            <p className="text-xs text-body/50">Acquired {item.acquiredDate}</p>
            <Link
              to={routePaths.customerCraftStory.replace(':craftId', item.craftId)}
              className="inline-block text-xs text-link hover:underline"
            >
              Read the craft story →
            </Link>
            <details className="group pt-1">
              <summary className="cursor-pointer list-none text-xs font-medium text-link hover:underline">
                Verify authenticity
              </summary>
              <div className="mt-3 space-y-3">
                <CertificateViewer
                  title={item.product}
                  issuedTo="Ayesha Rahman"
                  issuedDate={item.acquiredDate}
                  certId={item.heritageId}
                />
                <QRCodeViewer value={item.heritageId} />
              </div>
            </details>
          </div>
        ))}
        {heritageCollectionItems.length === 0 && (
          <p className="col-span-full p-6 text-center text-sm text-body/60">
            Your heritage collection is empty. Shop the marketplace to start collecting.
          </p>
        )}
      </div>
    </div>
  );
}
