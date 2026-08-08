export interface ExamResponse {
  id: number;
  teacherId: number;
  courseId?: number;
  title: string;
  description: string;
  durationMinutes: number;
  totalMarks: number;
  passingMarks?: number;
  startDate?: string;
  endDate?: string;
  isPublished: boolean;
  questionCount: number;
}

export interface ExamQuestionResponse {
  id: number;
  examId: number;
  questionId: number;
  order: number;
  marks: number;
  body: string;
  questionType: 'MCQ' | 'TrueFalse' | 'Essay' | 'Programming';
  choices?: ExamChoiceResponse[];
  difficulty: 'Easy' | 'Medium' | 'Hard';
}

export interface ExamChoiceResponse {
  id: number;
  choiceText: string;
  isCorrect: boolean;
  order: number;
}

export interface ExamAttemptResponse {
  id: number;
  examId: number;
  studentId: number;
  startedAt: string;
  submittedAt?: string;
  status: 'InProgress' | 'Submitted' | 'Graded';
  score?: number;
  totalMarks: number;
  passingMarks?: number;
  answers?: ExamAnswerResponse[];
}

export interface ExamAnswerResponse {
  id: number;
  examAttemptId: number;
  questionId: number;
  answerText?: string;
  selectedChoiceIds?: number[];
  isCorrect?: boolean;
  marksObtained?: number;
}

export interface ExamResultResponse {
  id: number;
  examAttemptId: number;
  studentId: number;
  examId: number;
  totalScore: number;
  percentage: number;
  grade: string;
  passedStatus: boolean;
  generatedAt: string;
}

export interface HomeworkResponse {
  id: number;
  teacherId: number;
  courseId?: number;
  lessonId?: number;
  title: string;
  description: string;
  dueDate?: string;
  totalMarks: number;
  questionCount: number;
}

export interface HomeworkSubmissionResponse {
  id: number;
  homeworkId: number;
  studentId: number;
  submissionType: 'File' | 'Text';
  fileUrl?: string;
  textAnswer?: string;
  submittedAt: string;
  grade?: number;
  feedback?: string;
  status: 'Pending' | 'Submitted' | 'Graded' | 'Late';
}
