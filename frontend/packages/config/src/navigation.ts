import { ROUTES } from './routes';

export interface NavItem {
  titleKey: string;
  href: string;
  iconName: string;
  badgeKey?: string;
  permission?: string;
}

export const STUDENT_NAV: NavItem[] = [
  { titleKey: 'nav.dashboard', href: ROUTES.STUDENT.DASHBOARD, iconName: 'LayoutDashboard' },
  { titleKey: 'nav.courses', href: ROUTES.PUBLIC.COURSES, iconName: 'BookOpen' },
  { titleKey: 'nav.judge', href: ROUTES.STUDENT.JUDGE, iconName: 'Code2' },
  { titleKey: 'nav.liveSessions', href: ROUTES.STUDENT.LIVE_SESSIONS, iconName: 'Video' },
  { titleKey: 'nav.notifications', href: ROUTES.STUDENT.NOTIFICATIONS, iconName: 'Bell' },
  { titleKey: 'nav.billing', href: ROUTES.STUDENT.BILLING, iconName: 'CreditCard' },
  { titleKey: 'nav.settings', href: ROUTES.STUDENT.SETTINGS, iconName: 'Settings' },
];

export const TEACHER_NAV: NavItem[] = [
  { titleKey: 'nav.teacherDashboard', href: ROUTES.TEACHER.DASHBOARD, iconName: 'BarChart3' },
  { titleKey: 'nav.myCourses', href: ROUTES.TEACHER.COURSES, iconName: 'GraduationCap' },
  { titleKey: 'nav.createCourse', href: ROUTES.TEACHER.CREATE_COURSE, iconName: 'PlusCircle' },
  { titleKey: 'nav.settings', href: ROUTES.STUDENT.SETTINGS, iconName: 'Settings' },
];

export const ADMIN_NAV: NavItem[] = [
  { titleKey: 'nav.adminOverview', href: ROUTES.ADMIN.DASHBOARD, iconName: 'ShieldAlert' },
  { titleKey: 'nav.userManagement', href: ROUTES.ADMIN.USERS, iconName: 'Users' },
  { titleKey: 'nav.allCourses', href: ROUTES.ADMIN.COURSES, iconName: 'BookOpen' },
  { titleKey: 'nav.auditLogs', href: ROUTES.ADMIN.AUDIT_LOGS, iconName: 'FileText' },
  { titleKey: 'nav.subscriptions', href: ROUTES.ADMIN.SUBSCRIPTIONS, iconName: 'CreditCard' },
];
