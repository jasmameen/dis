import api from './client';
import type { ApiResponse, User, DashboardStats, Student, Subject, AttendanceRow, Lookups, Notification, Teacher } from '../types';

export const authApi = {
  login: (login: string, password: string) =>
    api.post<ApiResponse<User>>('/auth/login', { login, password }),
  registerTeacher: (data: {
    fullName: string;
    email: string;
    username: string;
    password: string;
    phone?: string;
    departmentId?: number;
  }) => api.post<ApiResponse<boolean>>('/auth/register-teacher', data),
  forgotPassword: (email: string) =>
    api.post<ApiResponse<boolean>>('/auth/forgot-password', { email }),
};

export const dashboardApi = {
  getStats: () => api.get<ApiResponse<DashboardStats>>('/dashboard'),
};

export const studentsApi = {
  getAll: (params?: { stageId?: number; sectionId?: number }) =>
    api.get<ApiResponse<Student[]>>('/students', { params }),
  create: (data: Partial<Student>) => api.post<ApiResponse<Student>>('/students', data),
  update: (id: number, data: Partial<Student>) => api.put<ApiResponse<Student>>(`/students/${id}`, data),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/students/${id}`),
  import: (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return api.post<ApiResponse<number>>('/students/import', form);
  },
  export: (params?: { stageId?: number; sectionId?: number }) =>
    api.get('/students/export', { params, responseType: 'blob' }),
};

export const attendanceApi = {
  get: (params: { stageId: number; sectionId: number; subjectId: number; date: string }) =>
    api.get<ApiResponse<AttendanceRow[]>>('/attendance', { params }),
  save: (data: object) => api.post<ApiResponse<boolean>>('/attendance', data),
};

export const subjectsApi = {
  getAll: () => api.get<ApiResponse<Subject[]>>('/subjects'),
  create: (data: object) => api.post<ApiResponse<Subject>>('/subjects', data),
  update: (id: number, data: object) => api.put<ApiResponse<Subject>>(`/subjects/${id}`, data),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/subjects/${id}`),
};

export const teachersApi = {
  getAll: () => api.get<ApiResponse<Teacher[]>>('/teachers'),
  create: (data: object) => api.post<ApiResponse<Teacher>>('/teachers', data),
  update: (id: number, data: object) => api.put<ApiResponse<Teacher>>(`/teachers/${id}`, data),
  delete: (id: number) => api.delete<ApiResponse<boolean>>(`/teachers/${id}`),
};

export const reportsApi = {
  daily: (params?: object) => api.get<ApiResponse<object>>('/reports/daily', { params }),
  monthly: (year: number, month: number, params?: object) =>
    api.get<ApiResponse<object>>('/reports/monthly', { params: { year, month, ...params } }),
  yearly: (year: number, studentId: number) =>
    api.get<ApiResponse<object>>('/reports/yearly', { params: { year, studentId } }),
  exportDaily: (params?: object) =>
    api.get('/reports/daily/export', { params, responseType: 'blob' }),
};

export const notificationsApi = {
  getAll: () => api.get<ApiResponse<Notification[]>>('/notifications'),
  markRead: (id: number) => api.put<ApiResponse<boolean>>(`/notifications/${id}/read`),
};

export const lookupApi = {
  getAll: () => api.get<ApiResponse<Lookups>>('/lookup'),
  search: (params: object) => api.get<ApiResponse<Student[]>>('/lookup/search', { params }),
};
