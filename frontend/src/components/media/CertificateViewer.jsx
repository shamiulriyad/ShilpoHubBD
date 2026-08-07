export default function CertificateViewer({ title, issuedTo, issuedDate, certId }) {
  return (
    <div className="rounded-xl border-2 border-dashed border-primary/30 bg-surface p-6 text-center">
      <p className="text-xs font-semibold uppercase tracking-wide text-primary">Certificate of Authenticity</p>
      <h3 className="mt-2 text-lg font-semibold text-heading">{title}</h3>
      {issuedTo && <p className="mt-3 text-sm text-body/70">Issued to {issuedTo}</p>}
      {issuedDate && <p className="text-xs text-body/50">Issued {issuedDate}</p>}
      {certId && <p className="mt-2 font-mono text-xs text-primary">{certId}</p>}
    </div>
  );
}
