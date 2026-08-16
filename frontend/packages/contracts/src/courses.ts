export interface LessonResponse {
  id: number;
  moduleId: number;
  title: string;
  description?: string;
  videoUrl?: string;
  duration?: number;
  durationSeconds?: number;
  order: number;
  isFreePreview?: boolean;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  resources: LessonResourceResponse[];
  isCompleted?: boolean;
}

export interface CourseModuleResponse {
  id: number;
  courseId: number;
  title: string;
  monthNumber: number;
  description?: string;
  order: number;
  createdAt: string;
  lessons: LessonResponse[];
}

export interface CourseResponse {
  id: number;
  teacherId: number;
  teacherName: string;
  title: string;
  description: string;
  thumbnail?: string;
  category: string;
  price: number;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  moduleCount: number;
  lessonCount: number;
  enrollmentCount?: number;
  rating?: number;
}

export interface CourseDetailResponse {
  id: number;
  teacherId: number;
  teacherName: string;
  title: string;
  description: string;
  thumbnail?: string;
  category: string;
  price: number;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  modules: CourseModuleResponse[];
}

export interface CourseEnrollmentResponse {
  id: number;
  courseId: number;
  courseTitle: string;
  courseThumbnail?: string;
  category: string;
  teacherName: string;
  price: number;
  status: "Active" | "PendingPayment" | "Cancelled" | "Completed" | string;
  accessType: "Free" | "Paid" | string;
  enrolledAt: string;
  completedAt?: string;
  progress?: CourseProgressResponse;
}

export interface StudentProgressResponse {
  id: number;
  studentId: number;
  lessonId: number;
  completion: number;
  lastViewed?: string;
  watchTime: number;
  watchTimeSeconds?: number;
  completionPercentage?: number;
  isCompleted?: boolean;
  lastAccessedAt?: string;
}

export interface CourseProgressResponse {
  courseId: number;
  totalLessons: number;
  completedLessons: number;
  overallCompletionPercentage: number;
  lessonProgress: StudentProgressResponse[];
}

export interface LessonResourceResponse {
  id: number;
  lessonId: number;
  resourceType: string;
  fileUrl: string;
  fileName: string;
  fileSizeBytes?: number;
  createdAt: string;
}

export interface PagedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
