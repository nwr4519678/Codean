export interface LessonResponse {
  id: number;
  moduleId: number;
  title: string;
  description?: string;
  videoUrl?: string;
  durationSeconds: number;
  order: number;
  isFreePreview: boolean;
  isCompleted?: boolean;
}

export interface CourseModuleResponse {
  id: number;
  courseId: number;
  title: string;
  description?: string;
  order: number;
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

export interface StudentProgressResponse {
  lessonId: number;
  watchTimeSeconds: number;
  completionPercentage: number;
  isCompleted: boolean;
  lastAccessedAt: string;
}

export interface PagedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
