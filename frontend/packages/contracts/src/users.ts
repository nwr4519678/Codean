import { UserRole } from './enums';

export interface StudentProfileResponse {
  userId: number;
  bio?: string;
  githubUrl?: string;
  linkedinUrl?: string;
  websiteUrl?: string;
  enrolledCourseCount: number;
  completedCourseCount: number;
  solvedChallengeCount: number;
  totalPoints: number;
}

export interface TeacherProfileResponse {
  userId: number;
  headline?: string;
  bio?: string;
  expertiseKeywords: string[];
  totalStudents: number;
  totalCourses: number;
  averageRating: number;
}

export interface AdminUserItem {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
  emailConfirmed: boolean;
  createdAt: string;
}
