import { apiClient } from '../client';
import { HomeworkResponse, HomeworkSubmissionResponse, PagedList } from '@platform/contracts';

export const homeworkApi = {
  list: async (params?: {
    courseId?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PagedList<HomeworkResponse>> => {
    const res = await apiClient.get<PagedList<HomeworkResponse>>('/api/homeworks', { params });
    return res.data;
  },

  getById: async (id: number | string): Promise<HomeworkResponse> => {
    const res = await apiClient.get<HomeworkResponse>(`/api/homeworks/${id}`);
    return res.data;
  },

  create: async (payload: {
    courseId?: number;
    title: string;
    description: string;
    dueDate?: string;
    totalMarks: number;
  }): Promise<HomeworkResponse> => {
    const res = await apiClient.post<HomeworkResponse>('/api/homeworks', payload);
    return res.data;
  },

  submit: async (
    homeworkId: number | string,
    payload: { submissionType: 'File' | 'Text'; fileUrl?: string; textAnswer?: string }
  ): Promise<HomeworkSubmissionResponse> => {
    const res = await apiClient.post<HomeworkSubmissionResponse>(
      `/api/homeworks/${homeworkId}/submit`,
      payload
    );
    return res.data;
  },

  getSubmissions: async (
    homeworkId: number | string
  ): Promise<HomeworkSubmissionResponse[]> => {
    const res = await apiClient.get<HomeworkSubmissionResponse[]>(
      `/api/homeworks/${homeworkId}/submissions`
    );
    return res.data;
  },

  gradeSubmission: async (
    submissionId: number | string,
    payload: { grade: number; feedback: string }
  ): Promise<void> => {
    await apiClient.post(`/api/homeworks/submissions/${submissionId}/grade`, payload);
  },
};
