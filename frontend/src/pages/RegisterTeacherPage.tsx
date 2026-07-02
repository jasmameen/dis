import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { GraduationCap, UserPlus } from 'lucide-react';
import { authApi, lookupApi } from '../api/services';
import type { Lookups } from '../types';

export default function RegisterTeacherPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [lookups, setLookups] = useState<Lookups | null>(null);
  const [form, setForm] = useState({
    fullName: '',
    email: '',
    username: '',
    password: '',
    confirmPassword: '',
    phone: '',
    departmentId: 1,
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    lookupApi.getAll().then(r => {
      const data = r.data.data as Lookups;
      setLookups(data);
      if (data.departments?.length) setForm(f => ({ ...f, departmentId: data.departments[0].id }));
    }).catch(() => {});
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (form.password.length < 6) {
      setError('كلمة المرور يجب أن تكون 6 أحرف على الأقل');
      return;
    }
    if (form.password !== form.confirmPassword) {
      setError('كلمتا المرور غير متطابقتين');
      return;
    }

    setLoading(true);
    try {
      const res = await authApi.registerTeacher({
        fullName: form.fullName,
        email: form.email,
        username: form.username,
        password: form.password,
        phone: form.phone || undefined,
        departmentId: form.departmentId,
      });
      if (res.data.success) {
        setSuccess(res.data.message || 'تم إنشاء الحساب بنجاح');
        setTimeout(() => navigate('/login'), 2000);
      } else {
        setError(res.data.message || 'فشل التسجيل');
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'حدث خطأ أثناء التسجيل');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-600 to-primary-800 p-4">
      <div className="w-full max-w-lg">
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-white/20 rounded-2xl mb-4">
            <UserPlus size={32} className="text-white" />
          </div>
          <h1 className="text-2xl font-bold text-white">{t('registerTeacher')}</h1>
          <p className="text-primary-100 mt-1">إنشاء حساب أستاذ جديد</p>
        </div>

        <div className="card">
          <form onSubmit={handleSubmit} className="space-y-4">
            <input className="input-field" placeholder={t('fullName')} value={form.fullName} onChange={e => setForm({ ...form, fullName: e.target.value })} required />
            <input type="email" className="input-field" placeholder={t('email')} value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} required />
            <input className="input-field" placeholder={t('username')} value={form.username} onChange={e => setForm({ ...form, username: e.target.value })} required />
            <input className="input-field" placeholder={t('phone')} value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
            <select className="input-field" value={form.departmentId} onChange={e => setForm({ ...form, departmentId: +e.target.value })}>
              {lookups?.departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
            <input type="password" className="input-field" placeholder={t('password')} value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} required />
            <input type="password" className="input-field" placeholder={t('confirmPassword')} value={form.confirmPassword} onChange={e => setForm({ ...form, confirmPassword: e.target.value })} required />

            {error && <p className="text-red-500 text-sm">{error}</p>}
            {success && <p className="text-green-600 text-sm">{success}</p>}

            <button type="submit" disabled={loading} className="btn-primary w-full">
              {loading ? t('loading') : t('registerTeacher')}
            </button>

            <Link to="/login" className="flex items-center justify-center gap-2 text-sm text-primary-600 hover:underline">
              <GraduationCap size={16} />
              {t('backToLogin')}
            </Link>
          </form>
        </div>
      </div>
    </div>
  );
}
