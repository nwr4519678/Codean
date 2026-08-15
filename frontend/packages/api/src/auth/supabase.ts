export type SupabaseSession = {
  access_token: string;
  refresh_token: string;
  expires_in?: number;
  expires_at?: number;
  user: { id: string; email?: string; user_metadata?: Record<string, unknown>; email_confirmed_at?: string | null };
};

type RuntimeEnv = {
  VITE_SUPABASE_URL?: string;
  VITE_SUPABASE_PUBLISHABLE_KEY?: string;
};

declare global {
  // Injected by each Vite app's index.html so the shared API package works in both apps.
  var __PLATFORM_ENV__: RuntimeEnv | undefined;
}

const env = (): RuntimeEnv => globalThis.__PLATFORM_ENV__ ?? {};
const accessKey = "supabase_access_token";
const refreshKey = "supabase_refresh_token";

export function getSupabaseUrl(): string {
  return (env().VITE_SUPABASE_URL ?? "").replace(/\/$/, "");
}

function getPublishableKey(): string {
  return env().VITE_SUPABASE_PUBLISHABLE_KEY ?? "";
}

function assertConfigured() {
  if (!getSupabaseUrl() || !getPublishableKey()) {
    throw new Error("Supabase Auth is not configured. Set VITE_SUPABASE_URL and VITE_SUPABASE_PUBLISHABLE_KEY.");
  }
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  assertConfigured();
  const response = await fetch(`${getSupabaseUrl()}${path}`, {
    ...init,
    headers: {
      apikey: getPublishableKey(),
      "Content-Type": "application/json",
      ...(init.headers ?? {}),
    },
  });
  if (!response.ok) {
    const body = await response.json().catch(() => null) as { msg?: string; error_description?: string; message?: string } | null;
    throw new Error(body?.msg ?? body?.error_description ?? body?.message ?? `Supabase Auth request failed (${response.status}).`);
  }
  return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}

export function storeSupabaseSession(session: SupabaseSession | null) {
  if (typeof window === "undefined") return;
  if (!session) {
    localStorage.removeItem(accessKey);
    localStorage.removeItem(refreshKey);
    return;
  }
  localStorage.setItem(accessKey, session.access_token);
  localStorage.setItem(refreshKey, session.refresh_token);
}

export function storeSupabaseTokens(accessToken: string, refreshToken: string) {
  if (typeof window === "undefined") return;
  localStorage.setItem(accessKey, accessToken);
  localStorage.setItem(refreshKey, refreshToken);
}

export function hydrateSupabaseSessionFromUrl() {
  if (typeof window === "undefined" || !window.location.hash) return;
  const hash = new URLSearchParams(window.location.hash.slice(1));
  const accessToken = hash.get("access_token");
  const refreshToken = hash.get("refresh_token");
  if (!accessToken || !refreshToken) return;
  storeSupabaseTokens(accessToken, refreshToken);
  window.history.replaceState({}, document.title, `${window.location.pathname}${window.location.search}`);
}

export function getSupabaseAccessToken(): string | null {
  return typeof window === "undefined" ? null : localStorage.getItem(accessKey);
}

export function getSupabaseRefreshToken(): string | null {
  return typeof window === "undefined" ? null : localStorage.getItem(refreshKey);
}

export async function supabasePasswordLogin(email: string, password: string) {
  const session = await request<SupabaseSession>(`/auth/v1/token?grant_type=password`, {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });
  storeSupabaseSession(session);
  return session;
}

export async function supabasePasswordSignup(email: string, password: string, fullName: string) {
  const session = await request<SupabaseSession>("/auth/v1/signup", {
    method: "POST",
    body: JSON.stringify({ email, password, data: { full_name: fullName } }),
  });
  storeSupabaseSession(session);
  return session;
}

export async function supabaseRefreshSession() {
  const refreshToken = getSupabaseRefreshToken();
  if (!refreshToken) return null;
  const session = await request<SupabaseSession>(`/auth/v1/token?grant_type=refresh_token`, {
    method: "POST",
    body: JSON.stringify({ refresh_token: refreshToken }),
  });
  storeSupabaseSession(session);
  return session;
}

export async function supabaseLogout() {
  const token = getSupabaseAccessToken();
  if (token) {
    await request<void>("/auth/v1/logout", { method: "POST", headers: { Authorization: `Bearer ${token}` } }).catch(() => undefined);
  }
  storeSupabaseSession(null);
}

export async function supabaseRecoverPassword(email: string) {
  return request<void>("/auth/v1/recover", { method: "POST", body: JSON.stringify({ email }) });
}

export async function supabaseUpdatePassword(accessToken: string, password: string) {
  return request<void>("/auth/v1/user", { method: "PUT", headers: { Authorization: `Bearer ${accessToken}` }, body: JSON.stringify({ password }) });
}

export function supabaseOAuthUrl(provider: "google" | "azure", redirectTo: string) {
  if (!getSupabaseUrl()) return "#";
  return `${getSupabaseUrl()}/auth/v1/authorize?provider=${provider}&redirect_to=${encodeURIComponent(redirectTo)}`;
}
