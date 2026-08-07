import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from './api';
import { usersQueryKeys } from './keys';

export const useStudentProfile = () => {
  return useQuery({
    queryKey: usersQueryKeys.studentProfile(),
    queryFn: usersApi.getStudentProfile,
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
    mutationFn: ({ userId, role }: { userId: number; role: string }) =>
      usersApi.assignUserRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: usersQueryKeys.all });
    },
  });
};
