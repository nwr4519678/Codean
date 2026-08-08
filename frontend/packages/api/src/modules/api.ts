import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import { ModuleResponse } from '@platform/contracts';

export const modulesApi = {
  getByCourse: async (courseId: number | string): Promise<ModuleResponse[]> => {
    const res = await apiClient.get<ModuleResponse[]>(API_URLS.COURSES.MODULES(courseId));
    return res.data;
  },

  create: async (payload: {
    courseId: number;
    title: string;
    description?: string;
    monthNumber: number;
    order: number;
  }): Promise<ModuleResponse> => {
    const res = await apiClient.post<ModuleResponse>('/api/modules', payload);
    return res.data;
  },

  update: async (
    id: number | string,
    payload: { title: string; description?: string; monthNumber?: number; order?: number }
  ): Promise<ModuleResponse> => {
    const res = await apiClient.put<ModuleResponse>(`/api/modules/${id}`, payload);
    return res.data;
  },

  delete: async (id: number | string): Promise<void> => {
    await apiClient.delete(`/api/modules/${id}`);
  },

  createLesson: async (payload: {
    moduleId: number;
    title: string;
    description?: string;
    videoUrl?: string;
    duration?: number;
    order: number;
  }): Promise<unknown> => {
    const res = await apiClient.post(API_URLS.COURSES.LESSONS(payload.moduleId), payload);
    return res.data;
  },

  updateLesson: async (
    id: number | string,
    payload: { title: string; description?: string; videoUrl?: string; duration?: number }
  ): Promise<unknown> => {
    const res = await apiClient.put(`/api/lessons/${id}`, payload);
    return res.data;
  },

  publishLesson: async (id: number | string): Promise<void> => {
    await apiClient.post(`/api/lessons/${id}/publish`);
  },

  deleteLesson: async (id: number | string): Promise<void> => {
    await apiClient.delete(`/api/lessons/${id}`);
  },
};
