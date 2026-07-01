import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, ThemeProvider, useAuth } from './context/AuthContext';
import AdminRoute from './components/AdminRoute';
import Layout from './components/Layout';
import LoginPage from './pages/LoginPage';
import RegisterTeacherPage from './pages/RegisterTeacherPage';
import DashboardPage from './pages/DashboardPage';
import AttendancePage from './pages/AttendancePage';
import StudentsPage from './pages/StudentsPage';
import SubjectsPage from './pages/SubjectsPage';
import TeachersPage from './pages/TeachersPage';
import ReportsPage from './pages/ReportsPage';
import SearchPage from './pages/SearchPage';
import AdminPage from './pages/AdminPage';
import './i18n';

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  return user ? <>{children}</> : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterTeacherPage />} />
            <Route path="/" element={<PrivateRoute><Layout /></PrivateRoute>}>
              <Route index element={<DashboardPage />} />
              <Route path="attendance" element={<AttendancePage />} />
              <Route path="students" element={<StudentsPage />} />
              <Route path="subjects" element={<AdminRoute><SubjectsPage /></AdminRoute>} />
              <Route path="teachers" element={<AdminRoute><TeachersPage /></AdminRoute>} />
              <Route path="reports" element={<ReportsPage />} />
              <Route path="search" element={<SearchPage />} />
              <Route path="admin" element={<AdminRoute><AdminPage /></AdminRoute>} />
            </Route>
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}
