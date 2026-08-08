import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import {
  CourseResponse,
  CourseDetailResponse,
  PagedList,
  StudentProgressResponse,
} from '@platform/contracts';

export const coursesApi = {
  getCourses: async (params?: { pageNumber?: number; pageSize?: number; search?: string; category?: string; teacherId?: number; isPublished?: boolean }): Promise<PagedList<CourseResponse>> => {
    const res = await apiClient.get<PagedList<CourseResponse>>(API_URLS.COURSES.LIST, { params });
    return res.data;
  },

  getCourseById: async (id: number | string): Promise<CourseDetailResponse> => {
    const res = await apiClient.get<CourseDetailResponse>(API_URLS.COURSES.DETAIL(id));
    return res.data;
  },

  createCourse: async (payload: { title: string; description: string; category: string; price: number; thumbnail?: string }): Promise<CourseResponse> => {
    const res = await apiClient.post<CourseResponse>(API_URLS.COURSES.CREATE, payload);
    return res.data;
  },

  updateCourse: async (id: number | string, payload: { title: string; description: string; category: string; price: number; thumbnail?: string }): Promise<CourseResponse> => {
    const res = await apiClient.put<CourseResponse>(API_URLS.COURSES.UPDATE(id), payload);
    return res.data;
  },

  publishCourse: async (id: number | string): Promise<boolean> => {
    const res = await apiClient.post<boolean>(API_URLS.COURSES.PUBLISH(id));
    return res.data;
  },

  trackProgress: async (payload: { lessonId: number; watchTimeSeconds: number; completionPercentage: number }): Promise<StudentProgressResponse> => {
    const res = await apiClient.post<StudentProgressResponse>(API_URLS.COURSES.PROGRESS, payload);
    return res.data;
  },

  getCourseProgress: async (courseId: number | string): Promise<StudentProgressResponse[]> => {
    const res = await apiClient.get<StudentProgressResponse[]>(API_URLS.COURSES.COURSE_PROGRESS(courseId));
    return res.data;
  },
};
