import { apiClient } from '../client';
import { authApi } from '../auth/api';
import { API_URLS } from '@platform/config';
import {
  StudentProfileResponse,
  TeacherProfileResponse,
  AdminUserItem,
  PagedList,
} from '@platform/contracts';

export interface StudentProfileUpdate {
  grade?: string;
  school?: string;
  parentPhone?: string;
  parentPhone2?: string;
  notes?: string;
}

export const usersApi = {
  getStudentProfile: async (): Promise<StudentProfileResponse> => {
    const user = await authApi.getCurrentUser();
    const res = await apiClient.get<StudentProfileResponse>(API_URLS.USERS.STUDENT_PROFILE(user.id));
    return res.data;
  },

  getTeacherProfile: async (): Promise<TeacherProfileResponse> => {
    const user = await authApi.getCurrentUser();
    const res = await apiClient.get<TeacherProfileResponse>(API_URLS.USERS.TEACHER_PROFILE(user.id));
    return res.data;
  },

  updateStudentProfile: async (payload: StudentProfileUpdate): Promise<StudentProfileResponse> => {
    const res = await apiClient.put<StudentProfileResponse>(API_URLS.USERS.UPDATE_STUDENT_PROFILE, payload);
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
};
