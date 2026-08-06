export const Roles = {
  Support: "Support",
  Student: "Student",
  Teacher: "Teacher",
  TeacherAssistant: "TeacherAssistant",
  SchoolAdmin: "SchoolAdmin",
  PlatformAdmin: "PlatformAdmin",
  SuperAdmin: "SuperAdmin",
} as const;

export type Role = (typeof Roles)[keyof typeof Roles];

export function isStaff(role: Role): boolean {
  return role === Roles.Support || role === Roles.PlatformAdmin || role === Roles.SuperAdmin;
}
