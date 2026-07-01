import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Search as SearchIcon } from 'lucide-react';
import { lookupApi } from '../api/services';
import type { Student, Lookups } from '../types';

export default function SearchPage() {
  const { t } = useTranslation();
  const [query, setQuery] = useState('');
  const [lookups, setLookups] = useState<Lookups | null>(null);
  const [filters, setFilters] = useState({ stageId: 0, sectionId: 0, subjectId: 0 });
  const [results, setResults] = useState<Student[]>([]);
  const [searched, setSearched] = useState(false);

  useEffect(() => {
    lookupApi.getAll().then(r => setLookups(r.data.data as Lookups));
  }, []);

  const handleSearch = async () => {
    const res = await lookupApi.search({
      query: query || undefined,
      stageId: filters.stageId || undefined,
      sectionId: filters.sectionId || undefined,
    });
    setResults(res.data.data || []);
    setSearched(true);
  };

  return (
    <div className="space-y-6">
      <div className="card">
        <div className="flex gap-3 mb-4">
          <div className="flex-1 relative">
            <SearchIcon className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400" size={20} />
            <input
              className="input-field pr-10"
              placeholder={t('searchPlaceholder')}
              value={query}
              onChange={e => setQuery(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && handleSearch()}
            />
          </div>
          <button onClick={handleSearch} className="btn-primary">{t('search')}</button>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <select className="input-field" value={filters.stageId} onChange={e => setFilters({ ...filters, stageId: +e.target.value })}>
            <option value={0}>{t('stage')} - الكل</option>
            {lookups?.stages.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <select className="input-field" value={filters.sectionId} onChange={e => setFilters({ ...filters, sectionId: +e.target.value })}>
            <option value={0}>{t('section')} - الكل</option>
            {lookups?.sections.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <select className="input-field" value={filters.subjectId} onChange={e => setFilters({ ...filters, subjectId: +e.target.value })}>
            <option value={0}>{t('subject')} - الكل</option>
            {lookups?.subjects.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        </div>
      </div>

      {searched && (
        <div className="card p-0 overflow-hidden">
          {results.length === 0 ? (
            <p className="p-6 text-center text-gray-500">{t('noData')}</p>
          ) : (
            <div className="table-container border-0">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>{t('name')}</th>
                    <th>{t('universityNumber')}</th>
                    <th>{t('stage')}</th>
                    <th>{t('section')}</th>
                    <th>{t('department')}</th>
                  </tr>
                </thead>
                <tbody>
                  {results.map(s => (
                    <tr key={s.id}>
                      <td>{s.fullName}</td>
                      <td>{s.universityNumber}</td>
                      <td>{s.stageName}</td>
                      <td>{s.sectionName}</td>
                      <td>{s.departmentName}</td>
                    </tr>
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
