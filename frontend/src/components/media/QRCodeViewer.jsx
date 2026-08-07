export default function QRCodeViewer({ value, label = 'Scan to verify', size = 96 }) {
  return (
    <div className="flex flex-col items-center gap-2">
      <div
        className="flex items-center justify-center rounded-lg border border-dashed border-border bg-background text-[10px] text-body/40"
        style={{ width: size, height: size }}
      >
        QR Code
      </div>
      {value && <p className="font-mono text-xs text-body/60">{value}</p>}
      {label && <p className="text-xs text-body/50">{label}</p>}
    </div>
  );
}
