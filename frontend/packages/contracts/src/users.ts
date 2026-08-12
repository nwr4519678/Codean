import { UserRole } from './enums';

export interface StudentProfileResponse {
  userId: number;
  email: string;
  fullName: string;
  phone?: string;
  grade?: string;
  school?: string;
  parentPhone?: string;
  parentPhone2?: string;
  notes?: string;
}

export interface TeacherProfileResponse {
  userId: number;
  email: string;
  fullName: string;
  phone?: string;
  biography?: string;
  photo?: string;
  facebook?: string;
  youTube?: string;
  website?: string;
  experience?: string;
  specialization?: string;
  isVerified: boolean;
  approvedAt?: string;
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
