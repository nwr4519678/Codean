import { apiClient } from '../client';
import {
  ExamResponse,
  ExamQuestionResponse,
  ExamAttemptResponse,
  ExamResultResponse,
  PagedList,
} from '@platform/contracts';

export const examsApi = {
  list: async (params?: {
    courseId?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PagedList<ExamResponse>> => {
    const res = await apiClient.get<PagedList<ExamResponse>>('/api/exams', { params });
    return res.data;
  },

  getById: async (id: number | string): Promise<ExamResponse & { questions?: ExamQuestionResponse[] }> => {
    const res = await apiClient.get(`/api/exams/${id}`);
    return res.data;
  },

  create: async (payload: {
    courseId?: number;
    title: string;
    description: string;
    durationMinutes: number;
    totalMarks: number;
    passingMarks?: number;
  }): Promise<ExamResponse> => {
    const res = await apiClient.post<ExamResponse>('/api/exams', payload);
    return res.data;
  },

  publish: async (id: number | string): Promise<void> => {
    await apiClient.post(`/api/exams/${id}/publish`);
  },

  startAttempt: async (examId: number | string): Promise<ExamAttemptResponse> => {
    const res = await apiClient.post<ExamAttemptResponse>(`/api/exam-attempts/start/${examId}`);
    return res.data;
  },

  submitAttempt: async (
    attemptId: number | string,
    answers: Array<{ questionId: number; answerText?: string; selectedChoiceIds?: number[] }>
  ): Promise<ExamResultResponse> => {
    const res = await apiClient.post<ExamResultResponse>(`/api/exam-attempts/${attemptId}/submit`, {
      answers,
    });
    return res.data;
  },

  getAttempt: async (attemptId: number | string): Promise<ExamAttemptResponse> => {
    const res = await apiClient.get<ExamAttemptResponse>(`/api/exam-attempts/${attemptId}`);
    return res.data;
  },
};
