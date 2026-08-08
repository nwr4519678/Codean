import axios, { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import { API_BASE_URL, API_URLS } from '@platform/config';
import { RefreshTokenResponse } from '@platform/contracts';

const ACCESS_TOKEN_KEY  = 'platform_access_token';
const REFRESH_TOKEN_KEY = 'platform_refresh_token';
const TOKEN_EXPIRY_KEY  = 'platform_token_expiry'; // epoch ms

// ── Token Storage ────────────────────────────────────────────────────────────

export const getStoredAccessToken = (): string | null => {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(ACCESS_TOKEN_KEY);
};

export const getStoredRefreshToken = (): string | null => {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(REFRESH_TOKEN_KEY);
};

export const setAuthTokens = (accessToken: string, refreshToken: string) => {
  if (typeof window === 'undefined') return;
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);

  // Decode expiry from JWT payload (no library needed)
  try {
    const payload = JSON.parse(atob(accessToken.split('.')[1]));
    if (payload.exp) {
      localStorage.setItem(TOKEN_EXPIRY_KEY, String(payload.exp * 1000)); // convert s → ms
    }
  } catch {
    // ignore malformed tokens
  }

  scheduleProactiveRefresh();
};

export const clearAuthTokens = () => {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(TOKEN_EXPIRY_KEY);
  if (_refreshTimer !== null) {
    clearTimeout(_refreshTimer);
    _refreshTimer = null;
  }
};

// ── Proactive Refresh (fires 90s before token expiry) ────────────────────────

let _refreshTimer: ReturnType<typeof setTimeout> | null = null;

function scheduleProactiveRefresh() {
  if (typeof window === 'undefined') return;
  if (_refreshTimer !== null) clearTimeout(_refreshTimer);

  const expiryStr = localStorage.getItem(TOKEN_EXPIRY_KEY);
  if (!expiryStr) return;

  const expiryMs = Number(expiryStr);
  const nowMs    = Date.now();
  const delayMs  = expiryMs - nowMs - 90_000; // refresh 90s early

  if (delayMs <= 0) {
    // Already near/past expiry — refresh immediately
    void doProactiveRefresh();
    return;
  }

  _refreshTimer = setTimeout(() => void doProactiveRefresh(), delayMs);
}

async function doProactiveRefresh() {
  const refreshToken = getStoredRefreshToken();
  if (!refreshToken) return;

  try {
    const res = await axios.post<RefreshTokenResponse>(
      `${API_BASE_URL}${API_URLS.AUTH.REFRESH_TOKEN}`,
      { token: refreshToken },
    );
    const { accessToken, refreshToken: newRefreshToken } = res.data;
    setAuthTokens(accessToken, newRefreshToken);
    apiClient.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
  } catch {
    // Proactive refresh failed — let 401 interceptor handle it later
  }
}

// Re-schedule on page load (handles page refreshes mid-session)
if (typeof window !== 'undefined') {
  scheduleProactiveRefresh();
}

// ── Axios Client ─────────────────────────────────────────────────────────────

export const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000,
});

// Request interceptor: attach Bearer token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = getStoredAccessToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

// ── Response interceptor: silent token refresh on 401 ────────────────────────

let isRefreshing = false;
let failedQueue: Array<{ resolve: (token: string) => void; reject: (error: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else if (token) {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest  = error.config;
    const retryableRequest = originalRequest as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;

    if (
      !retryableRequest ||
      error.response?.status !== 401 ||
      retryableRequest._retry ||
      retryableRequest.url === API_URLS.AUTH.REFRESH_TOKEN
    ) {
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then((token) => {
          retryableRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient(retryableRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    retryableRequest._retry = true;
    isRefreshing = true;

    const refreshToken = getStoredRefreshToken();
    if (!refreshToken) {
      clearAuthTokens();
      isRefreshing = false;
      return Promise.reject(error);
    }

    try {
      const response = await axios.post<RefreshTokenResponse>(
        `${API_BASE_URL}${API_URLS.AUTH.REFRESH_TOKEN}`,
        { token: refreshToken },
      );
      const { accessToken, refreshToken: newRefreshToken } = response.data;
      setAuthTokens(accessToken, newRefreshToken);

      apiClient.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
      processQueue(null, accessToken);
      isRefreshing = false;

      retryableRequest.headers.Authorization = `Bearer ${accessToken}`;
      return apiClient(retryableRequest);
    } catch (refreshErr) {
      processQueue(refreshErr, null);
      clearAuthTokens();
      isRefreshing = false;
      return Promise.reject(refreshErr);
    }
  },
);
