import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { learningApi } from './api';
import { learningKeys } from './keys';

export const useEnrolledCourses = () =>
  useQuery({
    queryKey: learningKeys.enrolled(),
    queryFn: () => learningApi.getEnrolledCourses(),
  });

export const useLearningProgress = (courseId: number) =>
  useQuery({
    queryKey: learningKeys.progress(courseId),
    queryFn: () => learningApi.getCourseProgress(courseId),
    enabled: !!courseId,
  });

export const useTrackLessonProgress = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: learningApi.trackLessonProgress,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: learningKeys.enrolled() });
    },
  });
};

export const useEnrollCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (courseId: number) => learningApi.enrollCourse(courseId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: learningKeys.enrolled() });
    },
  });
};

export const useExam = (examId: number) =>
  useQuery({
    queryKey: learningKeys.exam(examId),
    queryFn: () => learningApi.getExam(examId),
    enabled: !!examId,
  });
