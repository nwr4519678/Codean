import axios, { type AxiosInstance, type AxiosRequestConfig } from "axios";

/**
 * Configured Axios client for the Platform API.
 *
 * - Reads its base URL from `NEXT_PUBLIC_API_URL`.
 * - Attaches the JWT access token (if any) from local storage on every request.
 * - Automatically refreshes expired access tokens via the /auth/refresh endpoint.
 * - Surfaces RFC 7807 problem+json errors as typed <ApiError>s.
 */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly code: string,
    message: string,
    public readonly details?: Record<string, unknown>,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

interface Tokens {
  accessToken: string;
  refreshToken: string;
  accessExpiresAt: string;
}

const ACCESS_KEY = "platform.access";
const REFRESH_KEY = "platform.refresh";
const ACCESS_EXP_KEY = "platform.accessExp";

export class ApiClient {
  private axios: AxiosInstance;
  private refreshing: Promise<void> | null = null;

  constructor(baseURL: string) {
    this.axios = axios.create({ baseURL, withCredentials: false });
    this.axios.interceptors.request.use((cfg) => {
      const access = localStorage.getItem(ACCESS_KEY);
      if (access) cfg.headers.Authorization = `Bearer ${access}`;
      return cfg;
    });

    this.axios.interceptors.response.use(
      (r) => r,
      async (err) => {
        const original = err.config as AxiosRequestConfig & { _retry?: boolean };
        if (err.response?.status === 401 && !original._retry) {
          original._retry = true;
          await this.tryRefresh();
          const access = localStorage.getItem(ACCESS_KEY);
          if (access) original.headers = { ...(original.headers ?? {}), Authorization: `Bearer ${access}` };
          return this.axios.request(original);
        }
        throw this.toApiError(err);
      }
    );
  }

  private toApiError(err: unknown): ApiError {
    const ax = err as { response?: { status: number; data: { code?: string; title?: string; detail?: string; errors?: Record<string, unknown> } } };
    if (ax.response) {
      const { status, data } = ax.response;
      return new ApiError(status, data.code ?? "unknown", data.title ?? data.detail ?? "Request failed.", data.errors);
    }
    return new ApiError(0, "network", "Network error.");
  }

  private async tryRefresh(): Promise<void> {
    if (this.refreshing) return this.refreshing;
    const refresh = localStorage.getItem(REFRESH_KEY);
    if (!refresh) return;
    this.refreshing = (async () => {
      try {
        const { data } = await this.axios.post<{ accessToken: string; refreshToken: string; accessTokenExpiresAt: string }>(
          "/api/v1/auth/refresh",
          { refreshToken: refresh }
        );
        localStorage.setItem(ACCESS_KEY, data.accessToken);
        localStorage.setItem(REFRESH_KEY, data.refreshToken);
        localStorage.setItem(ACCESS_EXP_KEY, data.accessTokenExpiresAt);
      } catch {
        localStorage.removeItem(ACCESS_KEY);
        localStorage.removeItem(REFRESH_KEY);
      }
    })();
    await this.refreshing;
    this.refreshing = null;
  }

  setTokens(t: Tokens) {
    localStorage.setItem(ACCESS_KEY, t.accessToken);
    localStorage.setItem(REFRESH_KEY, t.refreshToken);
    localStorage.setItem(ACCESS_EXP_KEY, t.accessExpiresAt);
  }

  clearTokens() {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(ACCESS_EXP_KEY);
  }

  get<T>(url: string, config?: AxiosRequestConfig) {
    return this.axios.get<T>(url, config).then((r) => r.data);
  }
  post<T>(url: string, body?: unknown, config?: AxiosRequestConfig) {
    return this.axios.post<T>(url, body, config).then((r) => r.data);
  }
  patch<T>(url: string, body?: unknown, config?: AxiosRequestConfig) {
    return this.axios.patch<T>(url, body, config).then((r) => r.data);
  }
  delete<T>(url: string, config?: AxiosRequestConfig) {
    return this.axios.delete<T>(url, config).then((r) => r.data);
  }
}

let _instance: ApiClient | null = null;
export function getApi(): ApiClient {
  if (!_instance) {
    const base = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";
    _instance = new ApiClient(base);
  }
  return _instance;
}
