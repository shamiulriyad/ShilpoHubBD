import { Outlet, Link } from 'react-router-dom';
import { routePaths } from '../routes/routePaths';
import { useTheme } from '../contexts/ThemeContext';

export default function RootLayout() {
  const { theme, toggleTheme } = useTheme();

  return (
    <div className={theme}>
      <header className="border-b border-slate-200 bg-white/80 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4">
          <Link to={routePaths.home} className="font-semibold text-slate-900">ShilpoHub</Link>
          <button type="button" onClick={toggleTheme} className="rounded-md border px-3 py-2 text-sm">
            Toggle Theme
          </button>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
import { Outlet, Link } from 'react-router-dom';
import { routePaths } from '../routes/routePaths';
import { useTheme } from '../contexts/ThemeContext';

export default function RootLayout() {
  const { theme, toggleTheme } = useTheme();

  return (
    <div className={theme}>
      <header className="border-b border-slate-200 bg-white/80 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-4">
          <Link to={routePaths.home} className="font-semibold text-slate-900">ShilpoHub</Link>
          <button type="button" onClick={toggleTheme} className="rounded-md border px-3 py-2 text-sm">
            Toggle Theme
          </button>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
