import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { teachersApi, lookupApi, subjectsApi } from '../api/services';
import type { Teacher, Lookups, Subject } from '../types';

export default function TeachersPage() {
  const { t } = useTranslation();
  const [teachers, setTeachers] = useState<Teacher[]>([]);
  const [lookups, setLookups] = useState<Lookups | null>(null);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [form, setForm] = useState({
    fullName: '', email: '', username: '', password: '', phone: '',
    departmentId: 1, subjectIds: [] as number[],
    stageSections: [{ stageId: 1, sectionId: 1, stageName: '', sectionName: '' }],
  });

  const load = () => teachersApi.getAll().then(r => setTeachers(r.data.data || []));

  useEffect(() => {
    lookupApi.getAll().then(r => setLookups(r.data.data as Lookups));
    subjectsApi.getAll().then(r => setSubjects(r.data.data || []));
    load();
  }, []);

  const handleSave = async () => {
    const data = {
      ...form,
      stageSections: form.stageSections.map(ss => ({ stageId: ss.stageId, sectionId: ss.sectionId, stageName: '', sectionName: '' })),
    };
    if (editId) await teachersApi.update(editId, data);
    else await teachersApi.create(data);
    setShowModal(false);
    load();
  };

  const toggleSubject = (id: number) => {
    setForm(f => ({
      ...f,
      subjectIds: f.subjectIds.includes(id) ? f.subjectIds.filter(s => s !== id) : [...f.subjectIds, id],
    }));
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-end">
        <button onClick={() => { setEditId(null); setShowModal(true); }} className="btn-primary flex items-center gap-2">
          <Plus size={18} />{t('add')}
        </button>
      </div>

      <div className="card p-0 overflow-hidden">
        <div className="table-container border-0">
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('fullName')}</th>
                <th>{t('email')}</th>
                <th>{t('department')}</th>
                <th>{t('subject')}</th>
                <th>{t('actions')}</th>
              </tr>
            </thead>
            <tbody>
              {teachers.map(te => (
                <tr key={te.id}>
                  <td>{te.fullName}</td>
                  <td>{te.email}</td>
                  <td>{te.departmentName}</td>
                  <td>{te.subjectIds.length} {t('subject')}</td>
                  <td>
                    <div className="flex gap-2">
                      <button onClick={() => { setEditId(te.id); setForm({ fullName: te.fullName, email: te.email, username: te.username, password: '', phone: te.phone || '', departmentId: te.departmentId || 1, subjectIds: te.subjectIds, stageSections: te.stageSections }); setShowModal(true); }} className="p-1 hover:text-primary-600"><Pencil size={16} /></button>
                      <button onClick={async () => { if (confirm('؟')) { await teachersApi.delete(te.id); load(); } }} className="p-1 hover:text-red-600"><Trash2 size={16} /></button>
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
          <div className="card w-full max-w-lg space-y-4 max-h-[90vh] overflow-y-auto">
            <h3 className="font-semibold">{editId ? t('edit') : t('add')} {t('teachers')}</h3>
            <input className="input-field" placeholder={t('fullName')} value={form.fullName} onChange={e => setForm({ ...form, fullName: e.target.value })} />
            <input className="input-field" placeholder={t('email')} value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} />
            <input className="input-field" placeholder={t('username')} value={form.username} onChange={e => setForm({ ...form, username: e.target.value })} />
            <input type="password" className="input-field" placeholder={t('password')} value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} />
            <input className="input-field" placeholder={t('phone')} value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
            <div>
              <label className="text-sm font-medium">{t('subject')}</label>
              <div className="flex flex-wrap gap-2 mt-1">
                {subjects.map(s => (
                  <button key={s.id} type="button" onClick={() => toggleSubject(s.id)}
                    className={`px-3 py-1 rounded text-sm ${form.subjectIds.includes(s.id) ? 'bg-primary-600 text-white' : 'bg-gray-200 dark:bg-gray-700'}`}>
                    {s.name}
                  </button>
                ))}
              </div>
            </div>
            <div className="flex gap-2">
              <select className="input-field" value={form.stageSections[0]?.stageId} onChange={e => setForm({ ...form, stageSections: [{ ...form.stageSections[0], stageId: +e.target.value }] })}>
                {lookups?.stages.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
              <select className="input-field" value={form.stageSections[0]?.sectionId} onChange={e => setForm({ ...form, stageSections: [{ ...form.stageSections[0], sectionId: +e.target.value }] })}>
                {lookups?.sections.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
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
