import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Save, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { attendanceApi, lookupApi, subjectsApi } from '../api/services';
import type { AttendanceRow, AttendanceStatus, Lookups, Subject } from '../types';
import { statusLabels, statusToApiNumber, normalizeStatus } from '../types';

export default function AttendancePage() {
  const { t, i18n } = useTranslation();
  const [lookups, setLookups] = useState<Lookups | null>(null);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [filters, setFilters] = useState({ stageId: 1, sectionId: 1, subjectId: 1, date: new Date().toISOString().split('T')[0] });
  const [rows, setRows] = useState<AttendanceRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    lookupApi.getAll().then(r => setLookups(r.data.data as Lookups));
    subjectsApi.getAll().then(r => {
      const list = r.data.data || [];
      setSubjects(list);
      if (list.length) setFilters(f => ({ ...f, subjectId: list[0].id }));
    });
  }, []);

  const loadStudents = async () => {
    setLoading(true);
    setSaved(false);
    setError('');
    try {
      const res = await attendanceApi.get(filters);
      const data = (res.data.data || []).map(row => ({
        ...row,
        status: normalizeStatus(row.status),
      }));
      setRows(data);
    } catch {
      setRows([]);
      setError('تعذر تحميل قائمة الطلاب');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { if (filters.subjectId) loadStudents(); }, [filters.stageId, filters.sectionId, filters.subjectId, filters.date]);

  const setStatus = (studentId: number, status: AttendanceStatus) => {
    setRows(prev => prev.map(r => r.studentId === studentId ? { ...r, status } : r));
    setSaved(false);
    setError('');
  };

  const markAll = (status: AttendanceStatus) => {
    setRows(prev => prev.map(r => ({ ...r, status })));
    setSaved(false);
  };

  const handleSave = async () => {
    if (rows.length === 0) {
      setError('لا يوجد طلاب لتسجيل حضورهم');
      return;
    }
    setError('');
    try {
      const res = await attendanceApi.save({
        stageId: filters.stageId,
        sectionId: filters.sectionId,
        subjectId: filters.subjectId,
        date: filters.date,
        entries: rows.map(r => ({
          studentId: r.studentId,
          status: statusToApiNumber(r.status),
        })),
      });
      if (res.data.success) {
        setSaved(true);
        await loadStudents();
      } else {
        setError(res.data.message || 'فشل حفظ الحضور');
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'حدث خطأ أثناء الحفظ');
    }
  };

  const lang = i18n.language as 'ar' | 'en';

  return (
    <div className="space-y-6">
      <div className="card">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">{t('stage')}</label>
            <select className="input-field" value={filters.stageId} onChange={e => setFilters({ ...filters, stageId: +e.target.value })}>
              {lookups?.stages.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('section')}</label>
            <select className="input-field" value={filters.sectionId} onChange={e => setFilters({ ...filters, sectionId: +e.target.value })}>
              {lookups?.sections.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('subject')}</label>
            <select className="input-field" value={filters.subjectId} onChange={e => setFilters({ ...filters, subjectId: +e.target.value })}>
              {subjects.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">{t('date')}</label>
            <input type="date" className="input-field" value={filters.date} onChange={e => setFilters({ ...filters, date: e.target.value })} />
          </div>
        </div>
      </div>

      {error && (
        <div className="flex items-center gap-2 p-3 bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-300 rounded-lg text-sm">
          <AlertCircle size={18} />
          {error}
        </div>
      )}

      <div className="flex flex-wrap gap-2">
        <button onClick={() => markAll('Present')} className="btn-secondary flex items-center gap-2">
          <CheckCircle size={18} className="text-green-600" /> {t('markAllPresent')}
        </button>
        <button onClick={() => markAll('Absent')} className="btn-secondary flex items-center gap-2">
          <XCircle size={18} className="text-red-600" /> {t('markAllAbsent')}
        </button>
        <button onClick={handleSave} disabled={loading || rows.length === 0} className="btn-primary flex items-center gap-2 mr-auto">
          <Save size={18} /> {t('saveAttendance')}
        </button>
        {saved && <span className="text-green-600 text-sm self-center">✓ تم الحفظ بنجاح</span>}
      </div>

      <div className="card p-0 overflow-hidden">
        {loading ? (
          <p className="p-6 text-center">{t('loading')}</p>
        ) : rows.length === 0 ? (
          <p className="p-6 text-center text-gray-500">{t('noData')}</p>
        ) : (
          <div className="table-container border-0">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('name')}</th>
                  <th>{t('universityNumber')}</th>
                  <th>{t('status')}</th>
                </tr>
              </thead>
              <tbody>
                {rows.map(row => (
                  <tr key={row.studentId}>
                    <td>{row.fullName}</td>
                    <td>{row.universityNumber}</td>
                    <td>
                      <div className="flex gap-1 flex-wrap">
                        {(['Present', 'Absent', 'Late', 'Leave'] as AttendanceStatus[]).map(s => (
                          <button
                            key={s}
                            onClick={() => setStatus(row.studentId, s)}
                            className={`px-2 py-1 rounded text-xs transition-all ${
                              row.status === s
                                ? statusLabels[s].color + ' ring-2 ring-offset-1 ring-primary-400'
                                : 'bg-gray-100 dark:bg-gray-700 text-gray-600'
                            }`}
                          >
                            {statusLabels[s][lang]}
                          </button>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
