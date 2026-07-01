import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  LayoutDashboard, ClipboardCheck, Users, BookOpen, GraduationCap,
  BarChart3, Search, Bell, Settings, LogOut, Moon, Sun, Globe
} from 'lucide-react';
import { useAuth, useTheme } from '../context/AuthContext';
import { useState, useEffect } from 'react';
import { notificationsApi } from '../api/services';
import type { Notification } from '../types';

const navItems = [
  { to: '/', icon: LayoutDashboard, label: 'dashboard', admin: false },
  { to: '/attendance', icon: ClipboardCheck, label: 'attendance', admin: false },
  { to: '/students', icon: Users, label: 'students', admin: false },
  { to: '/subjects', icon: BookOpen, label: 'subjects', admin: true },
  { to: '/teachers', icon: GraduationCap, label: 'teachers', admin: true },
  { to: '/reports', icon: BarChart3, label: 'reports', admin: false },
  { to: '/search', icon: Search, label: 'search', admin: false },
  { to: '/admin', icon: Settings, label: 'admin', admin: true },
];

export default function Layout() {
  const { t, i18n } = useTranslation();
  const { user, logout, isAdmin } = useAuth();
  const { dark, toggle } = useTheme();
  const navigate = useNavigate();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [showNotifs, setShowNotifs] = useState(false);

  useEffect(() => {
    notificationsApi.getAll().then(r => setNotifications(r.data.data || [])).catch(() => {});
  }, []);

  const toggleLang = () => {
    const newLang = i18n.language === 'ar' ? 'en' : 'ar';
    i18n.changeLanguage(newLang);
    document.documentElement.lang = newLang;
    document.documentElement.dir = newLang === 'ar' ? 'rtl' : 'ltr';
  };

  const unread = notifications.filter(n => !n.isRead).length;

  return (
    <div className="min-h-screen flex">
      <aside className="w-64 bg-white dark:bg-gray-800 border-l border-gray-200 dark:border-gray-700 flex flex-col no-print">
        <div className="p-6 border-b border-gray-200 dark:border-gray-700">
          <h1 className="text-lg font-bold text-primary-600">{t('appName')}</h1>
          <p className="text-sm text-gray-500 mt-1">{user?.fullName || user?.username}</p>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {navItems.filter(item => !item.admin || isAdmin).map(({ to, icon: Icon, label }) => (
            <NavLink
              key={to}
              to={to}
              end={to === '/'}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors ${
                  isActive
                    ? 'bg-primary-50 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300'
                    : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700'
                }`
              }
            >
              <Icon size={20} />
              <span>{t(label)}</span>
            </NavLink>
          ))}
        </nav>
        <div className="p-4 border-t border-gray-200 dark:border-gray-700">
          <button onClick={() => { logout(); navigate('/login'); }} className="flex items-center gap-3 w-full px-4 py-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg">
            <LogOut size={20} />
            <span>{t('logout')}</span>
          </button>
        </div>
      </aside>

      <div className="flex-1 flex flex-col">
        <header className="h-16 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between px-6 no-print">
          <h2 className="text-xl font-semibold">{t('welcome')}, {user?.fullName || user?.username}</h2>
          <div className="flex items-center gap-3">
            <button onClick={toggleLang} className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700" title={t('language')}>
              <Globe size={20} />
            </button>
            <button onClick={toggle} className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700">
              {dark ? <Sun size={20} /> : <Moon size={20} />}
            </button>
            <div className="relative">
              <button onClick={() => setShowNotifs(!showNotifs)} className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 relative">
                <Bell size={20} />
                {unread > 0 && (
                  <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">
                    {unread}
                  </span>
                )}
              </button>
              {showNotifs && (
                <div className="absolute left-0 mt-2 w-80 bg-white dark:bg-gray-800 rounded-xl shadow-lg border border-gray-200 dark:border-gray-700 z-50 max-h-96 overflow-y-auto">
                  <div className="p-3 border-b font-semibold">{t('notifications')}</div>
                  {notifications.length === 0 ? (
                    <p className="p-4 text-gray-500 text-sm">{t('noData')}</p>
                  ) : (
                    notifications.map(n => (
                      <div key={n.id} className={`p-3 border-b border-gray-100 dark:border-gray-700 ${!n.isRead ? 'bg-primary-50/50 dark:bg-primary-900/10' : ''}`}>
                        <p className="font-medium text-sm">{n.title}</p>
                        <p className="text-xs text-gray-500 mt-1">{n.message}</p>
                      </div>
                    ))
                  )}
                </div>
              )}
            </div>
          </div>
        </header>
        <main className="flex-1 p-6 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
