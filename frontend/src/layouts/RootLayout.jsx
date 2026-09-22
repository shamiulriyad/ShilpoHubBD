import { Outlet, useLocation } from 'react-router-dom';
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
  if (isAuthenticated && ((activeRole === 'LogisticsPartner' && explorePage) || (activeRole === 'Tourist' && (explorePage || tourismPage)))) {
    return <div className={theme} data-theme={theme}><DashboardLayout /></div>;
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
    </div>
  );
}
