export interface LiveSessionResponse {
  id: number;
  teacherId: number;
  courseId?: number;
  moduleId?: number;
  title: string;
  meetingId: string;
  meetingLink: string;
  startTime: string;
  endTime?: string;
  recordingLink?: string;
  status: 'Scheduled' | 'Live' | 'Completed' | 'Cancelled';
  password?: string;
  provider?: string;
  providerName: string;
  createdAt: string;
}

export interface ModuleResponse {
  id: number;
  courseId: number;
  title: string;
  description?: string;
  monthNumber: number;
  order: number;
  lessonCount?: number;
}

export interface AnnouncementResponse {
  id: number;
  teacherId: number;
  courseId?: number;
  title: string;
  body: string;
  publishedAt: string;
  isPinned: boolean;
}

export interface CertificateResponse {
  id: number;
  studentId: number;
  courseId?: number;
  examId?: number;
  certificateNumber: string;
  issuedAt: string;
  fileUrl?: string;
  courseTitle?: string;
  studentName?: string;
}

export interface GamificationProfile {
  totalXP: number;
  level: number;
  currentLevelXP: number;
  xpToNextLevel: number;
  studyStreak: number;
  badges: BadgeResponse[];
  rank?: number;
}

export interface BadgeResponse {
  id: number;
  name: string;
  description: string;
  icon: string;
  earnedAt?: string;
}

export interface PaymentWithInvoiceResponse {
  id: number;
  orderId: string;
  amount: number;
  currency: string;
  status: string;
  paymentMethod?: string;
  paidAt?: string;
  createdAt: string;
  invoice?: {
    id: number;
    invoiceNumber: string;
    issuedAt: string;
    totalAmount: number;
    taxAmount: number;
    pdfUrl?: string;
  };
}
