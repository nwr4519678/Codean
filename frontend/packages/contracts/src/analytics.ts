export interface PlatformOverviewResponse {
  totalStudents: number;
  totalTeachers: number;
  totalCourses: number;
  totalSubmissions: number;
  monthlyRevenue: number;
  activeUsersToday: number;
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
