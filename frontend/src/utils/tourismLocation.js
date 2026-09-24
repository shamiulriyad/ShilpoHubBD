// How a TourismLocation's provenance is shown. Records imported from Seed/TourismData carry a
// verificationStatus and a coordinatesPrecision; hand-entered/sample rows fall through to "Unverified".

export function verificationBadge(location) {
  switch (location.verificationStatus) {
    case 'Verified':
      return { tone: 'success', label: 'Verified · official source' };
    case 'SecondarySource':
      return { tone: 'secondary', label: 'Secondary source · confirm before booking' };
    default:
      return { tone: 'secondary', label: location.isVerified ? 'Verified' : 'Unverified · sample data' };
  }
}

const COORDINATE_NOTES = {
  geocoded_approximate: 'Map position is approximate (geocoded from OpenStreetMap)',
  community_listing: 'Map position from a community listing (unverified)',
  official_listing: 'Map position from an official listing',
};

export function coordinatesNote(location) {
  return COORDINATE_NOTES[location.coordinatesPrecision] ?? null;
}
