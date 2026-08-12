export interface MonthlyEnrollmentPoint {
  year: number;
  month: number;
  count: number;
}

export interface PlatformOverviewResponse {
  totalUsers: number;
  totalStudents: number;
  totalTeachers: number;
  activeCourses: number;
  totalSubmissions: number;
  totalRevenue: number;
  newStudentsThisMonth: number;
  newTeachersThisMonth: number;
  passedSubmissions: number;
  monthlyEnrollments: MonthlyEnrollmentPoint[];
  generatedAt: string;
}

export interface AuditLogResponse {
  id: number;
  userId?: number;
  userEmail?: string;
  action: string;
  entityType?: string;
  entityId?: string;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  createdAt: string;
}
