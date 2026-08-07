import { useQuery, useMutation } from '@tanstack/react-query';
import { judgeApi } from './api';
import { judgeQueryKeys } from './keys';

export const useCodingChallenges = () => {
  return useQuery({
    queryKey: judgeQueryKeys.challenges(),
    queryFn: judgeApi.getChallenges,
  });
};

export const useCodingChallengeDetail = (id: number | string) => {
  return useQuery({
    queryKey: judgeQueryKeys.challengeDetail(id),
    queryFn: () => judgeApi.getChallengeById(id),
    enabled: !!id,
  });
};

export const useSubmitCode = () => {
  return useMutation({
    mutationFn: judgeApi.submitCode,
  });
};

export const useSubmissionStatus = (submissionId: number | string | null) => {
  return useQuery({
    queryKey: judgeQueryKeys.submissionStatus(submissionId!),
    queryFn: () => judgeApi.getSubmissionStatus(submissionId!),
    enabled: !!submissionId,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status === 'Queued' || status === 'Running' ? 1500 : false;
    },
  });
};
