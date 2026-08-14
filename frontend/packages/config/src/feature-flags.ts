export interface FeatureFlags {
  judgeEnabled: boolean;
  paymentsEnabled: boolean;
  certificatesEnabled: boolean;
  liveSessionsEnabled: boolean;
  pwaEnabled: boolean;
  analyticsEnabled: boolean;
}

const runtimeEnv = (globalThis as typeof globalThis & { __PLATFORM_ENV__?: Record<string, string | undefined> }).__PLATFORM_ENV__;

export const FEATURE_FLAGS: FeatureFlags = {
  judgeEnabled: runtimeEnv?.VITE_FLAG_JUDGE !== 'false',
  paymentsEnabled: runtimeEnv?.VITE_FLAG_PAYMENTS !== 'false',
  certificatesEnabled: runtimeEnv?.VITE_FLAG_CERTS === 'true',
  liveSessionsEnabled: runtimeEnv?.VITE_FLAG_LIVE !== 'false',
  pwaEnabled: runtimeEnv?.VITE_FLAG_PWA !== 'false',
  analyticsEnabled: runtimeEnv?.VITE_FLAG_ANALYTICS !== 'false',
};
