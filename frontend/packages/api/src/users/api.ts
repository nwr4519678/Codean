import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import {
  StudentProfileResponse,
  TeacherProfileResponse,
  AdminUserItem,
  PagedList,
} from '@platform/contracts';

export const usersApi = {
  getStudentProfile: async (): Promise<StudentProfileResponse> => {
    const res = await apiClient.get<StudentProfileResponse>(API_URLS.USERS.STUDENT_PROFILE);
    return res.data;
  },

  getTeacherProfile: async (): Promise<TeacherProfileResponse> => {
    const res = await apiClient.get<TeacherProfileResponse>(API_URLS.USERS.TEACHER_PROFILE);
    return res.data;
  },

  updateStudentProfile: async (payload: Partial<StudentProfileResponse>): Promise<StudentProfileResponse> => {
    const res = await apiClient.put<StudentProfileResponse>(API_URLS.USERS.UPDATE_STUDENT_PROFILE, payload);
    return res.data;
  },

  getUsersPaged: async (params?: { pageNumber?: number; pageSize?: number; search?: string }): Promise<PagedList<AdminUserItem>> => {
    const res = await apiClient.get<PagedList<AdminUserItem>>(API_URLS.USERS.USERS_PAGED, { params });
    return res.data;
  },

  assignUserRole: async (userId: number, role: string): Promise<void> => {
    await apiClient.put(API_URLS.USERS.ASSIGN_ROLE(userId), { role });
  },

  setUserStatus: async (userId: number, isActive: boolean): Promise<void> => {
    await apiClient.put(API_URLS.USERS.SET_STATUS(userId), { isActive });
  },
};
