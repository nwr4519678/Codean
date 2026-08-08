import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from './api';
import { usersQueryKeys } from './keys';

export const useStudentProfile = () => {
  return useQuery({
    queryKey: usersQueryKeys.studentProfile(),
    queryFn: usersApi.getStudentProfile,
  });
};

export const useUpdateStudentProfile = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: usersApi.updateStudentProfile,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersQueryKeys.studentProfile() }),
  });
};

export const useTeacherProfile = () => {
  return useQuery({
    queryKey: usersQueryKeys.teacherProfile(),
    queryFn: usersApi.getTeacherProfile,
  });
};

export const useAdminUsers = (params?: { pageNumber?: number; pageSize?: number; search?: string }) => {
  return useQuery({
    queryKey: usersQueryKeys.list(params),
    queryFn: () => usersApi.getUsersPaged(params),
  });
};

export const useAssignRole = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roleId }: { userId: number; roleId: number }) =>
      usersApi.assignUserRole(userId, roleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: usersQueryKeys.all });
    },
  });
};
