import { getApi } from "./client";

export interface Paged<T> {
  data: T[];
  pagination: { nextCursor?: string; hasMore: boolean };
}

export interface CourseDto {
  id: string;
  title: string;
  slug: string;
  subtitle?: string;
  description?: string;
  coverUrl?: string;
  language: string;
  level: "Beginner" | "Intermediate" | "Advanced";
  status: "Draft" | "Published" | "Archived";
  tags: string[];
  ratingAvg: number;
  ratingCount: number;
  enrollmentCount: number;
  teacherId: string;
  publishedAt?: string;
}

export interface MonthlyPackageDto {
  id: string;
  courseId: string;
  monthNumber: number;
  title: string;
  price: number;
  currency: string;
  accessDurationDays: number;
  status: "Draft" | "Active" | "Archived";
}

export interface LessonDto {
  id: string;
  moduleId: string;
  title: string;
  order: number;
  kind: "Reading" | "Video" | "Coding" | "Quiz" | "Assignment" | "Mixed";
  visibility: "Free" | "Subscribed" | "CourseSpecific" | "GroupSpecific";
  estimatedMinutes?: number;
}

export interface LiveSessionDto {
  id: string;
  courseId: string;
  title: string;
  startAt: string;
  endAt: string;
  status: "Scheduled" | "Live" | "Ended" | "Cancelled";
  provider: "GoogleMeet" | "MicrosoftTeams";
}

export interface ProblemDto {
  id: string;
  title: string;
  slug: string;
  statementMd: string;
  languages: string[];
  timeLimitMs: number;
  memoryLimitMb: number;
}

export interface UserMe {
  id: string;
  email: string;
  emailVerified: boolean;
  twoFactorEnabled: boolean;
  locale?: string;
  timezone?: string;
  roles: string[];
}

export const api = {
  // ---- Auth ----
  auth: {
    me: () => getApi().get<UserMe>("/api/v1/users/me"),
    login: (email: string, password: string) =>
      getApi().post<{ accessToken: string; refreshToken: string; accessTokenExpiresAt: string }>(
        "/api/v1/auth/login",
        { email, password }
      ),
    register: (email: string, password: string) =>
      getApi().post<{ accessToken: string; refreshToken: string; accessTokenExpiresAt: string }>(
        "/api/v1/auth/register",
        { email, password }
      ),
    logout: () => getApi().post("/api/v1/auth/logout"),
  },

  // ---- Courses ----
  courses: {
    list: (params?: { q?: string; level?: string; tag?: string; cursor?: string }) =>
      getApi().get<Paged<CourseDto>>("/api/v1/courses", { params }),
    get: (id: string) => getApi().get<CourseDto>(`/api/v1/courses/${id}`),
    create: (data: Partial<CourseDto>) => getApi().post<CourseDto>("/api/v1/courses", data),
    enroll: (id: string) => getApi().post(`/api/v1/courses/${id}/enroll`),
  },

  // ---- Monthly packages / payments ----
  packages: {
    listForCourse: (courseId: string) =>
      getApi().get<MonthlyPackageDto[]>(`/api/v1/courses/${courseId}/packages`),
    createOrder: (packageId: string, couponCode?: string) =>
      getApi().post<{ orderId: string; checkoutUrl: string }>("/api/v1/payments/orders", { packageId, couponCode }),
  },

  // ---- Lessons / live ----
  lessons: {
    listForModule: (courseId: string, moduleId: string) =>
      getApi().get<LessonDto[]>(`/api/v1/courses/${courseId}/modules/${moduleId}/lessons`),
    get: (courseId: string, lessonId: string) =>
      getApi().get<LessonDto>(`/api/v1/courses/${courseId}/lessons/${lessonId}`),
  },
  live: {
    upcoming: () => getApi().get<LiveSessionDto[]>("/api/v1/live/sessions?upcoming=true"),
    join: (sessionId: string) => getApi().post<{ joinUrl: string; expiresAt: string }>(`/api/v1/live/sessions/${sessionId}/join`),
  },

  // ---- Coding ----
  problems: {
    list: (params?: { language?: string; tag?: string }) =>
      getApi().get<Paged<ProblemDto>>("/api/v1/coding/problems", { params }),
    submit: (problemId: string, language: string, source: string) =>
      getApi().post<{ id: string }>("/api/v1/coding/submissions", { problemId, language, source }),
  },
};
