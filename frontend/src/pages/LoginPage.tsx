import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { GraduationCap } from 'lucide-react';
import { authApi } from '../api/services';
import { useAuth } from '../context/AuthContext';

export default function LoginPage() {
  const { t } = useTranslation();
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ login: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showForgot, setShowForgot] = useState(false);
  const [forgotEmail, setForgotEmail] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const res = await authApi.login(form.login, form.password);
      if (res.data.success && res.data.data) {
        login(res.data.data);
        navigate('/');
      } else {
        setError(res.data.message || t('loginError'));
      }
    } catch {
      setError(t('loginError'));
    } finally {
      setLoading(false);
    }
  };

  const handleForgot = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await authApi.forgotPassword(forgotEmail);
      alert('تم إرسال رابط استعادة كلمة المرور');
      setShowForgot(false);
    } catch {
      alert('حدث خطأ');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-600 to-primary-800 p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-white/20 rounded-2xl mb-4">
            <GraduationCap size={32} className="text-white" />
          </div>
          <h1 className="text-2xl font-bold text-white">{t('appName')}</h1>
        </div>

        <div className="card">
          {!showForgot ? (
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">{t('email')}</label>
                <input
                  className="input-field"
                  value={form.login}
                  onChange={e => setForm({ ...form, login: e.target.value })}
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">{t('password')}</label>
                <input
                  type="password"
                  className="input-field"
                  value={form.password}
                  onChange={e => setForm({ ...form, password: e.target.value })}
                  required
                />
              </div>
              {error && <p className="text-red-500 text-sm">{error}</p>}
              <button type="submit" disabled={loading} className="btn-primary w-full">
                {loading ? t('loading') : t('login')}
              </button>
              <button type="button" onClick={() => setShowForgot(true)} className="text-sm text-primary-600 hover:underline w-full text-center">
                {t('forgotPassword')}
              </button>
              <Link to="/register" className="text-sm text-primary-600 hover:underline w-full text-center block">
                {t('registerTeacher')}
              </Link>
              <div className="text-xs text-gray-400 text-center pt-2 border-t">
                <p>Admin: admin@university.edu / Admin@123</p>
                <p>Teacher: teacher@university.edu / Teacher@123</p>
              </div>
            </form>
          ) : (
            <form onSubmit={handleForgot} className="space-y-4">
              <h3 className="font-semibold">{t('forgotPassword')}</h3>
              <input
                type="email"
                className="input-field"
                placeholder="email@university.edu"
                value={forgotEmail}
                onChange={e => setForgotEmail(e.target.value)}
                required
              />
              <div className="flex gap-2">
                <button type="submit" className="btn-primary flex-1">{t('save')}</button>
                <button type="button" onClick={() => setShowForgot(false)} className="btn-secondary flex-1">{t('cancel')}</button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
