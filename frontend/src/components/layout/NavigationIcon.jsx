const paths = {
  Dashboard: 'M3 10 12 3l9 7v11h-6v-7H9v7H3Z',
  'My Bookings': 'M4 5h16v15H4ZM8 3v4m8-4v4M4 10h16m-12 4h3m2 0h3',
  'Travel Passport': 'M5 3h14v18H5ZM8 17h8M12 6a4 4 0 1 0 0 8 4 4 0 0 0 0-8Zm0 0v8m-4-4h8',
  'Heritage Map': 'm3 5 6-2 6 2 6-2v16l-6 2-6-2-6 2Zm6-2v16m6-14v16',
  Festivals: 'M5 3v18M5 4h14l-3 4 3 4H5',
  'Cultural Events': 'M4 5h16v15H4ZM8 3v4m8-4v4M4 10h16m-8 3v4m-2-2h4',
  'Tour Routes': 'M5 18c0-8 14 0 14-9M5 15a3 3 0 1 0 0 6 3 3 0 0 0 0-6ZM19 3a3 3 0 1 0 0 6 3 3 0 0 0 0-6Z',
  'Village Explorer': 'm2 11 6-5 6 5m-10 0v9h8v-9m3-4 3-3 4 4m-6 0v12h4V8',
  'Local Cuisine': 'M5 3v7m4-7v7M3 3v6a4 4 0 0 0 8 0V3M7 13v8m11-18c-4 4-4 9 0 10V3Zm0 10v8',
  'Tourist Services': 'M4 16a8 8 0 0 1 16 0M2 19h20M12 8V5m-2 0h4',
  'Explore Heritage': 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18Zm4 5-3 5-5 3 3-5Z',
  Messages: 'M3 5h18v14H3Zm0 0 9 7 9-7',
  Settings: 'M4 7h16M4 17h16M8 4v6m8 4v6',
};
export default function NavigationIcon({ label }) {
  return <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d={paths[label] || 'm12 3 2 6 7 3-7 3-2 6-2-6-7-3 7-3Z'} /></svg>;
}
