import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import {
  CodingChallengeResponse,
  SubmitCodeResponse,
  CodeSubmissionResultResponse,
} from '@platform/contracts';

export const judgeApi = {
  getChallenges: async (): Promise<CodingChallengeResponse[]> => {
    const res = await apiClient.get<CodingChallengeResponse[]>(API_URLS.JUDGE.CHALLENGES);
    return res.data;
  },

  getChallengeById: async (id: number | string): Promise<CodingChallengeResponse> => {
    const res = await apiClient.get<CodingChallengeResponse>(API_URLS.JUDGE.CHALLENGE_DETAIL(id));
    return res.data;
  },

  submitCode: async (payload: { challengeId: number; sourceCode: string; language: string }): Promise<SubmitCodeResponse> => {
    const res = await apiClient.post<SubmitCodeResponse>(API_URLS.JUDGE.SUBMIT, payload);
    return res.data;
  },

  getSubmissionStatus: async (submissionId: number | string): Promise<CodeSubmissionResultResponse> => {
    const res = await apiClient.get<CodeSubmissionResultResponse>(API_URLS.JUDGE.SUBMISSION_STATUS(submissionId));
    return res.data;
  },
};
