import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { BookOpen, Users, UserCheck, UserX, TrendingUp } from 'lucide-react';
import { Bar } from 'react-chartjs-2';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js';
import { dashboardApi } from '../api/services';
import type { DashboardStats } from '../types';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

export default function DashboardPage() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    dashboardApi.getStats()
      .then(r => setStats(r.data.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="text-center py-12">{t('loading')}</div>;

  const cards = [
    { icon: BookOpen, label: t('assignedSubjects'), value: stats?.assignedSubjects ?? 0, color: 'text-blue-600' },
    { icon: Users, label: t('totalStudents'), value: stats?.totalStudents ?? 0, color: 'text-purple-600' },
    { icon: UserCheck, label: t('presentToday'), value: stats?.presentToday ?? 0, color: 'text-green-600' },
    { icon: UserX, label: t('absentToday'), value: stats?.absentToday ?? 0, color: 'text-red-600' },
  ];

  const chartData = {
    labels: stats?.weeklyStats.map(s => s.label) ?? [],
    datasets: [
      { label: t('present'), data: stats?.weeklyStats.map(s => s.present) ?? [], backgroundColor: '#22c55e' },
      { label: t('absent'), data: stats?.weeklyStats.map(s => s.absent) ?? [], backgroundColor: '#ef4444' },
    ],
  };

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {cards.map(({ icon: Icon, label, value, color }) => (
          <div key={label} className="stat-card">
            <div className="flex items-center justify-between">
              <Icon className={color} size={28} />
              <span className="text-3xl font-bold">{value}</span>
            </div>
            <p className="text-sm text-gray-500">{label}</p>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="card lg:col-span-2">
          <h3 className="font-semibold mb-4">{t('attendanceRate')} - 7 {t('days') || 'أيام'}</h3>
          <Bar data={chartData} options={{ responsive: true, plugins: { legend: { position: 'top' } } }} />
        </div>
        <div className="card">
          <div className="flex items-center gap-2 mb-4">
            <TrendingUp className="text-primary-600" />
            <h3 className="font-semibold">{t('attendanceRate')}</h3>
          </div>
          <div className="text-center py-8">
            <div className="text-5xl font-bold text-primary-600">{stats?.attendanceRateToday ?? 0}%</div>
            <p className="text-gray-500 mt-2">{t('presentToday')}</p>
          </div>
          <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-3 mt-4">
            <div
              className="bg-primary-600 h-3 rounded-full transition-all"
              style={{ width: `${stats?.attendanceRateToday ?? 0}%` }}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
