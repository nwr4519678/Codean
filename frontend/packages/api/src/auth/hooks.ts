import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authApi } from './api';
import { authQueryKeys } from './keys';

export const useCurrentUser = () => {
  return useQuery({
    queryKey: authQueryKeys.currentUser(),
    queryFn: authApi.getCurrentUser,
    retry: false,
    staleTime: 5 * 60 * 1000,
  });
};

export const useLogin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ email, password }: { email: string; password: string }) =>
      authApi.login(email, password),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authQueryKeys.currentUser() });
    },
  });
};

export const useRegister = () => {
  return useMutation({
    mutationFn: authApi.register,
  });
};

export const useLogout = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (refreshToken: string) => authApi.logout(refreshToken),
    onSuccess: () => {
      queryClient.clear();
    },
  });
};

export const useSessions = () => {
  return useQuery({
    queryKey: authQueryKeys.sessions(),
    queryFn: authApi.getSessions,
  });
};
