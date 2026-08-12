import { apiClient } from '../client';
import { authApi } from '../auth/api';
import { API_URLS } from '@platform/config';
import {
  StudentProfileResponse,
  TeacherProfileResponse,
  AdminUserItem,
  PagedList,
  RegisterResponse,
} from '@platform/contracts';

export interface StudentProfileUpdate {
  fullName?: string;
  phone?: string;
  grade?: string;
  school?: string;
  parentPhone?: string;
  parentPhone2?: string;
  notes?: string;
}

export interface TeacherProfileUpdate {
  fullName?: string;
  phone?: string;
  biography?: string;
  facebook?: string;
  youTube?: string;
  website?: string;
  experience?: string;
  specialization?: string;
  photo?: string;
}

export const usersApi = {
  getStudentProfile: async (): Promise<StudentProfileResponse> => {
    const user = await authApi.getCurrentUser();
    const id = user.userId ?? user.id!;
    const res = await apiClient.get<StudentProfileResponse>(API_URLS.USERS.STUDENT_PROFILE(id));
    return res.data;
  },

  getTeacherProfile: async (): Promise<TeacherProfileResponse> => {
    const user = await authApi.getCurrentUser();
    const id = user.userId ?? user.id!;
    const res = await apiClient.get<TeacherProfileResponse>(API_URLS.USERS.TEACHER_PROFILE(id));
    return res.data;
  },

  updateStudentProfile: async (payload: StudentProfileUpdate): Promise<StudentProfileResponse> => {
    const res = await apiClient.put<StudentProfileResponse>(API_URLS.USERS.UPDATE_STUDENT_PROFILE, payload);
    return res.data;
  },

  updateTeacherProfile: async (payload: TeacherProfileUpdate): Promise<TeacherProfileResponse> => {
    const res = await apiClient.put<TeacherProfileResponse>(API_URLS.USERS.UPDATE_TEACHER_PROFILE, payload);
    return res.data;
  },

  getUsersPaged: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<PagedList<AdminUserItem>> => {
    const res = await apiClient.get<PagedList<AdminUserItem>>(API_URLS.USERS.USERS_PAGED, { params });
    return res.data;
  },

  assignUserRole: async (userId: number, roleId: number): Promise<void> => {
    await apiClient.put(API_URLS.USERS.ASSIGN_ROLE(userId), { roleId });
  },

  setUserStatus: async (userId: number, isActive: boolean): Promise<void> => {
    await apiClient.put(API_URLS.USERS.SET_STATUS(userId), { isActive });
  },

  adminCreateUser: async (payload: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    role?: string;
  }): Promise<RegisterResponse> => {
    const res = await apiClient.post<RegisterResponse>(API_URLS.USERS.CREATE_USER, payload);
    return res.data;
  },

  deleteUser: async (userId: number): Promise<void> => {
    await apiClient.delete(`/api/users/${userId}`);
  },
};
