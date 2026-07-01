import { useEffect, useState, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Upload, Download, Pencil, Trash2, ShieldAlert } from 'lucide-react';
import { studentsApi, lookupApi } from '../api/services';
import { useAuth } from '../context/AuthContext';
import type { Student, Lookups } from '../types';

const emptyForm = { fullName: '', universityNumber: '', stageId: 1, sectionId: 1, departmentId: 1 };

export default function StudentsPage() {
  const { t } = useTranslation();
  const { isAdmin } = useAuth();
  const [students, setStudents] = useState<Student[]>([]);
  const [lookups, setLookups] = useState<Lookups | null>(null);
  const [filter, setFilter] = useState({ stageId: 0, sectionId: 0 });
  const [showModal, setShowModal] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState('');
  const fileRef = useRef<HTMLInputElement>(null);

  const load = () => {
    studentsApi.getAll({
      stageId: filter.stageId || undefined,
      sectionId: filter.sectionId || undefined,
    }).then(r => setStudents(r.data.data || []));
  };

  useEffect(() => {
    lookupApi.getAll().then(r => setLookups(r.data.data as Lookups));
    load();
  }, [filter]);

  const openAdd = () => { setEditId(null); setForm(emptyForm); setError(''); setShowModal(true); };
  const openEdit = (s: Student) => {
    setEditId(s.id);
    setForm({ fullName: s.fullName, universityNumber: s.universityNumber, stageId: s.stageId, sectionId: s.sectionId, departmentId: s.departmentId });
    setError('');
    setShowModal(true);
  };

  const handleSave = async () => {
    setError('');
    try {
      if (editId) await studentsApi.update(editId, form);
      else await studentsApi.create(form);
      setShowModal(false);
      load();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setError(msg || 'فشل حفظ بيانات الطالب');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('هل أنت متأكد من حذف هذا الطالب؟')) return;
    try {
      await studentsApi.delete(id);
      load();
    } catch {
      alert('فشل حذف الطالب');
    }
  };

  const handleImport = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const res = await studentsApi.import(file);
      alert(res.data.message);
      load();
    } catch {
      alert('فشل استيراد الملف');
    }
    e.target.value = '';
  };

  const handleExport = async () => {
    const res = await studentsApi.export({ stageId: filter.stageId || undefined, sectionId: filter.sectionId || undefined });
    const url = URL.createObjectURL(res.data);
    const a = document.createElement('a');
    a.href = url; a.download = 'students.xlsx'; a.click();
  };

  return (
    <div className="space-y-6">
      {!isAdmin && (
        <div className="flex items-center gap-2 p-3 bg-amber-50 dark:bg-amber-900/20 text-amber-800 dark:text-amber-200 rounded-lg text-sm">
          <ShieldAlert size={18} />
          عرض فقط — إضافة وتعديل الطلاب متاحة لمدير النظام
        </div>
      )}

      <div className="flex flex-wrap gap-3 items-center justify-between">
        <div className="flex gap-3">
          <select className="input-field w-auto" value={filter.stageId} onChange={e => setFilter({ ...filter, stageId: +e.target.value })}>
            <option value={0}>{t('stage')} - الكل</option>
            {lookups?.stages.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <select className="input-field w-auto" value={filter.sectionId} onChange={e => setFilter({ ...filter, sectionId: +e.target.value })}>
            <option value={0}>{t('section')} - الكل</option>
            {lookups?.sections.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        </div>
        <div className="flex gap-2">
          {isAdmin && (
            <>
              <button onClick={openAdd} className="btn-primary flex items-center gap-2"><Plus size={18} />{t('add')}</button>
              <button onClick={() => fileRef.current?.click()} className="btn-secondary flex items-center gap-2"><Upload size={18} />{t('import')}</button>
              <input ref={fileRef} type="file" accept=".xlsx,.xls,.csv" className="hidden" onChange={handleImport} />
            </>
          )}
          <button onClick={handleExport} className="btn-secondary flex items-center gap-2"><Download size={18} />{t('export')}</button>
        </div>
      </div>

      <div className="card p-0 overflow-hidden">
        <div className="table-container border-0">
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('name')}</th>
                <th>{t('universityNumber')}</th>
                <th>{t('stage')}</th>
                <th>{t('section')}</th>
                <th>{t('department')}</th>
                {isAdmin && <th>{t('actions')}</th>}
              </tr>
            </thead>
            <tbody>
              {students.length === 0 ? (
                <tr><td colSpan={isAdmin ? 6 : 5} className="text-center text-gray-500 py-6">{t('noData')}</td></tr>
              ) : students.map(s => (
                <tr key={s.id}>
                  <td>{s.fullName}</td>
                  <td>{s.universityNumber}</td>
                  <td>{s.stageName}</td>
                  <td>{s.sectionName}</td>
                  <td>{s.departmentName}</td>
                  {isAdmin && (
                    <td>
                      <div className="flex gap-2">
                        <button onClick={() => openEdit(s)} className="p-1 hover:text-primary-600" title={t('edit')}><Pencil size={16} /></button>
                        <button onClick={() => handleDelete(s.id)} className="p-1 hover:text-red-600" title={t('delete')}><Trash2 size={16} /></button>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && isAdmin && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="card w-full max-w-md space-y-4">
            <h3 className="font-semibold text-lg">{editId ? t('edit') : t('add')} {t('students')}</h3>
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <input className="input-field" placeholder={t('fullName')} value={form.fullName} onChange={e => setForm({ ...form, fullName: e.target.value })} />
            <input className="input-field" placeholder={t('universityNumber')} value={form.universityNumber} onChange={e => setForm({ ...form, universityNumber: e.target.value })} />
            <select className="input-field" value={form.stageId} onChange={e => setForm({ ...form, stageId: +e.target.value })}>
              {lookups?.stages.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
            <select className="input-field" value={form.sectionId} onChange={e => setForm({ ...form, sectionId: +e.target.value })}>
              {lookups?.sections.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
            <select className="input-field" value={form.departmentId} onChange={e => setForm({ ...form, departmentId: +e.target.value })}>
              {lookups?.departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
            <div className="flex gap-2">
              <button onClick={handleSave} className="btn-primary flex-1">{t('save')}</button>
              <button onClick={() => setShowModal(false)} className="btn-secondary flex-1">{t('cancel')}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
