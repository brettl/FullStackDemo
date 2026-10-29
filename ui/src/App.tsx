import { Link, NavLink, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { getToken, setToken } from './api';
import ContentPage from './pages/ContentPage';
import BookingPage from './pages/BookingPage';
import LoginPage from './pages/LoginPage';
import AdminPage from './pages/AdminPage';

export default function App() {
  // Re-render the nav when the user logs in or out.
  const [loggedIn, setLoggedIn] = useState(!!getToken());
  const navigate = useNavigate();
  const location = useLocation();

  // Keep the nav in step with the saved token after every page change. Doing it here (not inside logout)
  // means "log out" first moves to the Home page and only then hides the admin links.
  useEffect(() => { setLoggedIn(!!getToken()); }, [location.pathname]);

  const logout = () => {
    setToken(null);
    navigate('/');
  };

  return (
    <>
      <header className="site-header">
        <Link to="/" className="brand">Sample Hair Studio</Link>
        <nav>
          <NavLink to="/" end>Home</NavLink>
          <NavLink to="/contact">Contact</NavLink>
          <NavLink to="/booking">Booking</NavLink>
          {loggedIn ? (
            <>
              <NavLink to="/admin">Admin</NavLink>
              <button className="link-button" data-testid="logout" onClick={logout}>Log out</button>
            </>
          ) : (
            <NavLink to="/login">Staff login</NavLink>
          )}
        </nav>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<ContentPage pageKey="home" />} />
          <Route path="/contact" element={<ContentPage pageKey="contact" />} />
          <Route path="/booking" element={<BookingPage />} />
          <Route path="/login" element={<LoginPage onLogin={() => setLoggedIn(true)} />} />
          {/* The admin page is only for logged-in users; everyone else is sent to the login page. */}
          <Route path="/admin" element={loggedIn ? <AdminPage /> : <Navigate to="/login" replace />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>

      <footer className="site-footer">Demo site. All names and details are made up.</footer>
    </>
  );
}
