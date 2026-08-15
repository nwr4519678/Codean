import { apiClient, clearAuthTokens } from '../client';
import { supabaseLogout, supabasePasswordLogin, supabasePasswordSignup, supabaseRecoverPassword, getSupabaseAccessToken, supabaseUpdatePassword } from './supabase';
import { API_URLS } from '@platform/config';
import {
  LoginResponse,
  RegisterResponse,
  CurrentUserResponse,
  SetupTwoFactorResponse,
  SessionResponse,
} from '@platform/contracts';

export const authApi = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    await supabasePasswordLogin(email, password);
    const res = await apiClient.get<LoginResponse>(API_URLS.AUTH.ME);
    return { ...res.data, accessToken: getSupabaseAccessToken() ?? "", refreshToken: "", accessTokenExpiresAt: "", refreshTokenExpiresAt: "" };
  },

  register: async (payload: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    role?: string;
  }): Promise<RegisterResponse> => {
    const fullName = `${payload.firstName} ${payload.lastName}`.trim();
    const session = await supabasePasswordSignup(payload.email, payload.password, fullName);
    if (session.access_token) {
      await apiClient.post("/api/auth/sync-profile", { fullName });
    }
    return { userId: 0, email, fullName, emailVerificationRequired: !session.user?.email_confirmed_at };
  },

  getCurrentUser: async (): Promise<CurrentUserResponse> => {
    const res = await apiClient.get<CurrentUserResponse>(API_URLS.AUTH.ME);
    return res.data;
  },

  logout: async (_refreshToken: string): Promise<void> => {
    await supabaseLogout();
    clearAuthTokens();
  },

  forgotPassword: async (email: string): Promise<void> => {
    await supabaseRecoverPassword(email);
  },

  resetPassword: async (token: string, newPassword: string): Promise<void> => {
    const accessToken = token || getSupabaseAccessToken();
    if (!accessToken) throw new Error("Your password reset session has expired. Please request a new link.");
    await supabaseUpdatePassword(accessToken, newPassword);
  },

  verifyEmail: async (token: string): Promise<void> => {
    if (!getSupabaseAccessToken() && token) return;
  },

  setup2FA: async (): Promise<SetupTwoFactorResponse> => {
    const res = await apiClient.post<SetupTwoFactorResponse>(API_URLS.AUTH.TWO_FACTOR_SETUP);
    return res.data;
  },

  verify2FA: async (code: string): Promise<void> => {
    await apiClient.post(API_URLS.AUTH.TWO_FACTOR_VERIFY, { code });
  },

  getSessions: async (): Promise<SessionResponse[]> => {
    const res = await apiClient.get<SessionResponse[]>(API_URLS.AUTH.SESSIONS);
    return res.data;
  },

  revokeSession: async (sessionId: number): Promise<void> => {
    await apiClient.delete(API_URLS.AUTH.REVOKE_SESSION(sessionId));
  },
};
