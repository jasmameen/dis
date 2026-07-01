export type UserRole = 'Admin' | 'Teacher';

export type AttendanceStatus = 'Present' | 'Absent' | 'Late' | 'Leave';

export const statusToNumber: Record<AttendanceStatus, number> = {
  Present: 0, Absent: 1, Late: 2, Leave: 3,
};

export const numberToStatus: Record<number, AttendanceStatus> = {
  0: 'Present', 1: 'Absent', 2: 'Late', 3: 'Leave',
};

export function normalizeRole(role: unknown): UserRole {
  if (role === 'Admin' || role === 0) return 'Admin';
  return 'Teacher';
}

export function isAdminRole(role: unknown): boolean {
  return normalizeRole(role) === 'Admin';
}

export function normalizeStatus(status: unknown): AttendanceStatus {
  if (typeof status === 'string' && status in statusToNumber) return status as AttendanceStatus;
  if (typeof status === 'number') return numberToStatus[status] ?? 'Present';
  return 'Present';
}

export function statusToApiNumber(status: unknown): number {
  if (typeof status === 'number') return status;
  if (typeof status === 'string' && status in statusToNumber) return statusToNumber[status as AttendanceStatus];
  return 0;
}

export interface User {
  token: string;
  username: string;
  email: string;
  role: UserRole;
  teacherId?: number;
  fullName?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface LookupDto {
  id: number;
  name: string;
}

export interface Lookups {
  departments: LookupDto[];
  stages: LookupDto[];
  sections: LookupDto[];
  subjects: LookupDto[];
}

export interface DashboardStats {
  assignedSubjects: number;
  totalStudents: number;
  presentToday: number;
  absentToday: number;
  attendanceRateToday: number;
  weeklyStats: { label: string; present: number; absent: number }[];
}

export interface Student {
  id: number;
  fullName: string;
  universityNumber: string;
  stageId: number;
  stageName: string;
  sectionId: number;
  sectionName: string;
  departmentId: number;
  departmentName: string;
  isActive: boolean;
}

export interface Subject {
  id: number;
  name: string;
  code: string;
  stageId: number;
  stageName: string;
  departmentId: number;
  departmentName: string;
  creditHours: number;
}

export interface AttendanceRow {
  studentId: number;
  fullName: string;
  universityNumber: string;
  status: AttendanceStatus;
  detailId?: number;
}

export interface Notification {
  id: number;
  type: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface Teacher {
  id: number;
  userId: number;
  fullName: string;
  email: string;
  username: string;
  phone?: string;
  departmentId?: number;
  departmentName?: string;
  subjectIds: number[];
  stageSections: { stageId: number; sectionId: number; stageName: string; sectionName: string }[];
}

export const statusLabels: Record<AttendanceStatus, { ar: string; en: string; color: string }> = {
  Present: { ar: 'حاضر', en: 'Present', color: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' },
  Absent: { ar: 'غائب', en: 'Absent', color: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200' },
  Late: { ar: 'متأخر', en: 'Late', color: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200' },
  Leave: { ar: 'إجازة', en: 'Leave', color: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200' },
};
