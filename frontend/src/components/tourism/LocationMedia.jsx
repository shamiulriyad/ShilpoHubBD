import { useState } from 'react';

// Placeholder look per TourismLocation type: a soft gradient and a large icon. Used only when a
// record has no photo (or its photo fails to load) -- never a stand-in photo of somewhere else.
const PLACEHOLDERS = {
  Hotel: { icon: '🏨', from: '#d8ecf0', to: '#b6d6e6' },
  Resort: { icon: '🌴', from: '#d6efe4', to: '#a9d8d0' },
  Hostel: { icon: '🛏️', from: '#e2e6f5', to: '#c3cdea' },
  Restaurant: { icon: '🍽️', from: '#fbe7cf', to: '#f3c9a3' },
  TouristPlace: { icon: '🏖️', from: '#d5ecf6', to: '#bfe3cf' },
  HeritageSite: { icon: '🏛️', from: '#efe4d2', to: '#dcc7a6' },
  Attraction: { icon: '📍', from: '#eadcf3', to: '#f0cfdc' },
};
const FALLBACK = PLACEHOLDERS.Attraction;

function Placeholder({ location }) {
  const look = PLACEHOLDERS[location.type] ?? FALLBACK;
  return (
    <div
      className="flex h-full w-full flex-col items-center justify-center gap-1 text-center"
      style={{ backgroundImage: `linear-gradient(135deg, ${look.from}, ${look.to})` }}
      role="img"
      aria-label={`${location.name} — photo not available yet`}
    >
      <span className="text-4xl drop-shadow-sm" aria-hidden="true">{look.icon}</span>
      <span className="px-3 text-[11px] font-medium uppercase tracking-wide text-black/45">{location.type.replace(/([a-z])([A-Z])/g, '$1 $2')}</span>
    </div>
  );
}

// The photo for a TourismLocation, with the licence credit the source requires, or the
// placeholder above. `className` sets the height, e.g. "h-40" or "h-72".
export default function LocationMedia({ location, className = 'h-40' }) {
  const [failed, setFailed] = useState(false);
  const showPhoto = Boolean(location.imageUrl) && !failed;

  return (
    <div className={`relative w-full overflow-hidden bg-background ${className}`}>
      {showPhoto ? (
        <>
          <img
            src={location.imageUrl}
            alt={location.name}
            loading="lazy"
            onError={() => setFailed(true)}
            className="h-full w-full object-cover"
          />
          {location.imageCredit && (
            <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/65 to-transparent px-2.5 pb-1.5 pt-6">
              {location.imageSourceUrl?.startsWith('http') ? (
                <a
                  href={location.imageSourceUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  title={location.imageCredit}
                  className="block truncate text-[10px] text-white/85 hover:text-white hover:underline"
                >
                  {location.imageCredit}
                </a>
              ) : (
                <span className="block truncate text-[10px] text-white/85" title={location.imageCredit}>{location.imageCredit}</span>
              )}
            </div>
          )}
        </>
      ) : (
        <Placeholder location={location} />
      )}
    </div>
  );
}
