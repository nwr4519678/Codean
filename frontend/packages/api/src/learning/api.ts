import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import type { CourseCheckoutResponse, CourseEnrollmentResponse } from '@platform/contracts';

export const learningApi = {
  getCourseProgress: async (courseId: number) => {
    const res = await apiClient.get(API_URLS.COURSES.COURSE_PROGRESS(courseId));
    return res.data;
  },

  trackLessonProgress: async (payload: {
    lessonId: number;
    watchTime: number;
    completion: number;
  }) => {
    const res = await apiClient.post(API_URLS.COURSES.PROGRESS, payload);
    return res.data;
  },

  getEnrolledCourses: async (): Promise<CourseEnrollmentResponse[]> => {
    const res = await apiClient.get<CourseEnrollmentResponse[]>(API_URLS.COURSES.ENROLLED);
    return res.data;
  },

  enrollCourse: async (courseId: number): Promise<CourseEnrollmentResponse> => {
    const res = await apiClient.post<CourseEnrollmentResponse>(`${API_URLS.COURSES.DETAIL(courseId)}/enroll`);
    return res.data;
  },

  initiateCourseCheckout: async (courseId: number): Promise<CourseCheckoutResponse> => {
    const res = await apiClient.post<CourseCheckoutResponse>(API_URLS.COURSES.CHECKOUT(courseId));
    return res.data;
  },

  getExam: async (examId: number) => {
    const res = await apiClient.get(`/api/exams/${examId}`);
    return res.data;
  },

  submitExam: async (examId: number, answers: Record<number, string>) => {
    const res = await apiClient.post(`/api/exams/${examId}/submit`, { answers });
    return res.data;
  },
};
