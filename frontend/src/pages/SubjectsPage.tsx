import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, BookOpen } from 'lucide-react';
import { subjectsApi, lookupApi } from '../api/services';
import type { Subject, Lookups } from '../types';

export default function SubjectsPage() {
  const { t } = useTranslation();
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [lookups, setLookups] = useState<Lookups | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({ name: '', code: '', stageId: 1, departmentId: 1, creditHours: 3 });
  const [error, setError] = useState('');

  const load = () => subjectsApi.getAll().then(r => setSubjects(r.data.data || []));

  useEffect(() => {
    lookupApi.getAll().then(r => setLookups(r.data.data as Lookups));
    load();
  }, []);

  const openAdd = () => {
    setEditId(null);
    setForm({ name: '', code: '', stageId: 1, departmentId: 1, creditHours: 3 });
    setError('');
    setShowModal(true);
  };

  const openEdit = (s: Subject) => {
    setEditId(s.id);
    setForm({ name: s.name, code: s.code, stageId: s.stageId, departmentId: s.departmentId, creditHours: s.creditHours });
    setError('');
    setShowModal(true);
  };

  const handleSave = async () => {
    if (!form.name.trim() || !form.code.trim()) {
      setError('يرجى إدخال اسم المادة والكود');
      return;
    }
    setError('');
    try {
      if (editId) await subjectsApi.update(editId, form);
      else await subjectsApi.create(form);
      setShowModal(false);
      load();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'فشل حفظ المادة');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('هل أنت متأكد من حذف هذه المادة؟')) return;
    try {
      await subjectsApi.delete(id);
      load();
    } catch {
      alert('فشل حذف المادة');
    }
  };

  return (
    <div className="space-y-6">
      <div className="card bg-gradient-to-l from-primary-600 to-primary-700 text-white">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <BookOpen size={32} />
            <div>
              <h2 className="text-xl font-bold">{t('subjects')}</h2>
              <p className="text-primary-100 text-sm">إدارة المواد الدراسية — مدير النظام فقط</p>
            </div>
          </div>
          <button onClick={openAdd} className="bg-white text-primary-700 px-4 py-2 rounded-lg hover:bg-primary-50 flex items-center gap-2 font-medium">
            <Plus size={18} />{t('add')}
          </button>
        </div>
      </div>

      <div className="card p-0 overflow-hidden">
        <div className="table-container border-0">
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('name')}</th>
                <th>{t('code')}</th>
                <th>{t('stage')}</th>
                <th>{t('department')}</th>
                <th>{t('creditHours')}</th>
                <th>{t('actions')}</th>
              </tr>
            </thead>
            <tbody>
              {subjects.length === 0 ? (
                <tr><td colSpan={6} className="text-center text-gray-500 py-6">{t('noData')}</td></tr>
              ) : subjects.map(s => (
                <tr key={s.id}>
                  <td>{s.name}</td>
                  <td>{s.code}</td>
                  <td>{s.stageName}</td>
                  <td>{s.departmentName}</td>
                  <td>{s.creditHours}</td>
                  <td>
                    <div className="flex gap-2">
                      <button onClick={() => openEdit(s)} className="p-1 hover:text-primary-600" title={t('edit')}><Pencil size={16} /></button>
                      <button onClick={() => handleDelete(s.id)} className="p-1 hover:text-red-600" title={t('delete')}><Trash2 size={16} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="card w-full max-w-md space-y-4">
            <h3 className="font-semibold text-lg">{editId ? t('edit') : t('add')} {t('subject')}</h3>
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <div>
              <label className="block text-sm font-medium mb-1">{t('name')}</label>
              <input className="input-field" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('code')}</label>
              <input className="input-field" value={form.code} onChange={e => setForm({ ...form, code: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('stage')}</label>
              <select className="input-field" value={form.stageId} onChange={e => setForm({ ...form, stageId: +e.target.value })}>
                {lookups?.stages.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('department')}</label>
              <select className="input-field" value={form.departmentId} onChange={e => setForm({ ...form, departmentId: +e.target.value })}>
                {lookups?.departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('creditHours')}</label>
              <input type="number" min={1} className="input-field" value={form.creditHours} onChange={e => setForm({ ...form, creditHours: +e.target.value })} />
            </div>
            <div className="flex gap-2 pt-2">
              <button onClick={handleSave} className="btn-primary flex-1">{t('save')}</button>
              <button onClick={() => setShowModal(false)} className="btn-secondary flex-1">{t('cancel')}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
