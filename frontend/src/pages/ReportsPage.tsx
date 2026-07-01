import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Download, Printer } from 'lucide-react';
import { reportsApi, studentsApi } from '../api/services';
import type { Student } from '../types';

export default function ReportsPage() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<'daily' | 'monthly' | 'yearly'>('daily');
  const [report, setReport] = useState<Record<string, unknown> | null>(null);
  const [loading, setLoading] = useState(false);
  const [filters, setFilters] = useState({
    date: new Date().toISOString().split('T')[0],
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    studentId: 0,
  });
  const [students, setStudents] = useState<Student[]>([]);

  const loadStudents = async () => {
    const res = await studentsApi.getAll();
    setStudents(res.data.data || []);
    if (res.data.data?.length) setFilters(f => ({ ...f, studentId: res.data.data![0].id }));
  };

  const generate = async () => {
    setLoading(true);
    try {
      if (tab === 'daily') {
        const res = await reportsApi.daily({ fromDate: filters.date });
        setReport(res.data.data as Record<string, unknown>);
      } else if (tab === 'monthly') {
        const res = await reportsApi.monthly(filters.year, filters.month);
        setReport(res.data.data as Record<string, unknown>);
      } else {
        if (!filters.studentId) { await loadStudents(); return; }
        const res = await reportsApi.yearly(filters.year, filters.studentId);
        setReport(res.data.data as Record<string, unknown>);
      }
    } catch {
      setReport(null);
    } finally {
      setLoading(false);
    }
  };

  const exportExcel = async () => {
    const res = await reportsApi.exportDaily({ fromDate: filters.date });
    const url = URL.createObjectURL(res.data);
    const a = document.createElement('a');
    a.href = url; a.download = 'report.xlsx'; a.click();
  };

  const printReport = () => window.print();

  const tabs = [
    { key: 'daily' as const, label: t('dailyReport') },
    { key: 'monthly' as const, label: t('monthlyReport') },
    { key: 'yearly' as const, label: t('yearlyReport') },
  ];

  return (
    <div className="space-y-6">
      <div className="flex gap-2 border-b border-gray-200 dark:border-gray-700">
        {tabs.map(({ key, label }) => (
          <button key={key} onClick={() => { setTab(key); setReport(null); }}
            className={`px-4 py-2 -mb-px border-b-2 transition-colors ${tab === key ? 'border-primary-600 text-primary-600' : 'border-transparent text-gray-500'}`}>
            {label}
          </button>
        ))}
      </div>

      <div className="card">
        <div className="flex flex-wrap gap-4 items-end">
          {tab === 'daily' && (
            <div>
              <label className="block text-sm mb-1">{t('date')}</label>
              <input type="date" className="input-field" value={filters.date} onChange={e => setFilters({ ...filters, date: e.target.value })} />
            </div>
          )}
          {(tab === 'monthly' || tab === 'yearly') && (
            <>
              <div>
                <label className="block text-sm mb-1">السنة</label>
                <input type="number" className="input-field w-32" value={filters.year} onChange={e => setFilters({ ...filters, year: +e.target.value })} />
              </div>
              {tab === 'monthly' && (
                <div>
                  <label className="block text-sm mb-1">الشهر</label>
                  <input type="number" min={1} max={12} className="input-field w-32" value={filters.month} onChange={e => setFilters({ ...filters, month: +e.target.value })} />
                </div>
              )}
              {tab === 'yearly' && (
                <div>
                  <label className="block text-sm mb-1">{t('students')}</label>
                  <select className="input-field" value={filters.studentId} onChange={e => setFilters({ ...filters, studentId: +e.target.value })}>
                    {students.map(s => <option key={s.id} value={s.id}>{s.fullName}</option>)}
                  </select>
                </div>
              )}
            </>
          )}
          <button onClick={generate} className="btn-primary">{loading ? t('loading') : 'إنشاء التقرير'}</button>
          {report && (
            <>
              <button onClick={exportExcel} className="btn-secondary flex items-center gap-2 no-print"><Download size={18} />{t('exportExcel')}</button>
              <button onClick={printReport} className="btn-secondary flex items-center gap-2 no-print"><Printer size={18} />{t('printPdf')}</button>
            </>
          )}
        </div>
      </div>

      {report && (
        <div className="card print-area">
          <h3 className="font-bold text-lg mb-4">{tabs.find(t => t.key === tab)?.label}</h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {Object.entries(report).filter(([k]) => !Array.isArray(k) && typeof report[k] !== 'object').map(([key, value]) => (
              <div key={key} className="stat-card">
                <p className="text-sm text-gray-500">{key}</p>
                <p className="text-xl font-bold">{String(value)}</p>
              </div>
            ))}
          </div>
          {Array.isArray(report.studentAbsences) && (
            <div className="mt-6 table-container">
              <table className="data-table">
                <thead><tr><th>{t('name')}</th><th>{t('universityNumber')}</th><th>الغيابات</th><th>{t('attendanceRate')}</th></tr></thead>
                <tbody>
                  {(report.studentAbsences as { fullName: string; universityNumber: string; absenceCount: number; attendanceRate: number }[]).map((s, i) => (
                    <tr key={i}><td>{s.fullName}</td><td>{s.universityNumber}</td><td>{s.absenceCount}</td><td>{s.attendanceRate}%</td></tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
