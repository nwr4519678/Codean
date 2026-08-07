import { UserRole } from './enums';

export interface UserDto {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  avatarUrl?: string;
  emailConfirmed: boolean;
  twoFactorEnabled: boolean;
  createdAt: string;
}

export interface CurrentUserResponse {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  avatarUrl?: string;
  emailConfirmed: boolean;
  twoFactorEnabled: boolean;
  roles: string[];
  permissions: string[];
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: CurrentUserResponse;
}

export interface RegisterResponse {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  requiresEmailVerification: boolean;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface SetupTwoFactorResponse {
  secret: string;
  qrCodeUri: string;
  backupCodes: string[];
}

export interface SessionResponse {
  id: number;
  ipAddress?: string;
  userAgent?: string;
  createdAt: string;
  expiresAt: string;
  isCurrent: boolean;
}
