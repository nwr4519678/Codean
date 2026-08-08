export const BRAND = {
  name: 'Codean',
  tagline: 'Egyptian Baccalaureate Programming & AI',
  description:
    'Premium interactive platform for Egyptian Baccalaureate students to master Programming and Artificial Intelligence through structured lessons, interactive coding, and live classes.',
  copyright: (year: number) =>
    `© ${year} Codean — Egyptian Baccalaureate Programming & AI. All rights reserved.`,
  currency: 'EGP',
  locale: 'ar-EG',
  subjects: [
    { key: 'programming', label: 'Programming Fundamentals', color: 'primary' },
    { key: 'python', label: 'Python Programming', color: 'accent' },
    { key: 'ai', label: 'AI & Machine Learning', color: 'success' },
    { key: 'algorithms', label: 'Algorithms & Problem Solving', color: 'purple-500' },
    { key: 'web', label: 'Web Development', color: 'info' },
    { key: 'examPrep', label: 'Exam Preparation', color: 'destructive' },
  ] as const,
} as const;

export type SubjectKey = (typeof BRAND.subjects)[number]['key'];
