import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { homeworkApi } from './api';
import { homeworkQueryKeys } from './keys';

export const useHomeworkList = (params?: { courseId?: number; pageNumber?: number; pageSize?: number }) =>
  useQuery({
    queryKey: homeworkQueryKeys.list(params),
    queryFn: () => homeworkApi.list(params),
  });

export const useCreateHomework = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: homeworkApi.create,
    onSuccess: () => qc.invalidateQueries({ queryKey: homeworkQueryKeys.all }),
  });
};

export const useSubmitHomework = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      homeworkId,
      payload,
    }: {
      homeworkId: number | string;
      payload: { submissionType: 'File' | 'Text'; fileUrl?: string; textAnswer?: string };
    }) => homeworkApi.submit(homeworkId, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: homeworkQueryKeys.all }),
  });
};

export const useHomeworkSubmissions = (homeworkId: number | string) =>
  useQuery({
    queryKey: homeworkQueryKeys.submissions(homeworkId),
    queryFn: () => homeworkApi.getSubmissions(homeworkId),
    enabled: !!homeworkId,
  });

export const useGradeSubmission = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      submissionId,
      payload,
    }: {
      submissionId: number | string;
      payload: { grade: number; feedback: string };
    }) => homeworkApi.gradeSubmission(submissionId, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: homeworkQueryKeys.all }),
  });
};
