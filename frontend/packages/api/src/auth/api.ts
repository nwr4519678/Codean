import { apiClient, setAuthTokens, clearAuthTokens } from '../client';
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
    const res = await apiClient.post<LoginResponse>(API_URLS.AUTH.LOGIN, { email, password });
    setAuthTokens(res.data.accessToken, res.data.refreshToken);
    return res.data;
  },

  register: async (payload: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    role?: string;
  }): Promise<RegisterResponse> => {
    const res = await apiClient.post<RegisterResponse>(API_URLS.AUTH.REGISTER, {
      fullName: `${payload.firstName} ${payload.lastName}`.trim(),
      email: payload.email,
      password: payload.password,
      role: payload.role,
    });
    return res.data;
  },

  getCurrentUser: async (): Promise<CurrentUserResponse> => {
    const res = await apiClient.get<CurrentUserResponse>(API_URLS.AUTH.ME);
    return res.data;
  },

  logout: async (refreshToken: string): Promise<void> => {
    try {
      await apiClient.post(API_URLS.AUTH.REVOKE_TOKEN, { refreshToken });
    } finally {
      clearAuthTokens();
    }
  },

  forgotPassword: async (email: string): Promise<void> => {
    await apiClient.post(API_URLS.AUTH.FORGOT_PASSWORD, { email });
  },

  resetPassword: async (token: string, newPassword: string): Promise<void> => {
    await apiClient.post(API_URLS.AUTH.RESET_PASSWORD, { token, newPassword });
  },

  verifyEmail: async (token: string): Promise<void> => {
    await apiClient.post(API_URLS.AUTH.VERIFY_EMAIL, { token });
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
