import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { examsApi } from './api';
import { examsQueryKeys } from './keys';

export const useExams = (params?: { courseId?: number; pageNumber?: number; pageSize?: number }) =>
  useQuery({
    queryKey: examsQueryKeys.list(params),
    queryFn: () => examsApi.list(params),
  });

export const useExamDetail = (id: number | string) =>
  useQuery({
    queryKey: examsQueryKeys.detail(id),
    queryFn: () => examsApi.getById(id),
    enabled: !!id,
  });

export const useCreateExam = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: examsApi.create,
    onSuccess: () => qc.invalidateQueries({ queryKey: examsQueryKeys.all }),
  });
};

export const usePublishExam = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: examsApi.publish,
    onSuccess: () => qc.invalidateQueries({ queryKey: examsQueryKeys.all }),
  });
};

export const useStartExamAttempt = () =>
  useMutation({
    mutationFn: examsApi.startAttempt,
  });

export const useSubmitExamAttempt = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      attemptId,
      answers,
    }: {
      attemptId: number | string;
      answers: Array<{ questionId: number; answerText?: string; selectedChoiceIds?: number[] }>;
    }) => examsApi.submitAttempt(attemptId, answers),
    onSuccess: () => qc.invalidateQueries({ queryKey: examsQueryKeys.all }),
  });
};

export const useExamAttempt = (attemptId: number | string) =>
  useQuery({
    queryKey: examsQueryKeys.attempt(attemptId),
    queryFn: () => examsApi.getAttempt(attemptId),
    enabled: !!attemptId,
  });
