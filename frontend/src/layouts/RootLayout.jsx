import { Outlet } from 'react-router-dom';
import { useTheme } from '../contexts/ThemeContext';
import Navbar from '../components/layout/Navbar';
import Footer from '../components/layout/Footer';

export default function RootLayout() {
  const { theme } = useTheme();

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
