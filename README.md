# نظام الحضور الأكاديمي | Academic Attendance System

نظام متكامل لإدارة حضور الطلاب في الجامعات والمعاهد.

## التقنيات | Tech Stack

| الطبقة | التقنية |
|--------|---------|
| Backend | ASP.NET Core 8 Web API |
| Frontend | React 18 + TypeScript + Vite |
| Database | Microsoft SQL Server |
| Auth | JWT Bearer |
| Excel | EPPlus |
| UI | Tailwind CSS + Chart.js |

## هيكل المشروع | Project Structure

```
ad/
├── backend/
│   ├── AcademicAttendance.sln
│   └── AcademicAttendance.API/
│       ├── Controllers/     # API endpoints
│       ├── Models/          # Entity models
│       ├── Services/        # Business logic
│       ├── Data/            # DbContext & Seeder
│       └── DTOs/            # Data transfer objects
└── frontend/
    └── src/
        ├── pages/           # Application pages
        ├── components/      # Shared components
        ├── api/             # API client
        └── i18n/            # Arabic/English translations
```

## المتطلبات | Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) أو LocalDB (مثبت مع Visual Studio)

## التشغيل | Getting Started

### 1. قاعدة البيانات

عدّل connection string في `backend/AcademicAttendance.API/appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AcademicAttendanceDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 2. Backend

```powershell
cd backend
dotnet restore
dotnet run --project AcademicAttendance.API
```

API: http://localhost:5000  
Swagger: http://localhost:5000/swagger

### 3. Frontend

```powershell
cd frontend
npm install
npm run dev
```

App: http://localhost:5173

## حسابات تجريبية | Demo Accounts

| الدور | البريد | كلمة المرور |
|-------|--------|-------------|
| مدير | admin@university.edu | Admin@123 |
| أستاذ | teacher@university.edu | Teacher@123 |

## الوحدات | Modules

1. **تسجيل الدخول** — JWT + استعادة كلمة المرور
2. **Dashboard** — إحصائيات الحضور + رسوم بيانية
3. **تسجيل الحضور** — فلاتر (مرحلة/شعبة/مادة/تاريخ) + حالات متعددة
4. **إدارة الطلاب** — CRUD + استيراد/تصدير Excel
5. **إدارة المواد** — CRUD + ربط بالأستاذ
6. **إدارة الأساتذة** — CRUD + تحديد المواد والشعب
7. **التقارير** — يومي / شهري / سنوي + Excel + طباعة
8. **البحث** — بالاسم، الرقم الجامعي، المرحلة، الشعبة
9. **الإشعارات** — تنبيهات الغياب وتسجيل الحضور
10. **لوحة الإدارة** — صلاحيات Admin كاملة

## API Endpoints

| Method | Endpoint | الوصف |
|--------|----------|-------|
| POST | /api/auth/login | تسجيل الدخول |
| GET | /api/dashboard | إحصائيات Dashboard |
| GET/POST | /api/students | إدارة الطلاب |
| POST | /api/students/import | استيراد Excel |
| GET | /api/students/export | تصدير Excel |
| GET/POST | /api/attendance | تسجيل الحضور |
| GET/POST | /api/subjects | إدارة المواد |
| GET/POST | /api/teachers | إدارة الأساتذة |
| GET | /api/reports/daily | التقرير اليومي |
| GET | /api/reports/monthly | التقرير الشهري |
| GET | /api/reports/yearly | التقرير السنوي |
| GET | /api/notifications | الإشعارات |
| GET | /api/lookup | القوائم المرجعية |



## قاعدة البيانات | Database Tables

Users, Teachers, Students, Departments, Stages, Sections, Subjects, TeacherSubjects, TeacherStageSections, Attendance, AttendanceDetails, Notifications, AuditLogs
