import { SubmissionStatus } from './enums';

export interface TestCaseDto {
  input: string;
  expectedOutput: string;
  isHidden?: boolean;
}

export interface CodingChallengeResponse {
  id: number;
  teacherId: number;
  title: string;
  description: string;
  language: string;
  difficulty: string;
  marks: number;
  createdAt: string;
  category?: string;
  points?: number;
  timeLimitSeconds?: number;
  memoryLimitMb?: number;
  starterCode: string;
  supportedLanguages?: string[];
  testCases?: TestCaseDto[];
}

export interface SubmitCodeResponse {
  submissionId: number;
  executionId: string;
  status: SubmissionStatus;
  submittedAt: string;
}

export interface CodeSubmissionResultResponse {
  submissionId: number;
  executionId: string;
  status: SubmissionStatus;
  score: number;
  memoryUsedMb?: number;
  timeTakenSeconds?: number;
  compilerOutput?: string;
  passedTestCases: number;
  totalTestCases: number;
  submittedAt: string;
}
