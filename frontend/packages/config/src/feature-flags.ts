export interface FeatureFlags {
  judgeEnabled: boolean;
  paymentsEnabled: boolean;
  certificatesEnabled: boolean;
  liveSessionsEnabled: boolean;
  pwaEnabled: boolean;
  analyticsEnabled: boolean;
}

export const FEATURE_FLAGS: FeatureFlags = {
  judgeEnabled: process.env.NEXT_PUBLIC_FLAG_JUDGE !== 'false',
  paymentsEnabled: process.env.NEXT_PUBLIC_FLAG_PAYMENTS !== 'false',
  certificatesEnabled: process.env.NEXT_PUBLIC_FLAG_CERTS === 'true',
  liveSessionsEnabled: process.env.NEXT_PUBLIC_FLAG_LIVE !== 'false',
  pwaEnabled: process.env.NEXT_PUBLIC_FLAG_PWA !== 'false',
  analyticsEnabled: process.env.NEXT_PUBLIC_FLAG_ANALYTICS !== 'false',
};
