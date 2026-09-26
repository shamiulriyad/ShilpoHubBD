import { Outlet, ScrollRestoration, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import DashboardLayout from './DashboardLayout';
import { useTheme } from '../contexts/ThemeContext';
import Navbar from '../components/layout/Navbar';
import Footer from '../components/layout/Footer';

export default function RootLayout() {
  const { theme } = useTheme();
  const { isAuthenticated, activeRole } = useAuth();
  const { pathname } = useLocation();

  const explorePage = pathname === '/explore' || pathname.startsWith('/explore/');
  const tourismPage = pathname === '/tourism' || pathname.startsWith('/tourism/');
  const marketplacePage = pathname === '/marketplace' || pathname.startsWith('/marketplace/');
  const innovationPage = pathname.startsWith('/research/');
  if (isAuthenticated && activeRole === 'HeritageInnovationHub' && (innovationPage || explorePage)) {
    return <div className={theme} data-theme={theme}><DashboardLayout /><ScrollRestoration /></div>;
  }
  if (isAuthenticated && ((activeRole === 'LogisticsPartner' && explorePage) || (activeRole === 'Tourist' && (explorePage || tourismPage || marketplacePage)))) {
    return <div className={theme} data-theme={theme}><DashboardLayout /><ScrollRestoration /></div>;
  }

  return (
    <div className={theme} data-theme={theme}>
      <div className="flex min-h-screen flex-col bg-background text-body">
        <Navbar />
        <main className="flex-1">
          <Outlet />
        </main>
        <Footer />
      </div>
      {/* New page -> starts at the top; browser Back -> returns to where you were. */}
      <ScrollRestoration />
    </div>
  );
}
