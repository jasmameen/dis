import { useTranslation } from 'react-i18next';
import { Shield, Users, GraduationCap, BookOpen, Building } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { Navigate } from 'react-router-dom';

const adminSections = [
  { icon: Users, title: 'إدارة الطلاب', desc: 'إضافة وتعديل وحذف الطلاب واستيراد Excel', link: '/students' },
  { icon: GraduationCap, title: 'إدارة الأساتذة', desc: 'إدارة حسابات الأساتذة وربط المواد', link: '/teachers' },
  { icon: BookOpen, title: 'إدارة المواد', desc: 'إضافة المواد الدراسية وربطها بالأقسام', link: '/subjects' },
  { icon: Building, title: 'إدارة الأقسام', desc: 'إدارة الأقسام والمراحل والشعب', link: '#' },
  { icon: Shield, title: 'سجل التدقيق', desc: 'متابعة جميع العمليات في النظام', link: '#' },
];

export default function AdminPage() {
  const { t } = useTranslation();
  const { isAdmin } = useAuth();

  if (!isAdmin) return <Navigate to="/" replace />;

  return (
    <div className="space-y-6">
      <div className="card bg-gradient-to-l from-primary-600 to-primary-700 text-white">
        <div className="flex items-center gap-4">
          <Shield size={40} />
          <div>
            <h2 className="text-2xl font-bold">{t('adminPanel')}</h2>
            <p className="text-primary-100">صلاحيات كاملة لإدارة النظام</p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {adminSections.map(({ icon: Icon, title, desc, link }) => (
          <a key={title} href={link} className="card hover:shadow-md transition-shadow cursor-pointer">
            <Icon className="text-primary-600 mb-3" size={28} />
            <h3 className="font-semibold text-lg">{title}</h3>
            <p className="text-sm text-gray-500 mt-1">{desc}</p>
          </a>
        ))}
      </div>

      <div className="card">
        <h3 className="font-semibold mb-4">ميزات إضافية</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">✅ QR Code للحضور</div>
          <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">✅ سجل تدقيق (Audit Log)</div>
          <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">✅ دعم العربية والإنجليزية</div>
          <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">✅ رسوم بيانية تفاعلية</div>
          <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">🔜 بصمة الوجه (اختياري)</div>
          <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">🔜 تطبيق موبايل للأساتذة</div>
          <div className="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg">🔜 نسخ احتياطي تلقائي</div>
        </div>
      </div>
    </div>
  );
}
