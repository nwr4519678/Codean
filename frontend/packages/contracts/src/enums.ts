export enum UserRole {
  Admin = 'Admin',
  Teacher = 'Teacher',
  Student = 'Student'
}

export enum CourseStatus {
  Draft = 'Draft',
  Published = 'Published',
  Archived = 'Archived'
}

export enum SubmissionStatus {
  Queued = 'Queued',
  Running = 'Running',
  Passed = 'Passed',
  Failed = 'Failed',
  TimeLimitExceeded = 'TimeLimitExceeded',
  MemoryLimitExceeded = 'MemoryLimitExceeded',
  CompilationError = 'CompilationError',
  RuntimeError = 'RuntimeError'
}

export enum PaymentStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Failed = 'Failed',
  Refunded = 'Refunded'
}

export enum SubscriptionTier {
  Free = 'Free',
  Pro = 'Pro',
  Enterprise = 'Enterprise'
}

export enum AppLanguage {
  English = 'en',
  Arabic = 'ar'
}
